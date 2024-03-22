using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AngeliA;

public class Sheet {




	#region --- VAR ---


	public bool NotEmpty => Sprites.Count > 0;
	public readonly List<AngeSprite> Sprites = new();
	public readonly List<SpriteGroup> Groups = new();
	public readonly List<Atlas> Atlas = new();
	public readonly Dictionary<int, AngeSprite> SpritePool = new();
	public readonly Dictionary<int, SpriteGroup> GroupPool = new();
	private readonly Dictionary<int, object> TexturePool = new();


	#endregion




	#region --- MSG ---


	public Sheet () { }

	public Sheet (List<AngeSprite> sprites, List<SpriteGroup> groups, List<Atlas> atlasInfo) => SetData(sprites, groups, atlasInfo);


	#endregion




	#region --- API ---


	public void SetData (List<AngeSprite> sprites, List<SpriteGroup> groups, List<Atlas> atlasInfo) {
		Sprites.Clear();
		Groups.Clear();
		Atlas.Clear();
		Sprites.AddRange(sprites);
		Groups.AddRange(groups);
		Atlas.AddRange(atlasInfo);
		ApplyExtraData();
	}

	public bool LoadFromDisk (string path, System.Action<System.Exception> exceptionHandler = null) {

		Clear();
		var bytes = Util.CompressedFileToByte(path, out int length);
		if (length == 0) return false;

		using var stream = new MemoryStream(bytes);
		using var reader = new BinaryReader(stream);

		// File Version
		int fileVersion = reader.ReadInt32();

		// Load Data
		switch (fileVersion) {
			case 0:
				LoadFromBinary_v0(reader, exceptionHandler);
				break;
			default:
				exceptionHandler?.Invoke(
					new System.Exception($"Can not handle sheet version {fileVersion}. Expect: version-0")
				);
				return false;
		}

		return true;
	}

	public void SaveToDisk (string path, System.Action<System.Exception> exceptionHandler = null) {
		using var stream = new MemoryStream(1024);
		using var writer = new BinaryWriter(stream);
		writer.Write((int)0); // File Version
		SaveToBinary_v0(writer, exceptionHandler);
		Util.ByteToCompressedFile(path, stream.GetBuffer(), (int)stream.Position);
	}

	public void Clear () {
		Sprites.Clear();
		Groups.Clear();
		Atlas.Clear();
		SpritePool.Clear();
		GroupPool.Clear();
	}

	public int GetSpriteIdFromAnimationFrame (SpriteGroup group, int localFrame, int loopStart = -1) {

		int len = group.Count;
		if (len == 0) return 0;

		// Fix Loopstart
		loopStart = loopStart < 0 ? group.LoopStart : loopStart;
		int loopStartTiming = GetTiming(SpritePool, group, loopStart.Clamp(0, len - 1));
		int frameLen = GetTiming(SpritePool, group, len);
		int totalFrame = localFrame < loopStartTiming ? frameLen : frameLen - loopStartTiming;
		int frameOffset = localFrame < loopStartTiming ? 0 : loopStartTiming;
		localFrame = ((localFrame - frameOffset) % totalFrame) + frameOffset;

		// Get Target Index
		return GetTimingID(SpritePool, group, localFrame);

		// Func
		static int GetTiming (Dictionary<int, AngeSprite> pool, SpriteGroup group, int spriteIndex) {
			int result = 0;
			for (int i = 0; i < spriteIndex; i++) {
				int id = group.SpriteIDs[i];
				if (!pool.TryGetValue(id, out var sprite)) continue;
				result += sprite.Duration;
			}
			return result;
		}
		static int GetTimingID (Dictionary<int, AngeSprite> pool, SpriteGroup group, int targetTiming) {
			int result = 0;
			int totalDuration = 0;
			for (int i = 0; i < group.Count; i++) {
				int id = result = group.SpriteIDs[i];
				if (!pool.TryGetValue(id, out var sprite)) continue;
				totalDuration += sprite.Duration;
				if (totalDuration >= targetTiming) break;
			}
			return result;
		}
	}

	// Find
	public int IndexOfSprite (int id) => Sprites.FindIndex(s => s.ID == id);

	public int IndexOfGroup (int id) => Groups.FindIndex(s => s.ID == id);

	public bool ContainSprite (int id) => SpritePool.ContainsKey(id);

	public bool ContainGroup (int id) => GroupPool.ContainsKey(id);

	// Add
	public bool AddSprite (AngeSprite sprite) {
		if (sprite.ID == 0 || SpritePool.ContainsKey(sprite.ID)) return false;
		Sprites.Add(sprite);
		SpritePool.Add(sprite.ID, sprite);
		return true;
	}

	public AngeSprite CreateSpriteInGroup (SpriteGroup group, int atlasIndex, IRect pixelRect, int duration = 5) {
		string name = $"{group.Name} {group.Count}";
		var atlas = Atlas[atlasIndex];
		var sprite = new AngeSprite {
			Group = group,
			ID = name.AngeHash(),
			RealName = name,
			PixelRect = pixelRect,
			GlobalWidth = pixelRect.width * Const.ART_SCALE,
			GlobalHeight = pixelRect.height * Const.ART_SCALE,
			Atlas = atlas,
			AtlasIndex = atlasIndex,
			LocalZ = 0,
			SortingZ = atlas.AtlasZ * 1024,
			PivotX = 0,
			PivotY = 0,
			Pixels = new Color32[pixelRect.width * pixelRect.height],
			Rule = 0,
			GlobalBorder = default,
			IsTrigger = false,
			SummaryTint = default,
			Duration = duration,
			Tag = 0,
		};
		group.SpriteIDs.Add(sprite.ID);
		return sprite;
	}

	public SpriteGroup CreateSpriteGroup (string name, GroupType type) {
		int id = name.AngeHash();
		if (GroupPool.ContainsKey(id)) return null;
		var group = new SpriteGroup() {
			Name = name,
			ID = id,
			SpriteIDs = new(),
			Type = type,
		};
		Groups.Add(group);
		GroupPool.Add(id, group);
		return group;
	}

	// Remove
	public void RemoveAtlasAndAllSpritesInside (int index) {
		if (index < 0 || index >= Atlas.Count) return;
		// Remove Atlas
		Atlas.RemoveAt(index);
		// Remove Sprites
		for (int i = 0; i < Sprites.Count; i++) {
			var sprite = Sprites[i];
			if (sprite.AtlasIndex != index) continue;
			RemoveSprite(i);
			i--;
		}
	}

	public void RemoveGroupAndAllSpritesInside (int groupIndex) {
		var group = Groups[groupIndex];
		Groups.RemoveAt(groupIndex);
		GroupPool.Remove(group.ID);
		for (int i = 0; i < group.Count; i++) {
			int spId = group.SpriteIDs[i];
			int spIndex = Sprites.FindIndex(s => s.ID == spId);
			SpritePool.Remove(spId);
			Sprites.RemoveAt(spIndex);
		}
	}

	public void RemoveSprite (int spriteIndex) {
		var sprite = Sprites[spriteIndex];
		Sprites.RemoveAt(spriteIndex);
		SpritePool.Remove(sprite.ID);
		RemoveSpriteFromGroup(spriteIndex);
	}


	#endregion




	#region --- LGC ---


	private void LoadFromBinary_v0 (BinaryReader reader, System.Action<System.Exception> exceptionHandler) {

		var stream = reader.BaseStream;

		// Sprites
		int spriteCount = reader.ReadInt32();
		int spriteByteLength = reader.ReadInt32();
		long spriteEndPos = stream.Position + spriteByteLength;
		Sprites.Clear();
		try {
			for (int i = 0; i < spriteCount; i++) {
				var sprite = new AngeSprite();
				Sprites.Add(sprite);
				sprite.LoadFromBinary_v0(reader, exceptionHandler);
			}
		} catch (System.Exception ex) { exceptionHandler?.Invoke(ex); }
		if (stream.Position != spriteEndPos) stream.Position = spriteEndPos;

		// Groups
		int groupCount = reader.ReadInt32();
		int groupByteLength = reader.ReadInt32();
		long groupEndPos = stream.Position + groupByteLength;
		Groups.Clear();
		try {
			for (int i = 0; i < groupCount; i++) {
				var group = new SpriteGroup();
				Groups.Add(group);
				group.LoadFromBinary_v0(reader, exceptionHandler);
			}
		} catch (System.Exception ex) { exceptionHandler?.Invoke(ex); }
		if (stream.Position != groupEndPos) stream.Position = groupEndPos;

		// Atlas
		int atlasCount = reader.ReadInt32();
		int atlasByteLength = reader.ReadInt32();
		long atlasEndPos = stream.Position + atlasByteLength;
		Atlas.Clear();
		try {
			for (int i = 0; i < atlasCount; i++) {
				var atlas = new Atlas();
				Atlas.Add(atlas);
				atlas.LoadFromBinary_v0(reader, exceptionHandler);
			}
		} catch (System.Exception ex) { exceptionHandler?.Invoke(ex); }
		if (stream.Position != atlasEndPos) stream.Position = atlasEndPos;

		// Final
		ApplyExtraData();
	}

	private void SaveToBinary_v0 (BinaryWriter writer, System.Action<System.Exception> exceptionHandler) {
		try {

			var stream = writer.BaseStream;

			// Sprites
			{
				writer.Write((int)Sprites.Count);
				long markPos = stream.Position;
				writer.Write((int)0);
				long startPos = stream.Position;
				for (int i = 0; i < Sprites.Count; i++) {
					Sprites[i].SaveToBinary_v0(writer, exceptionHandler);
				}
				long endPos = stream.Position;
				stream.Position = markPos;
				writer.Write((int)(endPos - startPos));
				stream.Position = endPos;
			}

			// Groups
			{
				writer.Write((int)Groups.Count);
				long markPos = stream.Position;
				writer.Write((int)0);
				long startPos = stream.Position;
				for (int i = 0; i < Groups.Count; i++) {
					Groups[i].SaveToBinary_v0(writer, exceptionHandler);
				}
				long endPos = stream.Position;
				stream.Position = markPos;
				writer.Write((int)(endPos - startPos));
				stream.Position = endPos;
			}

			// Atlas
			{
				writer.Write((int)Atlas.Count);
				long markPos = stream.Position;
				writer.Write((int)0);
				long startPos = stream.Position;
				for (int i = 0; i < Atlas.Count; i++) {
					Atlas[i].SaveToBinary_v0(writer, exceptionHandler);
				}
				long endPos = stream.Position;
				stream.Position = markPos;
				writer.Write((int)(endPos - startPos));
				stream.Position = endPos;
			}

		} catch (System.Exception ex) { exceptionHandler?.Invoke(ex); }

	}

	private void ApplyExtraData () {
		// Fill Sprites
		SpritePool.Clear();
		for (int i = 0; i < Sprites.Count; i++) {
			var sp = Sprites[i];
			SpritePool.TryAdd(sp.ID, Sprites[i]);
		}
		// Fill Groups
		GroupPool.Clear();
		for (int i = 0; i < Groups.Count; i++) {
			var group = Groups[i];
			GroupPool.TryAdd(group.ID, group);
		}
		// Sprites
		for (int i = 0; i < Sprites.Count; i++) {
			var sprite = Sprites[i];
			sprite.Atlas = Atlas[sprite.AtlasIndex];
			sprite.SortingZ = sprite.Atlas.AtlasZ * 1024 + sprite.LocalZ;
		}
		// Groups
		for (int i = 0; i < Groups.Count; i++) {
			var group = Groups[i];
			for (int j = 0; j < group.SpriteIDs.Count; j++) {
				int id = group.SpriteIDs[j];
				if (SpritePool.TryGetValue(id, out var sprite)) {
					sprite.Group = group;
				}
			}
		}
		// Texture Pool
		foreach (var texture in TexturePool) 123;
		TexturePool.Clear();
		//static void FillSheetIntoTexturePool (Sheet sheet, Dictionary<int, Texture2D> TexturePool) {
		//	foreach (var sprite in sheet.Sprites) {
		//		if (TexturePool.ContainsKey(sprite.ID)) continue;
		//		var texture = GetTextureFromPixels(sprite.Pixels, sprite.PixelRect.width, sprite.PixelRect.height);
		//		if (!texture.HasValue) continue;
		//		Raylib.SetTextureFilter(texture.Value, TextureFilter.Point);
		//		Raylib.SetTextureWrap(texture.Value, TextureWrap.Clamp);
		//		TexturePool.Add(sprite.ID, texture.Value);
		//	}
		//}

	}

	private void RemoveSpriteFromGroup (int spriteIndex) {
		var sprite = Sprites[spriteIndex];
		var group = sprite.Group;
		if (group == null) return;
		int spIndexInGroup = group.SpriteIDs.IndexOf(sprite.ID);
		if (spIndexInGroup < 0) return;
		// ID
		group.SpriteIDs.RemoveAt(spIndexInGroup);
		// Empty Check
		if (group.Count == 0) {
			int groupIndex = Groups.FindIndex(s => s.ID == group.ID);
			if (groupIndex >= 0) Groups.RemoveAt(groupIndex);
			GroupPool.Remove(group.ID);
		}
	}


	#endregion




}

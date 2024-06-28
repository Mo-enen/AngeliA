using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace AngeliA;

public class Sheet {




	#region --- VAR ---


	// Api
	public readonly List<AngeSprite> Sprites = new();
	public readonly List<SpriteGroup> Groups = new();
	public readonly List<Atlas> Atlas = new();
	public readonly Dictionary<int, AngeSprite> SpritePool = new();
	public readonly Dictionary<int, SpriteGroup> GroupPool = new();
	public readonly Dictionary<int, object> TexturePool = new();

	// Data
	private readonly bool IgnoreGroups = false;
	private readonly bool IgnoreSpriteWithIgnoreTag = true;
	private readonly bool IgnoreTextureAndPixels = false;


	#endregion




	#region --- MSG ---


	public Sheet (bool ignoreGroups = false, bool ignoreSpriteWithIgnoreTag = true, bool ignoreTextureAndPixels = false) {
		IgnoreGroups = ignoreGroups;
		IgnoreSpriteWithIgnoreTag = ignoreSpriteWithIgnoreTag;
		IgnoreTextureAndPixels = ignoreTextureAndPixels;
	}


	public Sheet (
		List<AngeSprite> sprites,
		List<Atlas> atlasInfo,
		bool ignoreGroups = false,
		bool ignoreSpriteWithIgnoreTag = true,
		bool ignoreTextureAndPixels = false
	) : this(ignoreGroups, ignoreSpriteWithIgnoreTag, ignoreTextureAndPixels) {
		SetData(sprites, atlasInfo);
	}


	#endregion




	#region --- API ---


	public void SetData (List<AngeSprite> sprites, List<Atlas> atlasInfo) {
		Sprites.Clear();
		Atlas.Clear();
		Sprites.AddRange(sprites);
		Atlas.AddRange(atlasInfo);
		CalculateExtraData();
	}

	public bool LoadFromDisk (string path) {

		Clear();
		var bytes = Util.CompressedFileToByte(path, out int length);
		if (length == 0) return false;

		using var stream = new MemoryStream(bytes);
		using var reader = new BinaryReader(stream);

		// File Version
		int fileVersion = reader.ReadInt32();
		const int CODE_VERSION = 0;

		// Load Data
		switch (fileVersion) {
			case CODE_VERSION:
				LoadFromBinary_v0(reader);
				break;
			default:
				Debug.LogError($"Can not handle sheet version {fileVersion}. Expect: version-{CODE_VERSION}");
				return false;
		}

		return true;
	}

	public void SaveToDisk (string path) {
		using var stream = new MemoryStream(1024);
		using var writer = new BinaryWriter(stream);
		writer.Write((int)0); // File Version
		SaveToBinary_v0(writer);
		Util.ByteToCompressedFile(path, stream.GetBuffer(), (int)stream.Position);
	}

	public void CalculateExtraData () {

		// Sprites
		SpritePool.Clear();
		var spriteSpan = CollectionsMarshal.AsSpan(Sprites);
		for (int i = 0; i < spriteSpan.Length; i++) {
			var sprite = spriteSpan[i];
			sprite.Atlas = Atlas[sprite.AtlasIndex];
			sprite.SortingZ = sprite.Atlas.AtlasZ * 1024 + sprite.LocalZ;
			sprite.Group = null;
			SpritePool.TryAdd(sprite.ID, sprite);
		}

		// Make Groups
		GroupPool.Clear();
		if (!IgnoreGroups) {
			// Create Groups
			for (int i = 0; i < spriteSpan.Length; i++) {
				var sprite = spriteSpan[i];

				if (!Util.GetGroupInfoFromSpriteRealName(
					sprite.RealName, out string groupName, out int groupIndex
				)) continue;

				// Get or Create Group
				groupIndex = groupIndex.Clamp(0, SpriteGroup.MAX_COUNT - 1);
				int groupId = groupName.AngeHash();
				if (!GroupPool.TryGetValue(groupId, out var group)) {
					group = new SpriteGroup() {
						ID = groupId,
						Name = groupName,
						SpriteIDs = new(),
						LoopStart = 0,
						Animated = false,
						WithRule = false,
						Random = false,
					};
					Groups.Add(group);
					GroupPool.Add(groupId, group);
				}
				sprite.Group = group;

				// Add Sprite ID into Group
				if (groupIndex < group.Count) {
					group[groupIndex] = sprite.ID;
				} else if (groupIndex == group.Count) {
					group.Add(sprite.ID);
				} else {
					for (int safe = 0; groupIndex > group.Count && safe < SpriteGroup.MAX_COUNT; safe++) {
						group.Add(0);
					}
					group.Add(sprite.ID);
				}

				// Extra Info
				if (sprite.Duration > 0) group.Animated = true;
				if (sprite.Rule != 0) group.WithRule = true;
				if (sprite.Tag.HasAll(Tag.LoopStart)) group.LoopStart = groupIndex;
				if (sprite.Tag.HasAll(Tag.Random)) group.Random = true;

			}
			// Remove Null
			foreach (var group in Groups) {
				group.SpriteIDs.RemoveAll(id => id == 0);
			}
		}

		// Texture Pool
		foreach (var texture in TexturePool) {
			Game.UnloadTexture(texture);
		}
		TexturePool.Clear();
		if (!IgnoreTextureAndPixels) {
			for (int i = 0; i < spriteSpan.Length; i++) {
				var sprite = spriteSpan[i];
				SyncSpritePixelsIntoTexturePool(sprite);
			}
		}
	}

	public void Clear () {
		Sprites.Clear();
		Groups.Clear();
		Atlas.Clear();
		SpritePool.Clear();
		GroupPool.Clear();
		foreach (var (_, texture) in TexturePool) Game.UnloadTexture(texture);
		TexturePool.Clear();
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

	public bool TryGetTextureFromPool (int spriteID, out object texture) {
		texture = null;
		if (IgnoreTextureAndPixels || !SpritePool.TryGetValue(spriteID, out var sprite)) return false;
		if (TexturePool.TryGetValue(spriteID, out texture)) {
			return texture != null;
		} else {
			SyncSpritePixelsIntoTexturePool(sprite);
			if (TexturePool.TryGetValue(spriteID, out texture)) {
				return texture != null;
			} else {
				return false;
			}
		}
	}

	public void SyncSpritePixelsIntoTexturePool (AngeSprite sprite) {
		if (IgnoreTextureAndPixels) return;
		if (TexturePool.TryGetValue(sprite.ID, out var texture)) {
			var size = Game.GetTextureSize(texture);
			if (size.x != sprite.PixelRect.width || size.y != sprite.PixelRect.height) {
				// Resize
				Game.UnloadTexture(texture);
				TexturePool[sprite.ID] = Game.GetTextureFromPixels(
					sprite.Pixels, sprite.PixelRect.width, sprite.PixelRect.height
				);
			} else {
				// Fill
				Game.FillPixelsIntoTexture(sprite.Pixels, texture);
			}
		} else {
			TexturePool.Add(
				sprite.ID,
				Game.GetTextureFromPixels(sprite.Pixels, sprite.PixelRect.width, sprite.PixelRect.height)
			);
		}
	}

	public bool RenameSprite (int id, string newName) {
		if (!SpritePool.TryGetValue(id, out var sprite)) return false;
		return RenameSprite(sprite, newName);
	}

	public bool RenameSprite (AngeSprite sprite, string newName) {
		if (sprite.RealName == newName) return false;
		int id = newName.AngeHash();
		if (SpritePool.ContainsKey(id)) return false;
		int oldID = sprite.ID;
		if (sprite.Group != null) {
			for (int i = 0; i < sprite.Group.Count; i++) {
				if (sprite.Group.SpriteIDs[i] == oldID) {
					sprite.Group.SpriteIDs[i] = id;
					break;
				}
			}
		}
		SpritePool.Remove(oldID);
		SpritePool.Add(id, sprite);
		TexturePool.Remove(oldID);
		sprite.RealName = newName;
		sprite.ID = id;
		SyncSpritePixelsIntoTexturePool(sprite);
		return true;
	}

	public int MoveAtlas (int from, int to) {
		if (from == to) return to;
		if (to > from) to--;
		var atlas = Atlas[from];
		Atlas.RemoveAt(from);
		Atlas.Insert(to, atlas);
		int min = Util.Min(from, to);
		int max = Util.Max(from, to);
		int delta = (from - to).Sign3();
		foreach (var sp in Sprites) {
			if (sp.AtlasIndex == from) {
				sp.AtlasIndex = to;
				continue;
			}
			if (sp.AtlasIndex < min || sp.AtlasIndex > max) continue;
			sp.AtlasIndex += delta;
		}
		return to;
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
		SyncSpritePixelsIntoTexturePool(sprite);
		return true;
	}

	public void CombineSheet (Sheet sheet) {
		int atlasShift = Atlas.Count;
		foreach (var altas in sheet.Atlas) {
			Atlas.Add(altas);
		}
		if (!IgnoreGroups) {
			foreach (var group in sheet.Groups) {
				if (GroupPool.ContainsKey(group.ID)) continue;
				Groups.Add(group);
				GroupPool.Add(group.ID, group);
			}
		}
		foreach (var sprite in sheet.Sprites) {
			if (SpritePool.ContainsKey(sprite.ID)) continue;
			sprite.AtlasIndex += atlasShift;
			Sprites.Add(sprite);
			SpritePool.Add(sprite.ID, sprite);
			SyncSpritePixelsIntoTexturePool(sprite);
		}
	}

	// Remove
	public void RemoveAtlasAndAllSpritesInside (int atlasIndex) {
		if (atlasIndex < 0 || atlasIndex >= Atlas.Count) return;
		// Remove Atlas
		Atlas.RemoveAt(atlasIndex);
		// Remove Sprites
		for (int i = 0; i < Sprites.Count; i++) {
			if (Sprites[i].AtlasIndex == atlasIndex) {
				RemoveSprite(i);
				i--;
			}
		}
		// Fix Index
		foreach (var sprite in Sprites) {
			if (sprite.AtlasIndex > atlasIndex) {
				sprite.AtlasIndex--;
			}
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
		RemoveSpriteFromGroup(spriteIndex);
		Sprites.RemoveAt(spriteIndex);
		SpritePool.Remove(sprite.ID);
		if (TexturePool.Remove(sprite.ID, out object texture)) {
			Game.UnloadTexture(texture);
		}
	}

	// Create
	public AngeSprite CreateSprite (string name, IRect pixelRect, int atlasIndex) => new() {
		ID = name.AngeHash(),
		RealName = name,
		Atlas = Atlas[atlasIndex],
		AtlasIndex = atlasIndex,
		GlobalWidth = pixelRect.width * Const.ART_SCALE,
		GlobalHeight = pixelRect.height * Const.ART_SCALE,
		PixelRect = pixelRect,
		Pixels = new Color32[pixelRect.width * pixelRect.height],
		SortingZ = Atlas[atlasIndex].AtlasZ * 1024,
	};

	public string GetAvailableSpriteName (string basicName) {

		if (!SpritePool.ContainsKey(basicName.AngeHash())) return basicName;

		if (Util.GetGroupInfoFromSpriteRealName(basicName, out string groupName, out int groupIndex)) {
			// Grow Index
			string name = $"{groupName} {groupIndex}";
			while (SpritePool.ContainsKey(name.AngeHash())) {
				groupIndex++;
				name = $"{groupName} {groupIndex}";
			}
			return name;
		} else {
			// Add Index
			string name = basicName;
			int index = 0;
			while (SpritePool.ContainsKey(name.AngeHash())) {
				index++;
				name = $"{basicName} {index}";
			}
			return name;
		}
	}


	#endregion




	#region --- LGC ---


	private void LoadFromBinary_v0 (BinaryReader reader) {

		var stream = reader.BaseStream;

		// Sprites
		int spriteCount = reader.ReadInt32();
		int spriteByteLength = reader.ReadInt32();
		long spriteEndPos = stream.Position + spriteByteLength;
		Sprites.Clear();
		try {
			for (int i = 0; i < spriteCount; i++) {
				var sprite = new AngeSprite();
				sprite.LoadFromBinary_v0(reader, IgnoreTextureAndPixels);
				if (!IgnoreSpriteWithIgnoreTag || !sprite.Tag.HasAll(Tag.Palette)) {
					Sprites.Add(sprite);
				}
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
		if (stream.Position != spriteEndPos) stream.Position = spriteEndPos;

		// Atlas
		int atlasCount = reader.ReadInt32();
		int atlasByteLength = reader.ReadInt32();
		long atlasEndPos = stream.Position + atlasByteLength;
		Atlas.Clear();
		try {
			for (int i = 0; i < atlasCount; i++) {
				var atlas = new Atlas();
				Atlas.Add(atlas);
				atlas.LoadFromBinary_v0(reader);
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
		if (stream.Position != atlasEndPos) stream.Position = atlasEndPos;

		// Final
		CalculateExtraData();
	}

	private void SaveToBinary_v0 (BinaryWriter writer) {
		try {

			var stream = writer.BaseStream;

			// Sprites
			{
				var span = CollectionsMarshal.AsSpan(Sprites);
				writer.Write((int)span.Length);
				long markPos = stream.Position;
				writer.Write((int)0);
				long startPos = stream.Position;
				for (int i = 0; i < span.Length; i++) {
					span[i].SaveToBinary_v0(writer);
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
					Atlas[i].SaveToBinary_v0(writer);
				}
				long endPos = stream.Position;
				stream.Position = markPos;
				writer.Write((int)(endPos - startPos));
				stream.Position = endPos;
			}

		} catch (System.Exception ex) { Debug.LogException(ex); }

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

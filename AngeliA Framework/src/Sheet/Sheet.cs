using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace AngeliA;

public class Sheet (bool ignoreGroups = false, bool ignoreSpriteWithIgnoreTag = true, bool ignoreTextureAndPixels = false) {




	#region --- VAR ---


	// Const
	const int CODE_FILE_VERSION = 0;

	// Api
	public readonly List<AngeSprite> Sprites = [];
	public readonly List<SpriteGroup> Groups = [];
	public readonly List<Atlas> Atlas = [];
	public readonly Dictionary<int, AngeSprite> SpritePool = [];
	public readonly Dictionary<int, SpriteGroup> GroupPool = [];
	public readonly Dictionary<int, Atlas> AtlasPool = [];
	public readonly Dictionary<int, object> TexturePool = [];

	// Data
	private readonly bool IgnoreGroups = ignoreGroups;
	private readonly bool IgnoreSpriteWithIgnoreTag = ignoreSpriteWithIgnoreTag;
	private readonly bool IgnoreTextureAndPixels = ignoreTextureAndPixels;

	#endregion




	#region --- MSG ---


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

		// Load Data
		switch (fileVersion) {
			case CODE_FILE_VERSION:
				LoadFromBinary_v0(reader);
				break;
			default:
				Debug.LogError($"Can not handle sheet version {fileVersion}. Expect: version-{CODE_FILE_VERSION}");
				return false;
		}

		return true;
	}

	public void SaveToDisk (string path) {
		using var stream = new MemoryStream(1024);
		using var writer = new BinaryWriter(stream);
		writer.Write((int)CODE_FILE_VERSION);
		SaveToBinary_v0(writer);
		Util.ByteToCompressedFile(path, stream.GetBuffer(), (int)stream.Position);
	}

	public void CalculateExtraData () {

		// Atlas
		AtlasPool.Clear();
		foreach (var atlas in Atlas) {
			AtlasPool.TryAdd(atlas.ID, atlas);
		}

		// Sprites
		SpritePool.Clear();
		var spriteSpan = CollectionsMarshal.AsSpan(Sprites);
		for (int i = 0; i < spriteSpan.Length; i++) {
			var sprite = spriteSpan[i];
			sprite.Atlas = AtlasPool.TryGetValue(sprite.AtlasID, out var atlas) ? atlas : null;
			sprite.Group = null;
			sprite.AttachedSprite = null;
			SpritePool.TryAdd(sprite.ID, sprite);
		}

		// Make Groups
		GroupPool.Clear();
		if (!IgnoreGroups) {
			// Create Groups
			for (int i = 0; i < spriteSpan.Length; i++) {
				var sprite = spriteSpan[i];

				if (!FrameworkUtil.GetGroupInfoFromSpriteRealName(
					sprite.RealName, out string groupName, out int groupIndex
				)) continue;

				// Get or Create Group
				groupIndex = groupIndex.Clamp(0, SpriteGroup.MAX_COUNT - 1);
				int groupId = groupName.AngeHash();
				if (!GroupPool.TryGetValue(groupId, out var group)) {
					group = new SpriteGroup() {
						ID = groupId,
						Name = groupName,
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
					group.Sprites[groupIndex] = sprite;
				} else if (groupIndex == group.Count) {
					group.Sprites.Add(sprite);
				} else {
					for (int safe = 0; groupIndex > group.Count && safe < SpriteGroup.MAX_COUNT; safe++) {
						group.Sprites.Add(AngeSprite.EMPTY);
					}
					group.Sprites.Add(sprite);
				}

				// Extra Info
				if (sprite.Duration > 0) group.Animated = true;
				if (!sprite.Rule.IsEmpty) group.WithRule = true;
				if (sprite.Tag.HasAll(Tag.LoopStart)) {
					group.LoopStart = groupIndex;
				}
				if (sprite.Tag.HasAll(Tag.Random)) group.Random = true;

			}
			// Remove Null
			foreach (var group in Groups) {
				group.Sprites.RemoveAll(_sp => _sp == null || _sp.ID == 0);
			}
		}

		// Attachment
		var spSpan = Sprites.GetSpan();
		var spriteLen = Sprites.Count;
		for (int i = 0; i < spriteLen; i++) {
			try {
				const string ATTACH = ".attach";
				var sprite = spSpan[i];
				if (!sprite.RealName.EndsWith(ATTACH)) continue;
				int hostID = sprite.RealName.AngeHash(0, sprite.RealName.Length - ATTACH.Length);
				if (SpritePool.TryGetValue(hostID, out var hostSP)) {
					hostSP.AttachedSprite = sprite;
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
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
		AtlasPool.Clear();
		foreach (var (_, texture) in TexturePool) Game.UnloadTexture(texture);
		TexturePool.Clear();
	}

	public int GetSpriteAnimationDuration (SpriteGroup aniGroup) => aniGroup.Count == 0 ? 0 : GetTiming(aniGroup, aniGroup.Count);

	public bool TryGetSpriteFromAnimationFrame (SpriteGroup group, int localFrame, out AngeSprite sprite, int loopStart = -1) {
		int len = group.Count;
		sprite = null;
		if (len == 0) return false;
		loopStart = loopStart < 0 ? group.LoopStart : loopStart;
		int loopStartTiming = loopStart == 0 ? 0 : GetTiming(group, loopStart.Clamp(0, len - 1)) + 1;
		int frameLen = GetTiming(group, len);
		int totalFrame = localFrame < loopStartTiming ? frameLen : frameLen - loopStartTiming + 1;
		int frameOffset = localFrame < loopStartTiming ? 0 : loopStartTiming;
		localFrame = ((localFrame - frameOffset) % totalFrame) + frameOffset;
		sprite = GetTimingSprite(group, localFrame);
		return sprite != null;
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
		SpritePool.Remove(oldID);
		SpritePool.Add(id, sprite);
		TexturePool.Remove(oldID);
		sprite.RealName = newName;
		sprite.ID = id;
		SyncSpritePixelsIntoTexturePool(sprite);
		return true;
	}

	public bool RenameAtlas (int atlasID, string newName) {
		if (!AtlasPool.TryGetValue(atlasID, out var atlas)) return false;
		int newID = newName.AngeHash();
		if (newID == atlasID) return false;
		if (AtlasPool.ContainsKey(newID)) return false;
		AtlasPool.Remove(atlasID);
		AtlasPool.Add(newID, atlas);
		atlas.ID = newID;
		atlas.Name = newName;
		// Sync all Sprites
		int len = Sprites.Count;
		for (int i = 0; i < len; i++) {
			var sp = Sprites[i];
			if (sp.AtlasID == atlasID) {
				sp.AtlasID = newID;
			}
		}
		return true;
	}

	public void MoveAtlas (int from, int to, bool intoFolder = false) {

		var atlas = Atlas[from];

		if (from == to) {
			if (intoFolder && !atlas.IsFolder) {
				atlas.State = AtlasState.Sub;
			}
			return;
		}

		if (atlas.IsFolder) {
			// Range
			int len = 1;
			for (int i = from + 1; i < Atlas.Count; i++) {
				if (Atlas[i].InFolder) {
					len++;
				} else {
					break;
				}
			}
			// Folder Redirect
			if (to >= 0 && to < Atlas.Count && Atlas[to].InFolder) {
				for (; to < Atlas.Count; to++) {
					if (!Atlas[to].InFolder) break;
				}
			}
			// Check Valid
			if (to >= from && to < from + len) {
				return;
			}
			// Move Range
			if (to > from) {
				// to Bottom
				for (int i = len - 1; i >= 0; i--) {
					var _atlas = Atlas[from + i];
					Atlas.RemoveAt(from + i);
					Atlas.Insert(to + i - len, _atlas);
				}
			} else {
				// to Top
				for (int i = 0; i < len; i++) {
					var _atlas = Atlas[from + i];
					Atlas.RemoveAt(from + i);
					Atlas.Insert(to + i, _atlas);
				}
			}

		} else {
			// Single
			if (to > from) to--;
			Atlas.RemoveAt(from);
			Atlas.Insert(to, atlas);
			atlas.State = intoFolder ? AtlasState.Sub : AtlasState.Root;
		}
	}

	// Find
	public int IndexOfSprite (int id) {
		int len = Sprites.Count;
		var span = Sprites.AsReadOnly();
		for (int i = 0; i < len; i++) {
			if (span[i].ID == id) return i;
		}
		return -1;
	}

	public int IndexOfGroup (int id) {
		int len = Groups.Count;
		var span = Groups.AsReadOnly();
		for (int i = 0; i < len; i++) {
			if (span[i].ID == id) return i;
		}
		return -1;
	}

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

	public void CombineSheet (Sheet sheet, bool renameDuplicateSprites = false) {
		foreach (var altas in sheet.Atlas) {
			Atlas.Add(altas);
		}
		foreach (var sprite in sheet.Sprites) {
			if (SpritePool.ContainsKey(sprite.ID)) {
				if (renameDuplicateSprites) {
					sprite.RealName = GetAvailableSpriteName(sprite.RealName);
					sprite.ID = sprite.RealName.AngeHash();
					if (
						sprite.Group != null &&
						sprite.Group.Count > 0 &&
						sprite.Group.Sprites[0] == sprite &&
						FrameworkUtil.GetGroupInfoFromSpriteRealName(sprite.RealName, out string newGroupName, out _)
					) {
						sprite.Group.Name = newGroupName;
						sprite.Group.ID = sprite.Group.Name.AngeHash();
					}
				} else continue;
			}
			Sprites.Add(sprite);
			SpritePool.Add(sprite.ID, sprite);
			SyncSpritePixelsIntoTexturePool(sprite);
		}
		if (!IgnoreGroups) {
			foreach (var group in sheet.Groups) {
				if (GroupPool.ContainsKey(group.ID)) continue;
				Groups.Add(group);
				GroupPool.Add(group.ID, group);
			}
		}
	}

	public void CombineAllSheetInFolder (string folderPath, bool topOnly = false, string ignoreNameWithExtension = "") {
		foreach (var subPath in Util.EnumerateFiles(folderPath, topOnly, AngePath.SHEET_SEARCH_PATTERN)) {
			string name = Util.GetNameWithExtension(subPath);
			if (name == ignoreNameWithExtension) continue;
			var sheet = new Sheet();
			if (!sheet.LoadFromDisk(subPath)) continue;
			CombineSheet(sheet);
		}
	}

	// Remove
	public void RemoveAtlasAndAllSpritesInside (int atlasIndex) {
		if (atlasIndex < 0 || atlasIndex >= Atlas.Count) return;
		// Remove Atlas
		var removedAtlas = Atlas[atlasIndex];
		Atlas.RemoveAt(atlasIndex);
		AtlasPool.Remove(removedAtlas.ID);
		// Remove Sprites
		for (int i = 0; i < Sprites.Count; i++) {
			if (Sprites[i].AtlasID == removedAtlas.ID) {
				RemoveSprite(i);
				i--;
			}
		}
	}

	public void RemoveAllAtlasAndAllSpritesInsideExcept (int ignoreAtlasID) {

		if (ignoreAtlasID == 0) return;

		// Remove Sprites
		for (int i = 0; i < Sprites.Count; i++) {
			if (Sprites[i].AtlasID != ignoreAtlasID) {
				RemoveSprite(i);
				i--;
			}
		}

		// Remove Atlas
		AtlasPool.TryGetValue(ignoreAtlasID, out var keepAtlas);
		Atlas.Clear();
		AtlasPool.Clear();
		if (keepAtlas != null) {
			Atlas.Add(keepAtlas);
			AtlasPool.Add(keepAtlas.ID, keepAtlas);
		}
	}

	public void RemoveGroupAndAllSpritesInside (int groupIndex) {
		var group = Groups[groupIndex];
		Groups.RemoveAt(groupIndex);
		GroupPool.Remove(group.ID);
		for (int i = 0; i < group.Count; i++) {
			var sp = group.Sprites[i];
			if (sp == null || sp.ID == 0) continue;
			SpritePool.Remove(sp.ID);
			Sprites.Remove(sp);
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
	public AngeSprite CreateSprite (string name, IRect pixelRect, int atlasID) {
		return new() {
			ID = name.AngeHash(),
			RealName = name,
			Atlas = AtlasPool.TryGetValue(atlasID, out var atlas) ? atlas : Atlas[0],
			AtlasID = atlasID,
			GlobalWidth = pixelRect.width * Const.ART_SCALE,
			GlobalHeight = pixelRect.height * Const.ART_SCALE,
			PixelRect = pixelRect,
			Pixels = new Color32[pixelRect.width * pixelRect.height],
		};
	}

	public string GetAvailableSpriteName (string basicName) {

		if (!SpritePool.ContainsKey(basicName.AngeHash())) return basicName;

		if (FrameworkUtil.GetGroupInfoFromSpriteRealName(basicName, out string groupName, out int groupIndex)) {
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
		for (int i = 0; i < spriteCount; i++) {
			try {
				var sprite = new AngeSprite();
				sprite.LoadFromBinary_v0(reader, IgnoreTextureAndPixels);
				if (!IgnoreSpriteWithIgnoreTag || !sprite.Tag.HasAll(Tag.Palette)) {
					Sprites.Add(sprite);
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
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
		// Sprite
		group.Sprites.Remove(sprite);
		// Empty Check
		if (group.Count == 0) {
			int groupIndex = Groups.FindIndex(s => s.ID == group.ID);
			if (groupIndex >= 0) Groups.RemoveAt(groupIndex);
			GroupPool.Remove(group.ID);
		}
	}


	// Timing Util
	private static int GetTiming (SpriteGroup group, int spriteIndex) {
		int result = 0;
		var span = CollectionsMarshal.AsSpan(group.Sprites);
		int len = Util.Min(spriteIndex, span.Length);
		for (int i = 0; i < len; i++) {
			var sprite = span[i];
			if (sprite == null) continue;
			result += sprite.Duration;
		}
		return result;
	}

	private static AngeSprite GetTimingSprite (SpriteGroup group, int targetTiming) {
		AngeSprite result = null;
		int totalDuration = 0;
		var span = CollectionsMarshal.AsSpan(group.Sprites);
		for (int i = 0; i < group.Count; i++) {
			var sprite = span[i];
			if (sprite == null) continue;
			result = sprite;
			totalDuration += sprite.Duration;
			if (totalDuration >= targetTiming) break;
		}
		return result;
	}


	#endregion




}

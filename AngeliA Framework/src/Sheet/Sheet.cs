using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace AngeliA;

/// <summary>
/// Artwork sheet that holds sprites, sprite groups, atlas and textures data for rendering
/// </summary>
public class Sheet {




	#region --- VAR ---


	// Const
	const int CODE_FILE_VERSION = 0;

	// Api
	/// <summary>
	/// All sprite instances inside this sheet
	/// </summary>
	public readonly List<AngeSprite> Sprites = [];
	/// <summary>
	/// All sprite group instances inside this sheet
	/// </summary>
	public readonly List<SpriteGroup> Groups = [];
	/// <summary>
	/// All atlas instances inside this sheet
	/// </summary>
	public readonly List<Atlas> Atlas = [];
	public readonly Dictionary<int, AngeSprite> SpritePool = [];
	public readonly Dictionary<int, SpriteGroup> GroupPool = [];
	public readonly Dictionary<int, Atlas> AtlasPool = [];
	public readonly Dictionary<int, object> TexturePool = [];

	// Data
	private readonly bool IgnoreGroups;
	private readonly bool IgnoreSpriteWithPaletteTag;
	private readonly bool IgnoreTextureAndPixels;


	#endregion




	#region --- MSG ---


	/// <summary>
	/// Create a sheet with given data
	/// </summary>
	/// <param name="sprites"></param>
	/// <param name="atlasInfo"></param>
	/// <param name="ignoreGroups">True if do not require sprite group</param>
	/// <param name="ignoreSpriteWithIgnoreTag">True if do not load sprites with Tag.Palette</param>
	/// <param name="ignoreTextureAndPixels">True if do not load pixel data and do not create textures</param>
	public Sheet (
		List<AngeSprite> sprites,
		List<Atlas> atlasInfo,
		bool ignoreGroups = false,
		bool ignoreSpriteWithIgnoreTag = true,
		bool ignoreTextureAndPixels = false
	) : this(ignoreGroups, ignoreSpriteWithIgnoreTag, ignoreTextureAndPixels) {
		SetData(sprites, atlasInfo);
	}


	/// <summary>
	/// Artwork sheet that holds sprites, sprite groups, atlas and textures data for rendering
	/// </summary>
	/// <param name="ignoreGroups">True if do not require sprite group</param>
	/// <param name="ignoreSpriteWithPaletteTag">True if do not load sprites with Tag.Palette</param>
	/// <param name="ignoreTextureAndPixels">True if do not load pixel data and do not create textures</param>
	public Sheet (bool ignoreGroups = false, bool ignoreSpriteWithPaletteTag = true, bool ignoreTextureAndPixels = false) {
		IgnoreGroups = ignoreGroups;
		IgnoreSpriteWithPaletteTag = ignoreSpriteWithPaletteTag;
		IgnoreTextureAndPixels = ignoreTextureAndPixels;
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Set the sprites and atlas data of this sheet
	/// </summary>
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

	/// <summary>
	/// Clear all content inside this sheet
	/// </summary>
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

	/// <summary>
	/// Get how long does the given animation group takes in frame
	/// </summary>
	public int GetSpriteAnimationDuration (SpriteGroup aniGroup) => aniGroup.Count == 0 ? 0 : GetTiming(aniGroup, aniGroup.Count);

	/// <summary>
	/// Get current showing sprite from a animation group
	/// </summary>
	/// <param name="group"></param>
	/// <param name="localFrame">Animation frame start from 0</param>
	/// <param name="sprite">Result sprite</param>
	/// <param name="loopStart">Sprite index this animation start to play after it reach the end. Set to -1 to use loop start value from group.</param>
	/// <returns>True if the sprite is successfuly found</returns>
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

	/// <summary>
	/// Get texture for rendering from given ID
	/// </summary>
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

	/// <summary>
	/// Update rendering texture for the sprite if the pixel content changed
	/// </summary>
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

	/// <summary>
	/// Move the atlas inside the atlas list
	/// </summary>
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
	/// <summary>
	/// Find the sprite's index in the list
	/// </summary>
	/// <returns>-1 if the sprite not found</returns>
	public int IndexOfSprite (int id) {
		int len = Sprites.Count;
		var span = Sprites.AsReadOnly();
		for (int i = 0; i < len; i++) {
			if (span[i].ID == id) return i;
		}
		return -1;
	}

	/// <summary>
	/// Find the group's index in the list
	/// </summary>
	/// <returns>-1 if the group not found</returns>
	public int IndexOfGroup (int id) {
		int len = Groups.Count;
		var span = Groups.AsReadOnly();
		for (int i = 0; i < len; i++) {
			if (span[i].ID == id) return i;
		}
		return -1;
	}

	/// <summary>
	/// True if there is a sprite with given ID.
	/// </summary>
	public bool ContainSprite (int id) => SpritePool.ContainsKey(id);

	/// <summary>
	/// True if there is a group with given ID.
	/// </summary>
	public bool ContainGroup (int id) => GroupPool.ContainsKey(id);

	// Add
	/// <summary>
	/// Add the given sprite into this sheet
	/// </summary>
	/// <returns>True if the sprite is successfuly added</returns>
	public bool AddSprite (AngeSprite sprite) {
		if (sprite.ID == 0 || SpritePool.ContainsKey(sprite.ID)) return false;
		Sprites.Add(sprite);
		SpritePool.Add(sprite.ID, sprite);
		SyncSpritePixelsIntoTexturePool(sprite);
		return true;
	}

	/// <summary>
	/// Add all content from given sheet into this sheet.
	/// </summary>
	/// <param name="sheet"></param>
	/// <param name="renameDuplicateSprites">Set to false to skip the sprite with same name</param>
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

	/// <summary>
	/// Add all content from sheets from given folder into this sheet.
	/// </summary>
	/// <param name="folderPath"></param>
	/// <param name="topOnly"></param>
	/// <param name="ignoreNameWithExtension">Ignore sheets with this name</param>
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

	public void RemoveAllAtlasAndSpritesInsideExcept (int ignoreAtlasID) {

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
	/// <summary>
	/// Create a sprite without add into this sheet
	/// </summary>
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

	/// <summary>
	/// Get a new sprite name that be add into this sheet
	/// </summary>
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
				if (!IgnoreSpriteWithPaletteTag || !sprite.Tag.HasAll(Tag.Palette)) {
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

	private void CalculateExtraData () {

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

using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace AngeliA {
	public class Sheet {

		// VAR 
		public bool NotEmpty => Sprites.Count > 0;
		public readonly List<AngeSprite> Sprites = new();
		public readonly List<SpriteGroup> Groups = new();
		public readonly List<Atlas> Atlas = new();
		public readonly Dictionary<int, AngeSprite> SpritePool = new();
		public readonly Dictionary<int, SpriteGroup> GroupPool = new();

		// MSG
		public Sheet () { }
		public Sheet (List<AngeSprite> sprites, List<SpriteGroup> groups, List<Atlas> atlasInfo) => SetData(sprites, groups, atlasInfo);

		// API
		public virtual void SetData (List<AngeSprite> sprites, List<SpriteGroup> groups, List<Atlas> atlasInfo) {
			Sprites.Clear();
			Groups.Clear();
			Atlas.Clear();
			Sprites.AddRange(sprites);
			Groups.AddRange(groups);
			Atlas.AddRange(atlasInfo);
			ApplyExtraData();
		}

		public virtual bool LoadFromDisk (string path, System.Action<System.Exception> exceptionHandler = null) {

			Clear();
			if (!Util.FileExists(path)) return false;

			using var stream = new FileStream(path, FileMode.Open);
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

		public virtual void SaveToDisk (string path, System.Action<System.Exception> exceptionHandler = null) {
			using var stream = new FileStream(path, FileMode.Create);
			using var writer = new BinaryWriter(stream);
			writer.Write((int)0); // File Version
			SaveToBinary_v0(writer, exceptionHandler);
		}

		public virtual void Clear () {
			Sprites.Clear();
			Groups.Clear();
			Atlas.Clear();
			SpritePool.Clear();
			GroupPool.Clear();
		}

		// LGC
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
				SpritePool.TryAdd(sp.GlobalID, Sprites[i]);
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
		}

	}
}

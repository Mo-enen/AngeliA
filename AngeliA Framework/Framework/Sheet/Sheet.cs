using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace AngeliaFramework {
	public class Sheet {

		// VAR 
		public bool NotEmpty => Sprites.Count > 0;
		public object Texture = null;
		public readonly List<AngeSprite> Sprites = new();
		public readonly List<SpriteGroup> Groups = new();
		public readonly List<Atlas> Atlas = new();

		// MSG
		public Sheet () { }
		public Sheet (List<AngeSprite> sprites, List<SpriteGroup> groups, List<Atlas> atlasInfo, object texture) => SetData(sprites, groups, atlasInfo, texture);

		// API
		public virtual void SetData (List<AngeSprite> sprites, List<SpriteGroup> groups, List<Atlas> atlasInfo, object texture) {
			Sprites.Clear();
			Sprites.AddRange(sprites);
			Groups.Clear();
			Groups.AddRange(groups);
			Atlas.Clear();
			Atlas.AddRange(atlasInfo);
			Texture = texture;
			ApplyData();
		}

		public virtual bool LoadFromDisk (string path) {

			Clear();
			if (!Util.FileExists(path)) return false;

			using var stream = new FileStream(path, FileMode.Open);
			using var reader = new BinaryReader(stream);

			// File Version
			int fileVersion = reader.ReadInt32();

			// Load Texture
			int textureSize = reader.ReadInt32();
			if (textureSize > 0) {
				var pngBytes = reader.ReadBytes(textureSize);
				Texture = Game.PngBytesToTexture(pngBytes);
			}

			// Load Data
			switch (fileVersion) {
				case 0:
					LoadFromBinary_v0(reader);
					break;
				default:
					Game.LogError($"Can not handle sheet version {fileVersion}. Expect: version-0");
					return false;
			}

			// Final
			ApplyData();

			return true;
		}

		public virtual void SaveToDisk (string path) {
			using var stream = new FileStream(path, FileMode.Create);
			using var writer = new BinaryWriter(stream);
			// File Version
			writer.Write((int)0);
			// Save Texture
			if (Texture != null) {
				var pngBytes = Game.TextureToPngBytes(Texture);
				writer.Write((int)pngBytes.Length);
				writer.Write(pngBytes);
			} else {
				writer.Write((int)0);
			}
			// Save Data
			SaveToBinary_v0(writer);
		}

		public virtual void Clear () {
			Sprites.Clear();
			Groups.Clear();
			Atlas.Clear();
			Texture = null;
		}

		public static object LoadSheetTextureFromDisk (string path) {
			if (!Util.FileExists(path)) return null;
			object result = null;
			using var stream = new FileStream(path, FileMode.Open);
			using var reader = new BinaryReader(stream);
			reader.ReadInt32(); // File Version
			int textureSize = reader.ReadInt32();
			if (textureSize > 0) {
				var pngBytes = reader.ReadBytes(textureSize);
				result = Game.PngBytesToTexture(pngBytes);
			}
			return result;
		}

		// LGC
		private void ApplyData () {
			for (int i = 0; i < Sprites.Count; i++) {
				var sp = Sprites[i];
				sp.Atlas = Atlas[sp.AtlasIndex];
				sp.SortingZ = sp.Atlas.AtlasZ * 1024 + sp.LocalZ;
			}
			for (int i = 0; i < Groups.Count; i++) {
				var group = Groups[i];
				if (group.SpriteIndexes != null && group.SpriteIndexes.Count > 0) {
					group.Sprites = new List<AngeSprite>(group.SpriteIndexes.Count);
					for (int j = 0; j < group.SpriteIndexes.Count; j++) {
						int index = group.SpriteIndexes[j];
						var sp = Sprites[index];
						sp.Group = group;
						group.Sprites.Add(sp);
					}
				} else {
					group.Sprites = new();
				}
			}
		}

		private void LoadFromBinary_v0 (BinaryReader reader) {

			var stream = reader.BaseStream;

			var textureSize = Game.GetTextureSize(Texture);
			textureSize.x = textureSize.x.GreaterOrEquel(1);
			textureSize.y = textureSize.y.GreaterOrEquel(1);

			// Sprites
			int spriteCount = reader.ReadInt32();
			int spriteByteLength = reader.ReadInt32();
			long spriteEndPos = stream.Position + spriteByteLength;
			Sprites.Clear();
			try {
				for (int i = 0; i < spriteCount; i++) {
					var sprite = new AngeSprite();
					Sprites.Add(sprite);
					sprite.LoadFromBinary_v0(reader, textureSize.x, textureSize.y);
				}
			} catch (System.Exception ex) { Game.LogException(ex); }
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
					group.LoadFromBinary_v0(reader);
				}
			} catch (System.Exception ex) { Game.LogException(ex); }
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
					atlas.LoadFromBinary_v0(reader);
				}
			} catch (System.Exception ex) { Game.LogException(ex); }
			if (stream.Position != atlasEndPos) stream.Position = atlasEndPos;

		}

		private void SaveToBinary_v0 (BinaryWriter writer) {
			try {

				var stream = writer.BaseStream;

				// Sprites
				{
					writer.Write((int)Sprites.Count);
					long markPos = stream.Position;
					writer.Write((int)0);
					long startPos = stream.Position;
					for (int i = 0; i < Sprites.Count; i++) {
						Sprites[i].SaveToBinary_v0(writer);
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
						Groups[i].SaveToBinary_v0(writer);
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

			} catch (System.Exception ex) { Game.LogException(ex); }

		}

	}
}

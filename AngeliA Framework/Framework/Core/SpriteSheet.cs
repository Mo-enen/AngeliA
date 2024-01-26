using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace AngeliaFramework {


	public enum AtlasType {
		General = 0,
		Level = 1,
		Background = 2,
	}


	public enum GroupType {
		General = 0,
		Rule = 1,
		Random = 2,
		Animated = 3,
	}


	// Sprite
	public class AngeSprite {

		private static readonly StringBuilder CacheBuilder = new(256);

		public int GlobalID;
		public string RealName;
		public int GlobalWidth;
		public int GlobalHeight;
		public int PivotX;
		public int PivotY;
		public int SortingZ;
		public int LocalZ;
		public Int4 GlobalBorder;

		public IRect TextureRect;
		public FRect UvRect;
		public Float4 UvBorder; // xyzw => ldru

		public int AtlasIndex;
		public AtlasInfo Atlas;
		public Byte4 SummaryTint;
		public GroupType? GroupType;
		public bool IsTrigger;
		public int Rule;
		public int Tag;

		public void LoadFromBinary_v0 (BinaryReader reader, int textureWidth, int textureHeight) {
			uint byteLen = reader.ReadUInt32();
			long endPos = reader.BaseStream.Position + byteLen;
			try {
				float fTextureWidth = textureWidth;
				float fTextureHeight = textureHeight;
				// Name
				int nameLen = reader.ReadByte();
				CacheBuilder.Clear();
				for (int i = 0; i < nameLen; i++) {
					CacheBuilder.Append((char)reader.ReadByte());
				}
				RealName = CacheBuilder.ToString();

				// ID
				GlobalID = RealName.AngeHash();

				// Size
				GlobalWidth = reader.ReadUInt16();
				GlobalHeight = reader.ReadUInt16();

				// Pivot
				PivotX = reader.ReadInt16();
				PivotY = reader.ReadInt16();

				// Local Z
				LocalZ = reader.ReadInt32();

				// Global Border
				GlobalBorder = new Int4(
					reader.ReadUInt16(),
					reader.ReadUInt16(),
					reader.ReadUInt16(),
					reader.ReadUInt16()
				);

				// UV
				TextureRect = new IRect(
					reader.ReadInt32(),
					reader.ReadInt32(),
					reader.ReadInt32(),
					reader.ReadInt32()
				);
				UvRect = FRect.MinMaxRect(
					(TextureRect.x + 0.0001f) / fTextureWidth,
					(TextureRect.y + 0.0001f) / fTextureHeight,
					(TextureRect.xMax - 0.0001f) / fTextureWidth,
					(TextureRect.yMax - 0.0001f) / fTextureHeight
				);
				UvBorder = new(
					GlobalBorder.left / (float)GlobalWidth,
					GlobalBorder.down / (float)GlobalHeight,
					GlobalBorder.right / (float)GlobalWidth,
					GlobalBorder.up / (float)GlobalHeight
				);

				// Atlas Index
				AtlasIndex = reader.ReadByte();

				// Summary Tint
				SummaryTint = Util.IntToColor(reader.ReadInt32());

				// IsTrigger
				IsTrigger = reader.ReadByte() == 1;

				// Rule
				Rule = reader.ReadInt32();

				// Tag
				Tag = reader.ReadInt32();

				// Group Type
				GroupType = null;

			} catch (System.Exception ex) { Game.LogException(ex); }
			reader.BaseStream.Position = endPos;
		}

		public void SaveToBinary_v0 (BinaryWriter writer) {
			long markPos = writer.BaseStream.Position;
			writer.Write((uint)0);
			long startPos = writer.BaseStream.Position;
			try {

				// Name
				int len = RealName.Length.Clamp(0, 255);
				writer.Write((byte)len);
				for (int i = 0; i < len; i++) {
					writer.Write((byte)RealName[i]);
				}

				// Size
				writer.Write((ushort)GlobalWidth);
				writer.Write((ushort)GlobalHeight);

				// Pivot
				writer.Write((short)PivotX);
				writer.Write((short)PivotY);

				// Local Z
				writer.Write((int)LocalZ);

				// Global Border
				writer.Write((ushort)GlobalBorder.x);
				writer.Write((ushort)GlobalBorder.y);
				writer.Write((ushort)GlobalBorder.z);
				writer.Write((ushort)GlobalBorder.w);

				// Texture Rect
				writer.Write((int)TextureRect.x);
				writer.Write((int)TextureRect.y);
				writer.Write((int)TextureRect.width);
				writer.Write((int)TextureRect.height);

				// Atlas Index
				writer.Write((byte)AtlasIndex);

				// Summary Tint
				writer.Write((int)Util.ColorToInt(SummaryTint));

				// IsTrigger
				writer.Write((byte)(IsTrigger ? 1 : 0));

				// Rule
				writer.Write((int)Rule);

				// Tag
				writer.Write((int)Tag);

			} catch (System.Exception ex) { Game.LogException(ex); }
			long endPos = writer.BaseStream.Position;
			writer.BaseStream.Position = markPos;
			writer.Write((uint)(endPos - startPos));
			writer.BaseStream.Position = endPos;
		}

	}



	// Group
	public class SpriteGroup {

		public AngeSprite this[int i] => Sprites[i];
		public int Length => Sprites.Length;

		public int ID;
		public int LoopStart;
		public GroupType Type;
		public int[] SpriteIndexes;
		public AngeSprite[] Sprites;

		public void LoadFromBinary_v0 (BinaryReader reader) {
			uint byteLen = reader.ReadUInt32();
			long endPos = reader.BaseStream.Position + byteLen;
			try {
				// ID
				ID = reader.ReadInt32();

				// Loop Start
				LoopStart = reader.ReadInt16();

				// Group Type
				Type = (GroupType)reader.ReadByte();

				// Sprite Indexes
				int len = reader.ReadUInt16();
				SpriteIndexes = new int[len];
				for (int i = 0; i < len; i++) {
					SpriteIndexes[i] = reader.ReadInt32();
				}

			} catch (System.Exception ex) { Game.LogException(ex); }
			reader.BaseStream.Position = endPos;
		}

		public void SaveToBinary_v0 (BinaryWriter writer) {
			long markPos = writer.BaseStream.Position;
			writer.Write((uint)0);
			long startPos = writer.BaseStream.Position;
			try {

				// ID
				writer.Write((int)ID);

				// Loop Start
				writer.Write((short)LoopStart);

				// Group Type
				writer.Write((byte)Type);

				// Sprite Indexes
				writer.Write((ushort)SpriteIndexes.Length);
				for (int i = 0; i < SpriteIndexes.Length; i++) {
					writer.Write((int)SpriteIndexes[i]);
				}

			} catch (System.Exception ex) { Game.LogException(ex); }
			long endPos = writer.BaseStream.Position;
			writer.BaseStream.Position = markPos;
			writer.Write((uint)(endPos - startPos));
			writer.BaseStream.Position = endPos;
		}

	}



	// Atlas
	public class AtlasInfo {

		private static readonly StringBuilder CacheBuilder = new(256);
		public string Name;
		public AtlasType Type;
		public int AtlasZ;

		public void LoadFromBinary_v0 (BinaryReader reader) {
			uint byteLen = reader.ReadUInt32();
			long endPos = reader.BaseStream.Position + byteLen;
			try {
				// Name
				int nameLen = reader.ReadByte();
				CacheBuilder.Clear();
				for (int i = 0; i < nameLen; i++) {
					CacheBuilder.Append((char)reader.ReadByte());
				}
				Name = CacheBuilder.ToString();

				// Type
				Type = (AtlasType)reader.ReadByte();

				// Z
				AtlasZ = reader.ReadInt32();

			} catch (System.Exception ex) { Game.LogException(ex); }
			reader.BaseStream.Position = endPos;
		}

		public void SaveToBinary_v0 (BinaryWriter writer) {
			long markPos = writer.BaseStream.Position;
			writer.Write((uint)0);
			long startPos = writer.BaseStream.Position;
			try {
				// Name
				int len = Name.Length.Clamp(0, 255);
				writer.Write((byte)len);
				for (int i = 0; i < len; i++) {
					writer.Write((byte)Name[i]);
				}

				// Type
				writer.Write((byte)Type);

				// Z
				writer.Write((int)AtlasZ);

			} catch (System.Exception ex) { Game.LogException(ex); }
			long endPos = writer.BaseStream.Position;
			writer.BaseStream.Position = markPos;
			writer.Write((uint)(endPos - startPos));
			writer.BaseStream.Position = endPos;
		}

	}



	// Sheet
	public class EditableSheet : PoolingSheet {

		public readonly List<Byte4[]> SpritePixels = new();

		public override void SetData (List<AngeSprite> sprites, List<SpriteGroup> groups, List<AtlasInfo> atlasInfo, object texture) {
			base.SetData(sprites, groups, atlasInfo, texture);
			FillPixels();
		}

		public override bool LoadFromDisk (string path) {
			bool loaded = base.LoadFromDisk(path);
			FillPixels();
			return loaded;
		}

		public override void SaveToDisk (string path) {
			// Reconstruct
			



			// Save
			base.SaveToDisk(path);
		}

		public override void Clear () {
			base.Clear();
			SpritePixels.Clear();
		}

		private void FillPixels () {
			SpritePixels.Clear();
			if (Texture == null) return;
			var texturePixels = Game.GetPixelsFromTexture(Texture);
			var size = Game.GetTextureSize(Texture);
			int pixelLen = texturePixels.Length;
			int textureWidth = size.x;
			foreach (var sprite in Sprites) {
				int x = sprite.TextureRect.x;
				int y = sprite.TextureRect.y;
				int width = sprite.TextureRect.width;
				int height = sprite.TextureRect.height;
				var pixels = new Byte4[width * height];
				for (int j = 0; j < height; j++) {
					int targetY = j + y;
					for (int i = 0; i < width; i++) {
						int targetX = i + x;
						int targetIndex = targetY * textureWidth + targetX;
						pixels[j * width + i] = targetIndex >= 0 && targetIndex < pixelLen ?
							texturePixels[targetIndex] : Const.CLEAR;
					}
				}
				SpritePixels.Add(pixels);
			}
		}

	}


	public class PoolingSheet : Sheet {

		public readonly Dictionary<int, AngeSprite> SpritePool = new();
		public readonly Dictionary<int, SpriteGroup> GroupPool = new();

		public override void SetData (List<AngeSprite> sprites, List<SpriteGroup> groups, List<AtlasInfo> atlasInfo, object texture) {
			base.SetData(sprites, groups, atlasInfo, texture);
			FillPool();
		}

		public override bool LoadFromDisk (string path) {
			bool loaded = base.LoadFromDisk(path);
			FillPool();
			return loaded;
		}

		public override void Clear () {
			base.Clear();
			SpritePool.Clear();
			GroupPool.Clear();
		}

		private void FillPool () {
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
		}

	}


	public class Sheet {

		// VAR
		public bool NotEmpty => Sprites.Count > 0;
		public object Texture = null;
		public readonly List<AngeSprite> Sprites = new();
		public readonly List<SpriteGroup> Groups = new();
		public readonly List<AtlasInfo> AtlasInfo = new();


		// MSG
		public Sheet () { }
		public Sheet (List<AngeSprite> sprites, List<SpriteGroup> groups, List<AtlasInfo> atlasInfo, object texture) => SetData(sprites, groups, atlasInfo, texture);

		// API
		public virtual void SetData (List<AngeSprite> sprites, List<SpriteGroup> groups, List<AtlasInfo> atlasInfo, object texture) {
			Sprites.Clear();
			Sprites.AddRange(sprites);
			Groups.Clear();
			Groups.AddRange(groups);
			AtlasInfo.Clear();
			AtlasInfo.AddRange(atlasInfo);
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
			AtlasInfo.Clear();
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
				sp.Atlas = AtlasInfo[sp.AtlasIndex];
				sp.SortingZ = sp.Atlas.AtlasZ * 1024 + sp.LocalZ;
			}
			for (int i = 0; i < Groups.Count; i++) {
				var group = Groups[i];
				if (group.SpriteIndexes != null && group.SpriteIndexes.Length > 0) {
					group.Sprites = new AngeSprite[group.SpriteIndexes.Length];
					for (int j = 0; j < group.SpriteIndexes.Length; j++) {
						int index = group.SpriteIndexes[j];
						var sp = Sprites[index];
						sp.GroupType = group.Type;
						group.Sprites[j] = sp;
					}
				} else {
					group.Sprites = new AngeSprite[0];
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
			AtlasInfo.Clear();
			try {
				for (int i = 0; i < atlasCount; i++) {
					var atlas = new AtlasInfo();
					AtlasInfo.Add(atlas);
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
					writer.Write((int)AtlasInfo.Count);
					long markPos = stream.Position;
					writer.Write((int)0);
					long startPos = stream.Position;
					for (int i = 0; i < AtlasInfo.Count; i++) {
						AtlasInfo[i].SaveToBinary_v0(writer);
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
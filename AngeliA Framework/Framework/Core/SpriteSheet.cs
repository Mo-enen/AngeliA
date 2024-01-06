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
	public class SpriteCode {
		public readonly string Name;
		public readonly int ID;
		public readonly int Width;
		public readonly int Height;
		public SpriteCode (string name, int width, int height) {
			Name = name;
			ID = name.AngeHash();
			Width = width;
			Height = height;
		}
		public static implicit operator SpriteCode ((string str, int width, int height) value) => new(value.str, value.width, value.height);
		public static implicit operator int (SpriteCode code) => code.ID;
	}


	public class AngeSprite {

		const float UV_SCALE = 10000000f;
		private static readonly StringBuilder CacheBuilder = new(256);

		public int GlobalID;
		public string RealName;
		public int GlobalWidth;
		public int GlobalHeight;
		public int PivotX;
		public int PivotY;
		public int SortingZ;
		public Int4 GlobalBorder;

		public FRect UvRect;
		public Float4 UvBorder; // xyzw => ldru

		public int AtlasIndex;
		public AtlasInfo Atlas;
		public Byte4 SummaryTint;
		public GroupType? GroupType;
		public bool IsTrigger;
		public int Rule;
		public int Tag;

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
				RealName = CacheBuilder.ToString();

				// ID
				GlobalID = RealName.AngeHash();

				// Size
				GlobalWidth = reader.ReadUInt16();
				GlobalHeight = reader.ReadUInt16();

				// Pivot
				PivotX = reader.ReadInt16();
				PivotY = reader.ReadInt16();

				// Sorting Z
				SortingZ = reader.ReadInt32();

				// Global Border
				GlobalBorder = new Int4(
					reader.ReadUInt16(),
					reader.ReadUInt16(),
					reader.ReadUInt16(),
					reader.ReadUInt16()
				);

				// UV
				uint uvMinX = reader.ReadUInt32();
				uint uvMinY = reader.ReadUInt32();
				uint uvMaxX = reader.ReadUInt32();
				uint uvMaxY = reader.ReadUInt32();
				uint uvBorderL = reader.ReadUInt32();
				uint uvBorderR = reader.ReadUInt32();
				uint uvBorderD = reader.ReadUInt32();
				uint uvBorderU = reader.ReadUInt32();

				UvRect = FRect.MinMaxRect(
					uvMinX / UV_SCALE, uvMinY / UV_SCALE,
					uvMaxX / UV_SCALE, uvMaxY / UV_SCALE
				);
				//UvBottomRight = new(uvMaxX / UV_SCALE, uvMinY / UV_SCALE);
				//UvTopLeft = new(uvMinX / UV_SCALE, uvMaxY / UV_SCALE);
				//UvBottomLeft = new(uvMinX / UV_SCALE, uvMinY / UV_SCALE);
				//UvTopRight = new(uvMaxX / UV_SCALE, uvMaxY / UV_SCALE);
				UvBorder = new(uvBorderL / UV_SCALE, uvBorderD / UV_SCALE, uvBorderR / UV_SCALE, uvBorderU / UV_SCALE);

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

				// Sorting Z
				writer.Write((int)SortingZ);

				// Global Border
				writer.Write((ushort)GlobalBorder.x);
				writer.Write((ushort)GlobalBorder.y);
				writer.Write((ushort)GlobalBorder.z);
				writer.Write((ushort)GlobalBorder.w);

				// UV
				writer.Write((uint)(UvRect.x * UV_SCALE).RoundToInt());
				writer.Write((uint)(UvRect.y * UV_SCALE).RoundToInt());
				writer.Write((uint)(UvRect.xMax * UV_SCALE).RoundToInt());
				writer.Write((uint)(UvRect.yMax * UV_SCALE).RoundToInt());
				writer.Write((uint)(UvBorder.x * UV_SCALE).RoundToInt());
				writer.Write((uint)(UvBorder.z * UV_SCALE).RoundToInt());
				writer.Write((uint)(UvBorder.y * UV_SCALE).RoundToInt());
				writer.Write((uint)(UvBorder.w * UV_SCALE).RoundToInt());

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
			} catch (System.Exception ex) { Game.LogException(ex); }
			long endPos = writer.BaseStream.Position;
			writer.BaseStream.Position = markPos;
			writer.Write((uint)(endPos - startPos));
			writer.BaseStream.Position = endPos;
		}

	}



	// Sheet
	public class Sheet {

		// VAR
		public bool NotEmpty => Sprites.Length > 0;
		public AngeSprite[] Sprites { get; private set; } = System.Array.Empty<AngeSprite>();
		public SpriteGroup[] Groups { get; private set; } = System.Array.Empty<SpriteGroup>();
		public AtlasInfo[] AtlasInfo { get; private set; } = System.Array.Empty<AtlasInfo>();
		public Dictionary<int, AngeSprite> SpritePool { get; } = new();
		public Dictionary<int, SpriteGroup> GroupPool { get; } = new();

		// MSG
		public Sheet () { }
		public Sheet (string path) => LoadFromDisk(path);
		public Sheet (AngeSprite[] sprites, SpriteGroup[] groups, AtlasInfo[] atlasInfo) {
			Sprites = sprites;
			Groups = groups;
			AtlasInfo = atlasInfo;
			ApplyData();
			SpritePool.Clear();
			GroupPool.Clear();
		}

		// API
		public void LoadFromDisk (string path) {

			Clear();
			if (!Util.FileExists(path)) return;

			// Load Data
			using var stream = new FileStream(path, FileMode.Open);
			using var reader = new BinaryReader(stream);
			int fileVersion = reader.ReadInt32();
			switch (fileVersion) {
				case 0:
					LoadFromBinary_v0(reader);
					break;
				default:
					Game.LogError($"Can not handle sheet version {fileVersion}. Expect: version-0");
					break;
			}

			// Apply Instance
			ApplyData();

			// Fill Sprites
			for (int i = 0; i < Sprites.Length; i++) {
				var sp = Sprites[i];
				SpritePool.TryAdd(sp.GlobalID, Sprites[i]);
			}

			// Fill Groups
			for (int i = 0; i < Groups.Length; i++) {
				var group = Groups[i];
				GroupPool.TryAdd(group.ID, group);
			}

		}

		public void SaveToDisk (string path) {
			using var stream = new FileStream(path, FileMode.Create);
			using var writer = new BinaryWriter(stream);
			writer.Write((int)0); // File Version
			SaveToBinary_v0(writer);
		}

		public void Clear () {
			SpritePool.Clear();
			GroupPool.Clear();
			Sprites = System.Array.Empty<AngeSprite>();
			Groups = System.Array.Empty<SpriteGroup>();
			AtlasInfo = System.Array.Empty<AtlasInfo>();
		}

		public void ShiftUvToUserSpace () {
			for (int i = 0; i < Sprites.Length; i++) {
				Sprites[i].UvRect.x += 1f;
			}
		}

		// LGC
		private void ApplyData () {
			for (int i = 0; i < Sprites.Length; i++) {
				var sp = Sprites[i];
				sp.Atlas = AtlasInfo[sp.AtlasIndex];
			}
			for (int i = 0; i < Groups.Length; i++) {
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

			// Sprites
			int spriteCount = reader.ReadInt32();
			int spriteByteLength = reader.ReadInt32();
			long spriteEndPos = stream.Position + spriteByteLength;
			Sprites = new AngeSprite[spriteCount];
			try {
				for (int i = 0; i < spriteCount; i++) {
					var sprite = Sprites[i] = new AngeSprite();
					sprite.LoadFromBinary_v0(reader);
				}
			} catch (System.Exception ex) { Game.LogException(ex); }
			if (stream.Position != spriteEndPos) stream.Position = spriteEndPos;

			// Groups
			int groupCount = reader.ReadInt32();
			int groupByteLength = reader.ReadInt32();
			long groupEndPos = stream.Position + groupByteLength;
			Groups = new SpriteGroup[groupCount];
			try {
				for (int i = 0; i < groupCount; i++) {
					var group = Groups[i] = new SpriteGroup();
					group.LoadFromBinary_v0(reader);
				}
			} catch (System.Exception ex) { Game.LogException(ex); }
			if (stream.Position != groupEndPos) stream.Position = groupEndPos;

			// Atlas
			int atlasCount = reader.ReadInt32();
			int atlasByteLength = reader.ReadInt32();
			long atlasEndPos = stream.Position + atlasByteLength;
			AtlasInfo = new AtlasInfo[atlasCount];
			try {
				for (int i = 0; i < atlasCount; i++) {
					var atlas = AtlasInfo[i] = new AtlasInfo();
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
					writer.Write((int)Sprites.Length);
					long markPos = stream.Position;
					writer.Write((int)0);
					long startPos = stream.Position;
					for (int i = 0; i < Sprites.Length; i++) {
						Sprites[i].SaveToBinary_v0(writer);
					}
					long endPos = stream.Position;
					stream.Position = markPos;
					writer.Write((int)(endPos - startPos));
					stream.Position = endPos;
				}

				// Groups
				{
					writer.Write((int)Groups.Length);
					long markPos = stream.Position;
					writer.Write((int)0);
					long startPos = stream.Position;
					for (int i = 0; i < Groups.Length; i++) {
						Groups[i].SaveToBinary_v0(writer);
					}
					long endPos = stream.Position;
					stream.Position = markPos;
					writer.Write((int)(endPos - startPos));
					stream.Position = endPos;
				}

				// Atlas
				{
					writer.Write((int)AtlasInfo.Length);
					long markPos = stream.Position;
					writer.Write((int)0);
					long startPos = stream.Position;
					for (int i = 0; i < AtlasInfo.Length; i++) {
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
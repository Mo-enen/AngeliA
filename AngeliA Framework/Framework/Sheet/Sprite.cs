using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace AngeliaFramework {


	public class AngeSprite {

		private static readonly StringBuilder CacheBuilder = new(256);

		public int GlobalID;
		public string RealName;
		public int GlobalWidth;
		public int GlobalHeight;
		public int PixelWidth;
		public int PixelHeight;
		public int PivotX;
		public int PivotY;
		public int SortingZ;
		public int LocalZ;
		public Int4 GlobalBorder;
		public int AtlasIndex;
		public Atlas Atlas;
		public SpriteGroup Group;
		public bool IsTrigger;
		public int Rule;
		public int Tag;
		public Byte4 SummaryTint;
		public Byte4[] Pixels;
		public object Texture;

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
				PixelWidth = reader.ReadUInt16();
				PixelHeight = reader.ReadUInt16();
				GlobalWidth = PixelWidth * Const.ART_SCALE;
				GlobalHeight = PixelHeight * Const.ART_SCALE;

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

				// Group
				Group = null;

				// Pixels
				var bytes = reader.ReadBytes(PixelWidth * PixelHeight * 4);
				Pixels = AngeUtil.Bytes_to_Pixels(bytes, PixelWidth, PixelHeight);
				Texture = Game.GetTextureFromPixels(Pixels, PixelWidth, PixelHeight);

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
				writer.Write((ushort)PixelWidth);
				writer.Write((ushort)PixelHeight);

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

				// Pixels
				var bytes = AngeUtil.Pixels_to_Bytes(Pixels, PixelWidth, PixelHeight);
				writer.Write(bytes);

			} catch (System.Exception ex) { Game.LogException(ex); }
			long endPos = writer.BaseStream.Position;
			writer.BaseStream.Position = markPos;
			writer.Write((uint)(endPos - startPos));
			writer.BaseStream.Position = endPos;
		}

		public string GetFullName () {

			CacheBuilder.Clear();
			CacheBuilder.Append(RealName);

			// Trigger
			if (IsTrigger) {
				CacheBuilder.Append(" #IsTrigger");
			}

			// Tag
			if (Tag != 0) {
				for (int i = 0; i < SpriteTag.ALL_TAGS.Length; i++) {
					if (SpriteTag.ALL_TAGS[i] == Tag) {
						CacheBuilder.Append(" #tag=");
						CacheBuilder.Append(SpriteTag.ALL_TAGS_STRING[i]);
						break;
					}
				}
			}

			// No Collider
			if (
				Atlas.Type == AtlasType.Level &&
				(GlobalBorder.horizontal >= GlobalWidth || GlobalBorder.vertical >= GlobalHeight)
			) {
				CacheBuilder.Append(" #noCollider");
			}

			if (Group != null) {
				// Ani
				switch (Group.Type) {
					case GroupType.Rule:
						// Rule
						if (Rule != 0) {
							CacheBuilder.Append(" #rule=");
							CacheBuilder.Append(AngeUtil.RuleDigitToString(Rule));
						}
						break;
					case GroupType.Random:
						// Ran
						CacheBuilder.Append(" #ran");
						break;
					case GroupType.Animated:
						// Ani
						CacheBuilder.Append(" #ani");
						// Loopstart
						if (
							Group.LoopStart >= 0 && Group.LoopStart < Group.SpriteIDs.Count &&
							Group.SpriteIDs[Group.LoopStart] == GlobalID
						) {
							CacheBuilder.Append(" #loopStart");
						}
						break;
				}
			}

			// Z
			if (LocalZ != 0) {
				CacheBuilder.Append(" #z=");
				CacheBuilder.Append(LocalZ);
			}

			string result = CacheBuilder.ToString();
			CacheBuilder.Clear();
			return result;
		}

	}


	public class FlexSprite {
		public static readonly FlexSprite PIXEL = new() {
			AngePivot = default,
			AtlasName = "(Procedure)",
			AtlasType = AtlasType.General,
			AtlasZ = 0,
			Border = default,
			FullName = "Pixel",
			Pixels = new Byte4[1] { Const.WHITE },
			Size = new(1, 1),
		};
		public string FullName;
		public Int2 AngePivot;
		public Int4 Border;
		public Int2 Size;
		public int AtlasZ;
		public string AtlasName;
		public AtlasType AtlasType;
		public Byte4[] Pixels;
	}


}
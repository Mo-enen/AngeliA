using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace AngeliaFramework {


	public enum GroupType {
		General = 0,
		Rule = 1,
		Random = 2,
		Animated = 3,
	}


	public class SpriteGroup {

		public AngeSprite this[int i] => Sprites[i];
		public int Length => Sprites.Count;

		public int ID;
		public int LoopStart;
		public GroupType Type;
		public List<int> SpriteIndexes;
		public List<AngeSprite> Sprites;

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
				SpriteIndexes = new List<int>(len);
				for (int i = 0; i < len; i++) {
					SpriteIndexes.Add(reader.ReadInt32());
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
				writer.Write((ushort)SpriteIndexes.Count);
				for (int i = 0; i < SpriteIndexes.Count; i++) {
					writer.Write((int)SpriteIndexes[i]);
				}

			} catch (System.Exception ex) { Game.LogException(ex); }
			long endPos = writer.BaseStream.Position;
			writer.BaseStream.Position = markPos;
			writer.Write((uint)(endPos - startPos));
			writer.BaseStream.Position = endPos;
		}

	}
}

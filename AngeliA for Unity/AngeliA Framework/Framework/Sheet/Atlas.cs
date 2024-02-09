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


	public class Atlas {

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
}
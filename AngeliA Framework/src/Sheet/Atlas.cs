using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AngeliA;

public enum AtlasType {
	General = 0,
	Level = 1,
	Background = 2,
}


public class Atlas {

	private static readonly StringBuilder CacheBuilder = new(256);
	public int ID;
	public string Name;
	public AtlasType Type;
	public int IndentLevel;

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

			// ID
			ID = Name.AngeHash();

			// Type
			Type = (AtlasType)reader.ReadByte();

			// Indent
			IndentLevel = reader.ReadInt32();

		} catch (System.Exception ex) {
			Debug.LogException(ex);
		}
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

			// Indent
			writer.Write((int)IndentLevel);

		} catch (System.Exception ex) {
			Debug.LogException(ex);
		}
		long endPos = writer.BaseStream.Position;
		writer.BaseStream.Position = markPos;
		writer.Write((uint)(endPos - startPos));
		writer.BaseStream.Position = endPos;
	}

}
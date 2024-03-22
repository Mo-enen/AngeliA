using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AngeliA;


public enum GroupType {
	General = 0,
	Rule = 1,
	Random = 2,
	Animated = 3,
}


public class SpriteGroup {

	private static readonly StringBuilder CacheBuilder = new(256);

	public int Count => SpriteIDs.Count;

	public int ID;
	public string Name;
	public int LoopStart;
	public GroupType Type;
	public List<int> SpriteIDs;

	public void LoadFromBinary_v0 (BinaryReader reader, Action<Exception> exceptionHandler) {
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

			// Group Type
			Type = (GroupType)reader.ReadByte();

			if (Type == GroupType.Animated) {
				// Loop Start
				LoopStart = reader.ReadInt16();
			}

			// Sprite Indexes
			int spriteLen = reader.ReadUInt16();
			SpriteIDs = new List<int>(spriteLen);
			for (int i = 0; i < spriteLen; i++) {
				SpriteIDs.Add(reader.ReadInt32());
			}

		} catch (Exception ex) { exceptionHandler?.Invoke(ex); }
		reader.BaseStream.Position = endPos;
	}

	public void SaveToBinary_v0 (BinaryWriter writer, Action<Exception> exceptionHandler) {
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

			// Group Type
			writer.Write((byte)Type);

			if (Type == GroupType.Animated) {
				// Loop Start
				writer.Write((short)LoopStart);
			}

			// Sprite Indexes
			int spriteCount = SpriteIDs.Count;
			writer.Write((ushort)spriteCount);
			for (int i = 0; i < spriteCount; i++) {
				writer.Write((int)SpriteIDs[i]);
			}

		} catch (Exception ex) { exceptionHandler?.Invoke(ex); }

		long endPos = writer.BaseStream.Position;
		writer.BaseStream.Position = markPos;
		writer.Write((uint)(endPos - startPos));
		writer.BaseStream.Position = endPos;
	}

}

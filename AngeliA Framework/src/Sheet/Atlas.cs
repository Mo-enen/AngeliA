using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AngeliA;

/// <summary>
/// Type of sprite atlas
/// </summary>
public enum AtlasType {
	/// <summary>
	/// No special info
	/// </summary>
	General = 0,
	/// <summary>
	/// Sprites inside are level blocks
	/// </summary>
	Level = 1,
	/// <summary>
	/// Sprites inside are background blocks
	/// </summary>
	Background = 2,
}


/// <summary>
/// Internal data used for display folder layout
/// </summary>
public enum AtlasState {
	Root = 0,
	Sub = 1,
	Folded = 2,
	Unfolded = 3,
}


/// <summary>
/// Container of sprites inside artwork sheet
/// </summary>
public class Atlas {

	private static readonly StringBuilder CacheBuilder = new(256);
	/// <summary>
	/// Unique ID of this atlas. From Name.AngeHash();
	/// </summary>
	public int ID;
	/// <summary>
	/// Unique name of this atlas
	/// </summary>
	public string Name;
	public AtlasType Type;
	/// <summary>
	/// Internal data used for display folder layout
	/// </summary>
	public AtlasState State;

	/// <summary>
	/// True if this atlas is folder
	/// </summary>
	public bool IsFolder => State == AtlasState.Folded || State == AtlasState.Unfolded;
	/// <summary>
	/// True if this atlas is inside a folder
	/// </summary>
	public bool InFolder => State == AtlasState.Sub;

	internal void LoadFromBinary_v0 (BinaryReader reader) {
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
			State = (AtlasState)reader.ReadInt32();

		} catch (System.Exception ex) {
			Debug.LogException(ex);
		}
		reader.BaseStream.Position = endPos;
	}

	internal void SaveToBinary_v0 (BinaryWriter writer) {
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
			writer.Write((int)State);

		} catch (System.Exception ex) {
			Debug.LogException(ex);
		}
		long endPos = writer.BaseStream.Position;
		writer.BaseStream.Position = markPos;
		writer.Write((uint)(endPos - startPos));
		writer.BaseStream.Position = endPos;
	}

}
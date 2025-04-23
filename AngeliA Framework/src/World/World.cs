using System.Collections;
using System.Collections.Generic;
using System.IO;



namespace AngeliA;

/// <summary>
/// Type of blocks in map
/// </summary>
public enum BlockType {
	/// <summary>
	/// Dynamic object with logic attached
	/// </summary>
	Entity = 0,
	/// <summary>
	/// Static block with collider
	/// </summary>
	Level = 1,
	/// <summary>
	/// Static block without collider
	/// </summary>
	Background = 2,
	/// <summary>
	/// Internal IMapItem that do not spawn into stage
	/// </summary>
	Element = 3,
}


/// <summary>
/// Instance of a 128x128 map data
/// </summary>
public class World {




	#region --- VAR ---


	// Api
	/// <summary>
	/// Position for the bottom-left of the world in world space (1 world space = 256 * 128 global space)
	/// </summary>
	public Int3 WorldPosition { get; set; }
	/// <summary>
	/// ID of all background blocks (index 0 means bottom-left, index 1 make it goes right)
	/// </summary>
	public int[] Backgrounds { get; set; } = new int[Const.MAP * Const.MAP];
	/// <summary>
	/// ID of all level blocks (index 0 means bottom-left, index 1 make it goes right)
	/// </summary>
	public int[] Levels { get; set; } = new int[Const.MAP * Const.MAP];
	/// <summary>
	/// ID of all entity blocks (index 0 means bottom-left, index 1 make it goes right)
	/// </summary>
	public int[] Entities { get; set; } = new int[Const.MAP * Const.MAP];
	/// <summary>
	/// ID of all element blocks (index 0 means bottom-left, index 1 make it goes right)
	/// </summary>
	public int[] Elements { get; set; } = new int[Const.MAP * Const.MAP];


	#endregion




	#region --- API ---


	/// <summary>
	/// Instance of a 128x128 map data
	/// </summary>
	/// <param name="pos">Position in world space (1 world space = 256 * 128 global space)</param>
	public World (Int3 pos) => WorldPosition = pos;


	/// <summary>
	/// Instance of a 128x128 map data
	/// </summary>
	public World () : this(new Int3(int.MinValue, int.MinValue, int.MinValue)) { }


	/// <summary>
	/// True if the world data is empty
	/// </summary>
	public bool EmptyCheck () {
		foreach (var a in Entities) if (a != 0) return false;
		foreach (var a in Levels) if (a != 0) return false;
		foreach (var a in Backgrounds) if (a != 0) return false;
		foreach (var a in Elements) if (a != 0) return false;
		return true;
	}


	/// <summary>
	/// True if the given block ID exists inside this world
	/// </summary>
	/// <param name="blockID"></param>
	/// <param name="type">Type of the block</param>
	public bool ContainsBlock (int blockID, BlockType type) {
		switch (type) {
			case BlockType.Entity:
				foreach (var e in Entities) if (e == blockID) return true;
				break;
			case BlockType.Level:
				foreach (var lv in Levels) if (lv == blockID) return true;
				break;
			case BlockType.Background:
				foreach (var bg in Backgrounds) if (bg == blockID) return true;
				break;
			case BlockType.Element:
				foreach (var ele in Elements) if (ele == blockID) return true;
				break;
		}
		return false;
	}


	public void CopyFrom (World source) {
		WorldPosition = source.WorldPosition;
		System.Array.Copy(source.Backgrounds, Backgrounds, Backgrounds.Length);
		System.Array.Copy(source.Levels, Levels, Levels.Length);
		System.Array.Copy(source.Entities, Entities, Entities.Length);
		System.Array.Copy(source.Elements, Elements, Elements.Length);
	}


	/// <summary>
	/// Reset all block data inside this world data
	/// </summary>
	/// <param name="pos">Set new world position. Set to null to not clear world position</param>
	public void Clear (Int3? pos = null) {
		WorldPosition = pos ?? WorldPosition;
		System.Array.Clear(Levels, 0, Levels.Length);
		System.Array.Clear(Backgrounds, 0, Backgrounds.Length);
		System.Array.Clear(Entities, 0, Entities.Length);
		System.Array.Clear(Elements, 0, Elements.Length);
	}


	// Load
	public bool LoadFromDisk (string filePath, int worldX, int worldY, int worldZ) {

		bool success = false;

		try {

			System.Array.Clear(Levels, 0, Levels.Length);
			System.Array.Clear(Backgrounds, 0, Backgrounds.Length);
			System.Array.Clear(Entities, 0, Entities.Length);
			System.Array.Clear(Elements, 0, Elements.Length);

			WorldPosition = new(worldX, worldY, worldZ);

			if (!Util.FileExists(filePath)) return success;

			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII);

			// Load Content
			while (reader.NotEnd()) {
				int id = reader.ReadInt32();
				int x = reader.ReadByte();
				int y = reader.ReadByte();
				if (x < Const.MAP) {
					if (y < Const.MAP) {
						// Entity x y
						if (x < 0 || x >= Const.MAP || y < 0 || y >= Const.MAP || id == 0) continue;
						if (Entities[y * Const.MAP + x] != 0) continue;
						Entities[y * Const.MAP + x] = id;
					} else {
						// Element x yy
						y -= Const.MAP;
						if (x < 0 || x >= Const.MAP || y < 0 || y >= Const.MAP || id == 0) continue;
						if (Elements[y * Const.MAP + x] != 0) continue;
						Elements[y * Const.MAP + x] = id;
					}
				} else {
					if (y < Const.MAP) {
						// Level xx y
						x -= Const.MAP;
						if (x < 0 || x >= Const.MAP || y < 0 || y >= Const.MAP || id == 0) continue;
						if (Levels[y * Const.MAP + x] != 0) continue;
						Levels[y * Const.MAP + x] = id;
					} else {
						// Background xx yy
						x -= Const.MAP;
						y -= Const.MAP;
						if (x < 0 || x >= Const.MAP || y < 0 || y >= Const.MAP || id == 0) continue;
						if (Backgrounds[y * Const.MAP + x] != 0) continue;
						Backgrounds[y * Const.MAP + x] = id;
					}
				}
			}
			success = true;
		} catch (System.Exception ex) { Debug.LogException(ex); }

		// Final
		return success;
	}


	// Save
	public void SaveToDisk (string filePath) {

		if (string.IsNullOrEmpty(filePath)) return;

		try {

			// Save
			const int SIZE = Const.MAP;
			using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
			using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII);

			// Background
			for (int y = 0; y < SIZE; y++) {
				for (int x = 0; x < SIZE; x++) {
					int id = Backgrounds[y * SIZE + x];
					if (id == 0) continue;
					writer.Write((int)id);
					writer.Write((byte)(x + SIZE));
					writer.Write((byte)(y + SIZE));
				}
			}

			// Level
			for (int y = 0; y < SIZE; y++) {
				for (int x = 0; x < SIZE; x++) {
					int id = Levels[y * SIZE + x];
					if (id == 0) continue;
					writer.Write((int)id);
					writer.Write((byte)(x + SIZE));
					writer.Write((byte)y);
				}
			}

			// Entity
			for (int y = 0; y < SIZE; y++) {
				for (int x = 0; x < SIZE; x++) {
					int id = Entities[y * SIZE + x];
					if (id == 0) continue;
					writer.Write((int)id);
					writer.Write((byte)x);
					writer.Write((byte)y);
				}
			}

			// Element
			for (int y = 0; y < SIZE; y++) {
				for (int x = 0; x < SIZE; x++) {
					int id = Elements[y * SIZE + x];
					if (id == 0) continue;
					writer.Write((int)id);
					writer.Write((byte)x);
					writer.Write((byte)(y + SIZE));
				}
			}

		} catch (System.Exception ex) { Debug.LogException(ex); }

	}


	#endregion




}
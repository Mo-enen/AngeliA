using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace AngeliA.Framework;

public enum BlockType {
	Entity = 0,
	Level = 1,
	Background = 2,
	Element = 3,
}

public class World {




	#region --- VAR ---


	// Api
	public Int3 WorldPosition { get; set; } = default;
	public int[] Backgrounds { get; set; } = null;
	public int[] Levels { get; set; } = null;
	public int[] Entities { get; set; } = null;
	public int[] Elements { get; set; } = null;

	// Data
	private static readonly Dictionary<Int3, string> WorldNamePool = new();
	private static readonly Color32[] CacheMapPixels = new Color32[Const.MAP * Const.MAP];


	#endregion




	#region --- API ---


	public World () : this(new(int.MinValue, int.MinValue)) { }
	public World (Int3 pos) => Reset(pos);


	public bool EmptyCheck () {
		foreach (var a in Entities) if (a != 0) return false;
		foreach (var a in Levels) if (a != 0) return false;
		foreach (var a in Backgrounds) if (a != 0) return false;
		foreach (var a in Elements) if (a != 0) return false;
		return true;
	}


	public void CopyFrom (World source) {
		WorldPosition = source.WorldPosition;
		System.Array.Copy(source.Backgrounds, Backgrounds, Backgrounds.Length);
		System.Array.Copy(source.Levels, Levels, Levels.Length);
		System.Array.Copy(source.Entities, Entities, Entities.Length);
		System.Array.Copy(source.Elements, Elements, Elements.Length);
	}


	public void Reset (Int3 pos) {
		WorldPosition = pos;
		Levels = new int[Const.MAP * Const.MAP];
		Backgrounds = new int[Const.MAP * Const.MAP];
		Entities = new int[Const.MAP * Const.MAP];
		Elements = new int[Const.MAP * Const.MAP];
	}


	// Load
	public bool LoadFromDisk (string mapFile) =>
		GetWorldPositionFromName(Util.GetNameWithoutExtension(mapFile), out var pos) &&
		LoadFromDiskLogic(mapFile, pos.x, pos.y, pos.z);


	public bool LoadFromDisk (string mapFolder, int worldX, int worldY, int worldZ) =>
		LoadFromDiskLogic(
			Util.CombinePaths(mapFolder, GetWorldNameFromPosition(worldX, worldY, worldZ)),
			worldX, worldY, worldZ
		);


	public static void ForAllEntities (string filePath, System.Action<int, int, int> callback) {
		if (!Util.FileExists(filePath)) return;
		using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII);
		int SIZE = Const.MAP;
		while (reader.NotEnd()) {
			int id = reader.ReadInt32();
			int x = reader.ReadByte();
			int y = reader.ReadByte();
			if (x < Const.MAP) {
				// Entity x y
				if (x < 0 || x >= SIZE || y < 0 || y >= SIZE || id == 0) continue;
				callback.Invoke(id, x, y);
			}
		}
	}


	public static bool LoadMapIntoTexture (string mapFolder, int worldX, int worldY, int worldZ, object texture) {
		if (texture == null) return false;
		string filePath = Util.CombinePaths(mapFolder, GetWorldNameFromPosition(worldX, worldY, worldZ));
		System.Array.Clear(CacheMapPixels, 0, CacheMapPixels.Length);
		if (!Util.FileExists(filePath)) {
			Game.FillPixelsIntoTexture(CacheMapPixels, texture);
			return false;
		}
		using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII);
		int SIZE = Const.MAP;
		while (reader.NotEnd()) {
			int id = reader.ReadInt32();
			int x = reader.ReadByte();
			int y = reader.ReadByte();
			if (x < SIZE && y >= SIZE) continue;
			if (CellRenderer.TryGetSprite(id, out var sprite)) {
				if (x >= SIZE) x -= SIZE;
				if (y >= SIZE) y -= SIZE;
				CacheMapPixels[y * SIZE + x] = sprite.SummaryTint;
			}
		}
		Game.FillPixelsIntoTexture(CacheMapPixels, texture);
		return true;

	}


	public void FillMapIntoTexture (object texture) {
		lock (CacheMapPixels) {
			int len = Const.MAP * Const.MAP;
			for (int i = 0; i < len; i++) {
				int id = Entities[i];
				if (id != 0 && CellRenderer.TryGetSprite(id, out var sprite)) {
					CacheMapPixels[i] = sprite.SummaryTint;
					continue;
				}
				id = Levels[i];
				if (id != 0 && CellRenderer.TryGetSprite(id, out sprite)) {
					CacheMapPixels[i] = sprite.SummaryTint;
					continue;
				}
				id = Backgrounds[i];
				if (id != 0 && CellRenderer.TryGetSprite(id, out sprite)) {
					CacheMapPixels[i] = sprite.SummaryTint;
					continue;
				}
				CacheMapPixels[i] = Color32.CLEAR;
			}
			Game.FillPixelsIntoTexture(CacheMapPixels, texture);
		}
	}


	// Save
	public void SaveToDisk (string mapFolder) => SaveToDisk(mapFolder, WorldPosition.x, WorldPosition.y, WorldPosition.z);


	public void SaveToDisk (string mapFolder, int worldX, int worldY, int worldZ) {


		try {

			// Save
			const int SIZE = Const.MAP;
			string path = Util.CombinePaths(mapFolder, GetWorldNameFromPosition(worldX, worldY, worldZ));
			using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
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

			// Empty Check
			if (stream.Position == 0 && Util.FileExists(path)) {
				stream.Close();
				writer.Close();
				Util.DeleteFile(path);
			}
		} catch (System.Exception ex) { Game.LogException(ex); }

	}


	// Misc
	public static bool GetWorldPositionFromName (string fileName, out Int3 pos) {
		pos = default;
		int _index0 = fileName.IndexOf('_');
		if (_index0 < 0) return false;
		int _index1 = fileName.IndexOf('_', _index0 + 1);
		if (_index1 < 0) return false;
		if (
			int.TryParse(fileName[.._index0], out int x) &&
			int.TryParse(fileName[(_index0 + 1)..(_index1)], out int y) &&
			int.TryParse(fileName[(_index1 + 1)..], out int z)
		) {
			pos = new(x, y, z);
			return true;
		}
		pos = default;
		return false;
	}


	public static string GetWorldNameFromPosition (int x, int y, int z) {
		var pos = new Int3(x, y, z);
		if (WorldNamePool.TryGetValue(pos, out string name)) {
			return name;
		} else {
			string newName = $"{x}_{y}_{z}.{AngePath.MAP_FILE_EXT}";
			WorldNamePool[pos] = newName;
			return newName;
		}
	}


	#endregion




	#region --- LGC ---


	private bool LoadFromDiskLogic (string filePath, int worldX, int worldY, int worldZ) {

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
				try {
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
				} catch (System.Exception ex) { Game.LogException(ex); }
			}
		} catch (System.Exception ex) { Game.LogException(ex); }

		// Final
		success = true;
		return success;
	}


	#endregion




}
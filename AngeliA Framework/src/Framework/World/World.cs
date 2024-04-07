using System.Collections;
using System.Collections.Generic;
using System.IO;



namespace AngeliA;

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


	public static bool LoadMapIntoTexture (string filePath, object texture) {
		lock (CacheMapPixels) {
			if (texture == null) return false;
			System.Array.Clear(CacheMapPixels, 0, CacheMapPixels.Length);
			if (string.IsNullOrEmpty(filePath) || !Util.FileExists(filePath)) {
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
				if (Renderer.TryGetSprite(id, out var sprite)) {
					if (x >= SIZE) x -= SIZE;
					if (y >= SIZE) y -= SIZE;
					CacheMapPixels[y * SIZE + x] = sprite.SummaryTint;
				}
			}
			Game.FillPixelsIntoTexture(CacheMapPixels, texture);
			return true;
		}
	}


	public void FillMapIntoTexture (object texture) {
		lock (CacheMapPixels) {
			int len = Const.MAP * Const.MAP;
			for (int i = 0; i < len; i++) {
				int id = Entities[i];
				if (id != 0 && Renderer.TryGetSprite(id, out var sprite)) {
					CacheMapPixels[i] = sprite.SummaryTint;
					continue;
				}
				id = Levels[i];
				if (id != 0 && Renderer.TryGetSprite(id, out sprite)) {
					CacheMapPixels[i] = sprite.SummaryTint;
					continue;
				}
				id = Backgrounds[i];
				if (id != 0 && Renderer.TryGetSprite(id, out sprite)) {
					CacheMapPixels[i] = sprite.SummaryTint;
					continue;
				}
				CacheMapPixels[i] = Color32.CLEAR;
			}
			Game.FillPixelsIntoTexture(CacheMapPixels, texture);
		}
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
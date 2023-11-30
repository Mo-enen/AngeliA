using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;


namespace AngeliaFramework {

	public enum BlockType {
		Entity = 0,
		Level = 1,
		Background = 2,
	}

	public enum MapLocation {
		BuiltIn, User, Procedure, Unknown,
	}

	public class World {




		#region --- VAR ---


		// Api
		public MapLocation LoadedLocation { get; private set; } = MapLocation.Unknown;
		public Vector3Int WorldPosition { get; set; } = default;
		public int[] Background { get; set; } = null;
		public int[] Level { get; set; } = null;
		public int[] Entity { get; set; } = null;

		// Data
		private static readonly Color32[] FILLING_PIXELS = new Color32[Const.MAP * Const.MAP];
		private static readonly object FileStreamingLock = new();


		#endregion




		#region --- API ---


		public World () : this(new(int.MinValue, int.MinValue)) { }
		public World (Vector3Int pos) => Reset(pos);


		public bool EmptyCheck () {
			// Level
			foreach (var a in Entity) if (a != 0) return false;
			foreach (var a in Level) if (a != 0) return false;
			foreach (var a in Background) if (a != 0) return false;
			return true;
		}


		public void CopyFrom (World source) {
			WorldPosition = source.WorldPosition;
			System.Array.Copy(source.Background, Background, Background.Length);
			System.Array.Copy(source.Level, Level, Level.Length);
			System.Array.Copy(source.Entity, Entity, Entity.Length);
		}


		public void Reset (Vector3Int pos) {
			WorldPosition = pos;
			Level = new int[Const.MAP * Const.MAP];
			Background = new int[Const.MAP * Const.MAP];
			Entity = new int[Const.MAP * Const.MAP];
		}


		// Load
		public bool LoadFromDisk (string mapFile, MapLocation location) =>
			GetWorldPositionFromName(Util.GetNameWithoutExtension(mapFile), out var pos) &&
			LoadFromDiskLogic(mapFile, location, pos.x, pos.y, pos.z);


		public bool LoadFromDisk (string mapFolder, MapLocation location, int worldX, int worldY, int worldZ) => LoadFromDiskLogic(
			Util.CombinePaths(mapFolder, GetWorldNameFromPosition(worldX, worldY, worldZ)), location, worldX, worldY, worldZ
		);


		public static void ForAllEntities (string filePath, System.Action<int, int, int> callback) {
			if (!Util.FileExists(filePath)) return;
			lock (FileStreamingLock) {
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
		}


		// Save
		public void SaveToDisk (string mapFolder) => SaveToDisk(mapFolder, WorldPosition.x, WorldPosition.y, WorldPosition.z);


		public void SaveToDisk (string mapFolder, int worldX, int worldY, int worldZ) {

			lock (FileStreamingLock) {

				// Save
				const int SIZE = Const.MAP;
				string path = Util.CombinePaths(mapFolder, GetWorldNameFromPosition(worldX, worldY, worldZ));
				using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
				using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII);

				// Level
				for (int y = 0; y < SIZE; y++) {
					for (int x = 0; x < SIZE; x++) {
						int id = Level[y * SIZE + x];
						if (id == 0) continue;
						writer.Write((int)id);
						writer.Write((byte)(x + SIZE));
						writer.Write((byte)y);
					}
				}

				// Background
				for (int y = 0; y < SIZE; y++) {
					for (int x = 0; x < SIZE; x++) {
						int id = Background[y * SIZE + x];
						if (id == 0) continue;
						writer.Write((int)id);
						writer.Write((byte)(x + SIZE));
						writer.Write((byte)(y + SIZE));
					}
				}

				// Entity
				for (int y = 0; y < SIZE; y++) {
					for (int x = 0; x < SIZE; x++) {
						int id = Entity[y * SIZE + x];
						if (id == 0) continue;
						writer.Write((int)id);
						writer.Write((byte)x);
						writer.Write((byte)y);
					}
				}
			}
		}


		// Misc
		public static bool GetWorldPositionFromName (string fileName, out Vector3Int pos) {
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


		public static string GetWorldNameFromPosition (int x, int y, int z) => $"{x}_{y}_{z}.{Const.MAP_FILE_EXT}";


		public void FillIntoTexture (Texture2D texture, bool ignoreItem = true) {
			if (texture == null) return;
			if (texture.width != Const.MAP || texture.height != Const.MAP) {
				Debug.LogWarning($"Texture size must be {Const.MAP} x {Const.MAP}.");
				return;
			}
			const int LEN = Const.MAP * Const.MAP;
			for (int i = 0; i < LEN; i++) {
				if (!ignoreItem || !ItemSystem.HasItem(Entity[i])) {
					if (Fill(Entity[i])) continue;
				}
				if (Fill(Level[i])) continue;
				if (Fill(Background[i])) continue;
				FILLING_PIXELS[i] = Const.CLEAR;
				bool Fill (int id) {
					if (id == 0 || !CellRenderer.TryGetSprite(id, out var sprite)) return false;
					FILLING_PIXELS[i] = sprite.SummaryTint;
					return true;
				}
			}
			texture.SetPixels32(FILLING_PIXELS);
			texture.Apply();
		}


		#endregion




		#region --- LGC ---


		private bool LoadFromDiskLogic (string filePath, MapLocation location, int worldX, int worldY, int worldZ) {

			bool success = false;
			LoadedLocation = MapLocation.Unknown;

			lock (FileStreamingLock) {

				try {

					System.Array.Clear(Level, 0, Level.Length);
					System.Array.Clear(Background, 0, Background.Length);
					System.Array.Clear(Entity, 0, Entity.Length);

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
									Entity[y * Const.MAP + x] = id;
								} else {
									// ?? x yy
									y -= Const.MAP;
									if (x < 0 || x >= Const.MAP || y < 0 || y >= Const.MAP || id == 0) continue;


								}
							} else {
								if (y < Const.MAP) {
									// Level xx y
									x -= Const.MAP;
									if (x < 0 || x >= Const.MAP || y < 0 || y >= Const.MAP || id == 0) continue;
									Level[y * Const.MAP + x] = id;
								} else {
									// Background xx yy
									x -= Const.MAP;
									y -= Const.MAP;
									if (x < 0 || x >= Const.MAP || y < 0 || y >= Const.MAP || id == 0) continue;
									Background[y * Const.MAP + x] = id;
								}
							}
						} catch (System.Exception ex) { Debug.LogException(ex); }
					}
					success = true;
					LoadedLocation = location;
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			return success;
		}


		#endregion




	}
}
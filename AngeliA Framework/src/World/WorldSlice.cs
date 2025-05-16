using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AngeliA;

public readonly struct WorldSliceRegion (int id, int tag, int x, int y, int z, int w, int h) {




	#region --- VAR ---


	// Api
	public readonly int ID = id;
	public readonly int Tag = tag;
	public readonly int X = x;
	public readonly int Y = y;
	public readonly int Z = z;
	public readonly int W = w;
	public readonly int H = h;

	// Short
	private static string SliceFilePath => Util.CombinePaths(Universe.BuiltIn.UniverseMetaRoot, "WorldSlice");

	// Data
	private static readonly Dictionary<int, WorldSliceRegion> Pool = [];
	private static readonly StringBuilder FileTextBuilder = new();
	private static bool IsPoolDirty = false;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () => LoadSliceFromFile();


	[OnGameQuitting]
	internal static void OnGameQuitting () => SaveSliceToFile();


	#endregion




	#region --- API ---


	public static bool AddSlice (int id, int tag, int x, int y, int z, int w, int h) {
		bool added = Pool.TryAdd(id, new WorldSliceRegion(id, tag, x, y, z, w, h));
		IsPoolDirty = true;
		return added;
	}


	public static bool RemoveSlice (int id) {
		bool removed = Pool.Remove(id);
		IsPoolDirty = true;
		return removed;
	}


	public static void SyncAllSlicesFromMapToPool (string mapFolder, out bool changed) {

		// Check for Changes
		changed = false;
		long sliceMoDate = Util.GetFileModifyDate(SliceFilePath);
		foreach (var path in Util.EnumerateFiles(mapFolder, true, AngePath.MAP_SEARCH_PATTERN)) {
			long mapMoDate = Util.GetFileModifyDate(path);
			if (mapMoDate > sliceMoDate) {
				changed = true;
				break;
			}
		}
		if (!changed) return;

		// Recalculate Pool from Maps
		IsPoolDirty = true;
		Pool.Clear();
		int currentDynamicID = int.MinValue;
		var stream = WorldStream.GetOrCreateStreamFromPool(mapFolder);
		foreach (var path in Util.EnumerateFiles(mapFolder, true, AngePath.MAP_SEARCH_PATTERN)) {
			string name = Util.GetNameWithoutExtension(path);
			if (!WorldPathPool.TryGetWorldPositionFromName(name, out var worldPos)) continue;
			// Get Slices Inside World
			int z = worldPos.z;
			int l = worldPos.x * Const.MAP;
			int r = l + Const.MAP;
			int d = worldPos.y * Const.MAP;
			int u = d + Const.MAP;
			for (int j = d; j < u; j++) {
				for (int i = l; i < r; i++) {
					int id = stream.GetBlockAt(i, j, z, BlockType.Element);
					if (id != WorldSlice.TYPE_ID) continue;
					if (!WorldSlice.TryGetSliceFromMap(stream, i, j, z, out var slice)) continue;
					if (slice.ID == 0) {
						slice = new WorldSliceRegion(
							currentDynamicID, slice.Tag, slice.X, slice.Y, slice.Z, slice.W, slice.H
						);
						currentDynamicID++;
					}
					Pool.TryAdd(slice.ID, slice);
				}
			}
		}
	}


	public static bool LoadSliceFromFile () {
		string path = SliceFilePath;
		if (!Util.FileExists(path)) return false;
		Pool.Clear();
		try {
			using var sr = new StreamReader(path);
			while (sr.Peek() >= 0) {
				int id = int.Parse(sr.ReadLine());
				int tag = int.Parse(sr.ReadLine());
				int x = int.Parse(sr.ReadLine());
				int y = int.Parse(sr.ReadLine());
				int w = int.Parse(sr.ReadLine());
				int h = int.Parse(sr.ReadLine());
				int z = int.Parse(sr.ReadLine());
				Pool.Add(id, new(id, tag, x, y, z, w, h));
			}
		} catch (System.Exception ex) {
			Debug.LogException(ex);
			return false;
		}
		IsPoolDirty = false;
		return true;
	}


	public static void SaveSliceToFile (bool forceSave = false) {
		if (!IsPoolDirty && !forceSave) return;
		FileTextBuilder.Clear();
		foreach (var (_, slice) in Pool) {
			FileTextBuilder.Append(slice.ID);
			FileTextBuilder.Append('\n');
			FileTextBuilder.Append(slice.Tag);
			FileTextBuilder.Append('\n');
			FileTextBuilder.Append(slice.X);
			FileTextBuilder.Append('\n');
			FileTextBuilder.Append(slice.Y);
			FileTextBuilder.Append('\n');
			FileTextBuilder.Append(slice.W);
			FileTextBuilder.Append('\n');
			FileTextBuilder.Append(slice.H);
			FileTextBuilder.Append('\n');
			FileTextBuilder.Append(slice.Z);
			FileTextBuilder.Append('\n');
		}
		Util.TextToFile(FileTextBuilder.ToString(), SliceFilePath);
		FileTextBuilder.Clear();
		IsPoolDirty = false;
	}


	#endregion



}

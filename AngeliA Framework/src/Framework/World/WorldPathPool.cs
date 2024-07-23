using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class WorldPathPool : Dictionary<Int3, string> {

	// VAR
	public string MapRoot { get; private set; } = "";
	private static readonly Dictionary<Int3, string> WorldNamePool = new();

	
	// API
	public void SetMapRoot (string newRoot) {
		MapRoot = newRoot;
		Clear();
	}

	public bool TryGetPath (Int3 worldPos, out string path) {
		path = null;
		if (string.IsNullOrEmpty(MapRoot)) return false;
		if (TryGetValue(worldPos, out path)) return path != null;
		path = Util.CombinePaths(MapRoot, GetWorldNameFromPosition(worldPos.x, worldPos.y, worldPos.z));
		if (!Util.FileExists(path)) path = null;
		Add(worldPos, path);
		return path != null;
	}

	public string GetOrAddPath (Int3 worldPos) {
		if (string.IsNullOrEmpty(MapRoot)) return "";
		if (TryGetValue(worldPos, out string path) && path != null) return path;
		path = Util.CombinePaths(MapRoot, GetWorldNameFromPosition(worldPos.x, worldPos.y, worldPos.z));
		this[worldPos] = path;
		return path;
	}

	public static string GetWorldNameFromPosition (Int3 pos) => GetWorldNameFromPosition(pos.x, pos.y, pos.z);
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

}

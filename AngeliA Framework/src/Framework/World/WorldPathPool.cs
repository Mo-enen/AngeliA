using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class WorldPathPool : Dictionary<Int3, string> {

	// VAR
	private static readonly Dictionary<Int3, string> WorldNamePool = new();
	private string MapRoot = "";

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

	public static unsafe bool TryGetWorldPositionFromName (string name, out Int3 pos) {

		pos = default;

		// Find Index of '_'
		int index0 = name.IndexOf('_');
		if (index0 <= 0 || index0 == name.Length - 1) return false;
		int index1 = name.IndexOf('_', index0 + 1);
		if (index1 <= index0 + 1 || index1 <= 0 || index1 == name.Length - 1) return false;

		// Get Position
		fixed (char* ptr = name) {
			var span = new System.ReadOnlySpan<char>(ptr, name.Length);
			if (
				int.TryParse(span[..index0], out int x) &&
				int.TryParse(span.Slice(index0 + 1, index1 - index0 - 1), out int y) &&
				int.TryParse(span[(index1 + 1)..], out int z)
			) {
				pos.x = x;
				pos.y = y;
				pos.z = z;
				return true;
			}
			return false;
		}

	}

}

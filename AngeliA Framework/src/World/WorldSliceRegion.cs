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
	private static readonly Dictionary<int, WorldSliceRegion> RegionPool = [];
	private static readonly HashSet<int> SliceIdSet = [];
	private static readonly StringBuilder FileTextBuilder = new();
	private static bool IsPoolDirty = false;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		LoadSliceFromFile();
		// Slice Set
		SliceIdSet.Clear();
		if (Renderer.TryGetSpriteGroup(WorldSlice.TYPE_ID, out var group)) {
			for (int i = 0; i < group.Count; i++) {
				var sp = group.Sprites[i];
				if (!sp.Rule.IsEmpty) {
					SliceIdSet.Add(sp.ID);
				}
			}
		}
		SliceIdSet.TrimExcess();
	}


	[OnGameQuitting]
	internal static void OnGameQuitting () => SaveSliceToFile();


	public override string ToString () => $"[Slice] id: {ID}, tag: {Tag}, rect:({X}, {Y}, {W}, {H}), z:{Z}";


	#endregion




	#region --- API ---


	public static bool IsSliceElement (int id) => id != 0 && SliceIdSet.Contains(id);


	// Pool
	public static bool TryGetSliceFromPool (int id, out WorldSliceRegion region) => RegionPool.TryGetValue(id, out region);


	public static bool AddSliceToPool (int id, int tag, int x, int y, int z, int w, int h) {
		bool added = RegionPool.TryAdd(id, new WorldSliceRegion(id, tag, x, y, z, w, h));
		IsPoolDirty = true;
		return added;
	}


	public static bool RemoveSliceFromPool (int id) {
		bool removed = RegionPool.Remove(id);
		IsPoolDirty = true;
		return removed;
	}


	// Map
	public static void SyncAllSlicesFromMapToPool (string mapFolder, out bool changed) {

		// Check for Changes
		changed = false;
		long sliceMoDate = Util.GetFileModifyDate(SliceFilePath);
		if (sliceMoDate != 0) {
			foreach (var path in Util.EnumerateFiles(mapFolder, true, AngePath.MAP_SEARCH_PATTERN)) {
				long mapMoDate = Util.GetFileModifyDate(path);
				if (mapMoDate > sliceMoDate) {
					changed = true;
					break;
				}
			}
		} else {
			changed = true;
		}

		// Only Sync when Changed
		if (!changed) return;

		// Recalculate Pool from Maps
		IsPoolDirty = true;
		RegionPool.Clear();
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
					if (!IsSliceElement(id)) continue;
					if (!TryGetSliceFromMap(stream, i, j, z, out var slice)) continue;
					if (slice.ID == 0) {
						slice = new WorldSliceRegion(
							currentDynamicID, slice.Tag, slice.X, slice.Y, slice.Z, slice.W, slice.H
						);
						currentDynamicID++;
					}
					RegionPool.TryAdd(slice.ID, slice);
				}
			}
		}
	}


	public static bool TryGetSliceFromMap (IBlockSquad squad, int unitX, int unitY, int unitZ, out WorldSliceRegion slice) {
		slice = default;

		// Pivot Check
		if (
			IsSlice(squad, unitX - 1, unitY, unitZ) ||
			IsSlice(squad, unitX, unitY - 1, unitZ) ||
			!IsSlice(squad, unitX + 1, unitY, unitZ) ||
			!IsSlice(squad, unitX, unitY + 1, unitZ)
		) return false;

		const int MAX_DISTANCE = Const.MAP * 2;

		// ID
		int resultID = FrameworkUtil.ReadSystemLetterAngeHash(squad, unitX, unitY - 1, unitZ, Direction4.Right);

		// Tag
		var (tagLV, tagBG, tagEN, tagEL) = squad.GetAllBlocksAt(unitX - 1, unitY - 1, unitZ);
		int resultTag = tagEL != 0 ? tagEL : tagEN != 0 ? tagEN : tagLV != 0 ? tagLV : tagBG;

		// Find Region R
		int r = unitX;
		for (int i = 1; i < MAX_DISTANCE; i++) {
			if (!IsSlice(squad, unitX + i, unitY, unitZ)) break;
			r = unitX + i;
		}

		// Find Region U
		int u = unitY;
		for (int i = 1; i < MAX_DISTANCE; i++) {
			if (!IsSlice(squad, unitX, unitY + i, unitZ)) break;
			u = unitY + i;
		}

		int resultX = unitX;
		int resultY = unitY;
		int resultZ = unitZ;
		int resultW = r - unitX + 1;
		int resultH = u - unitY + 1;

		// Final
		slice = new WorldSliceRegion(resultID, resultTag, resultX, resultY, resultZ, resultW, resultH);
		return true;

		// Func
		static bool IsSlice (IBlockSquad squad, int x, int y, int z) {
			int id = squad.GetBlockAt(x, y, z, BlockType.Element);
			return id != 0 && SliceIdSet.Contains(id);
		}
	}


	public static bool LoadSliceIntoMap (IBlockSquad from, IBlockSquad to, Int3 targetUnitPos, int sliceRegionID, bool clearOriginal = false) {
		if (!TryGetSliceFromPool(sliceRegionID, out var region)) return false;
		LoadSliceIntoMap(from, to, targetUnitPos, region, clearOriginal);
		return true;
	}
	public static void LoadSliceIntoMap (IBlockSquad from, IBlockSquad to, Int3 targetUnitPos, WorldSliceRegion region, bool clearOriginal = false) {
		int l = region.X;
		int r = region.X + region.W;
		int d = region.Y;
		int u = region.Y + region.H;
		int z = region.Z;
		for (int j = d; j < u; j++) {
			for (int i = l; i < r; i++) {
				var (level, bg, entity, element) = from.GetAllBlocksAt(i, j, z);
				if (clearOriginal || level != 0) {
					to.SetBlockAt(targetUnitPos.x, targetUnitPos.y, targetUnitPos.z, BlockType.Level, level);
				}
				if (clearOriginal || bg != 0) {
					to.SetBlockAt(targetUnitPos.x, targetUnitPos.y, targetUnitPos.z, BlockType.Background, bg);
				}
				if (clearOriginal || entity != 0) {
					to.SetBlockAt(targetUnitPos.x, targetUnitPos.y, targetUnitPos.z, BlockType.Entity, entity);
				}
				if (clearOriginal || element != 0) {
					to.SetBlockAt(targetUnitPos.x, targetUnitPos.y, targetUnitPos.z, BlockType.Element, element);
				}
			}
		}
	}


	// File
	public static bool LoadSliceFromFile () {
		string path = SliceFilePath;
		if (!Util.FileExists(path)) return false;
		RegionPool.Clear();
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
				RegionPool.Add(id, new(id, tag, x, y, z, w, h));
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
		foreach (var (_, slice) in RegionPool) {
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

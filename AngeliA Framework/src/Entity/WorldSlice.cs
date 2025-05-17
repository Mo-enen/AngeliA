using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.MapEditorGroup("System", -512)]
public class WorldSlice : IMapItem {

	// VAR
	public const int MAX_DISTANCE = Const.MAP * 2;
	public static readonly int TYPE_ID = typeof(WorldSlice).AngeHash();
	private static readonly HashSet<int> SliceIdSet = [];

	// MSG
	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		SliceIdSet.Clear();
		if (Renderer.TryGetSpriteGroup(TYPE_ID, out var group)) {
			for (int i = 0; i < group.Count; i++) {
				var sp = group.Sprites[i];
				if (!sp.Rule.IsEmpty) {
					SliceIdSet.Add(sp.ID);
				}
			}
		}
		SliceIdSet.TrimExcess();
	}

	// API
	public static bool IsSlice (int id) => id != 0 && SliceIdSet.Contains(id);

	public static bool TryGetSliceFromMap (IBlockSquad squad, int unitX, int unitY, int unitZ, out WorldSliceRegion slice) {
		slice = default;

		// Pivot Check
		if (
			IsSlice(squad, unitX - 1, unitY, unitZ) ||
			IsSlice(squad, unitX, unitY - 1, unitZ) ||
			!IsSlice(squad, unitX + 1, unitY, unitZ) ||
			!IsSlice(squad, unitX, unitY + 1, unitZ)
		) return false;

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

}

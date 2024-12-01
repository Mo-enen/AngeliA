using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class BlockColoringSystem {




	#region --- VAR ---


	// Const
	public static readonly Dictionary<int, Color32> COLOR_POOL = [];

	// Api
	public static bool Enable { get; private set; } = false;

	// Data
	private static int CellIndexStart = -1;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Enable = Util.TryGetAttributeFromAllAssemblies<UseBlockColoringSystemAttribute>();
		if (!Enable) return;
		// Init Color Pool
		COLOR_POOL.Clear();
		foreach (var type in typeof(BlockColor).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not BlockColor bColor) continue;
			COLOR_POOL.TryAdd(type.AngeHash(), bColor.Color);
		}
	}


	[BeforeLevelRendered]
	internal static void BeforeLevelRendered () {
		if (!Enable) return;
		CellIndexStart = Renderer.GetUsedCellCount(RenderLayer.DEFAULT);
	}


	[AfterLevelRendered]
	internal static void AfterLevelRendered () {
		if (!Enable) return;
		if (!Renderer.GetCells(RenderLayer.DEFAULT, out var cells, out int count)) return;
		var squad = WorldSquad.Front;
		int z = Stage.ViewZ;
		for (int i = CellIndexStart; i < count; i++) {
			var cell = cells[i];
			int unitX = (cell.X + cell.Width / 2).ToUnit();
			int unitY = (cell.Y + cell.Height / 2).ToUnit();
			int id = squad.GetBlockAt(unitX, unitY, z, BlockType.Element);
			if (id != 0 && COLOR_POOL.TryGetValue(id, out var color)) {
				float lerp = color.a / 255f;
				cell.Color.r = (byte)Util.LerpUnclamped(255, color.r, lerp);
				cell.Color.g = (byte)Util.LerpUnclamped(255, color.g, lerp);
				cell.Color.b = (byte)Util.LerpUnclamped(255, color.b, lerp);
			}
		}
	}


	#endregion




	#region --- API ---


	public static bool TryGetColor (int elementID, out Color32 color) => COLOR_POOL.TryGetValue(elementID, out color);


	#endregion




	#region --- LGC ---



	#endregion




}

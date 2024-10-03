using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


internal static class EnvironmentShadowRenderer {


	private static int CellStartIndex = int.MaxValue;
	private static int CellUpdateFrame = -1;


	[BeforeLevelRendered]
	public static void BeforeLevelRendered () => BeforeLayerFrameUpdate(EntityLayer.ENVIRONMENT);


	[AfterLevelRendered]
	public static void AfterLevelRendered () => AfterLayerFrameUpdate(EntityLayer.ENVIRONMENT);


	[BeforeLayerFrameUpdate]
	public static void BeforeLayerFrameUpdate (int layerIndex) {
		if (layerIndex != EntityLayer.ENVIRONMENT) return;
		CellStartIndex = Renderer.GetUsedCellCount(RenderLayer.DEFAULT);
		CellUpdateFrame = Game.GlobalFrame;
	}


	[AfterLayerFrameUpdate]
	public static void AfterLayerFrameUpdate (int layerIndex) {
		if (layerIndex != EntityLayer.ENVIRONMENT || CellUpdateFrame != Game.GlobalFrame) return;
		if (!WorldSquad.Enable) return;
		if (Renderer.GetCells(RenderLayer.DEFAULT, out var cells, out int count)) {
			for (int i = CellStartIndex; i < count; i++) {
				var cell = cells[i];
				if (cell.TextSprite != null) continue;
				FrameworkUtil.DrawEnvironmentShadow(cell, z: cell.Z - 1);
			}
		}
	}


}


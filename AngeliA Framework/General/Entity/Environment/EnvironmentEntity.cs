using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngeliaFramework {


	[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
	public abstract class EnvironmentRigidbody : Rigidbody { }


	[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
	public abstract class EnvironmentEntity : Entity {



		private static int CellStartIndex = int.MaxValue;
		private static int CellUpdateFrame = -1;


		[BeforeLevelRendered]
		public static void BeforeLevelRendered () => BeforeLayerFrameUpdate(EntityLayer.ENVIRONMENT);


		[AfterLevelRendered]
		public static void AfterLevelRendered () => AfterLayerFrameUpdate(EntityLayer.ENVIRONMENT);


		[BeforeLayerFrameUpdate]
		public static void BeforeLayerFrameUpdate (int layerIndex) {
			if (layerIndex != EntityLayer.ENVIRONMENT) return;
			CellStartIndex = CellRenderer.GetUsedCellCount(RenderLayer.DEFAULT);
			CellUpdateFrame = Game.GlobalFrame;
		}


		[AfterLayerFrameUpdate]
		public static void AfterLayerFrameUpdate (int layerIndex) {
			if (layerIndex != EntityLayer.ENVIRONMENT || CellUpdateFrame != Game.GlobalFrame) return;
			if (CellRenderer.GetCells(RenderLayer.DEFAULT, out var cells, out int count)) {
				CellRenderer.SetLayerToShadow();
				for (int i = CellStartIndex; i < count; i++) {
					var cell = cells[i];
					AngeUtil.DrawEnvironmentShadow(cell, z: cell.Z - 1);
				}
				CellRenderer.SetLayerToDefault();
			}
		}


	}
}

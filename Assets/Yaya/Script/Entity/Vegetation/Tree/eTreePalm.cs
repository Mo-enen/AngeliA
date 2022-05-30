using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityBounds(-Const.CELL_SIZE * 2, 0, Const.CELL_SIZE * 5, Const.CELL_SIZE)]
	public class eTreePalm : eTree {


		// Api
		protected override string TrunkCode => "Trunk Palm";
		protected override string LeafCode => "Leaf Palm";


		// MSG
		public override void FillPhysics () {
			base.FillPhysics();
			if (!HasTrunkOnTop) {
				CellPhysics.FillBlock(YayaConst.ENVIRONMENT, Rect.Shift(-Const.CELL_SIZE * 2, Const.CELL_SIZE / 2), true, Const.ONEWAY_UP_TAG);
				CellPhysics.FillBlock(YayaConst.ENVIRONMENT, Rect.Shift(-Const.CELL_SIZE, Const.CELL_SIZE / 2), true, Const.ONEWAY_UP_TAG);
				CellPhysics.FillBlock(YayaConst.ENVIRONMENT, Rect.Shift(0, Const.CELL_SIZE / 2), true, Const.ONEWAY_UP_TAG);
				CellPhysics.FillBlock(YayaConst.ENVIRONMENT, Rect.Shift(Const.CELL_SIZE, Const.CELL_SIZE / 2), true, Const.ONEWAY_UP_TAG);
				CellPhysics.FillBlock(YayaConst.ENVIRONMENT, Rect.Shift(Const.CELL_SIZE * 2, Const.CELL_SIZE / 2), true, Const.ONEWAY_UP_TAG);
			}
		}


		protected override Direction3 GetDirection () {
			base.GetDirection();
			return Direction3.Vertical;
		}


		protected override void DrawLeaf () {
			if (HasTrunkOnTop) return;
			CellRenderer.Draw(LeafArtworkCode, Rect.Shift(-Const.CELL_SIZE * 2, GetLeafShiftY(-24) + Const.CELL_SIZE / 2));
			CellRenderer.Draw(LeafArtworkCode, Rect.Shift(-Const.CELL_SIZE, GetLeafShiftY(-12) + Const.CELL_SIZE / 2));
			CellRenderer.Draw(LeafArtworkCode, Rect.Shift(0, GetLeafShiftY(0) + Const.CELL_SIZE / 2));
			CellRenderer.Draw(LeafArtworkCode, Rect.Shift(Const.CELL_SIZE, GetLeafShiftY(12) + Const.CELL_SIZE / 2));
			CellRenderer.Draw(LeafArtworkCode, Rect.Shift(Const.CELL_SIZE * 2, GetLeafShiftY(24) + Const.CELL_SIZE / 2));
		}


	}
}
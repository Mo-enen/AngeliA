using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityBounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE)]
	public class eTreeWillow : eTree {


		// Api
		protected override string LeafCode => "Leaf Willow";

		// Data
		public override void FillPhysics () {
			if (!HasTrunkOnTop) base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true);
			CellPhysics.FillBlock(
				YayaConst.ENVIRONMENT,
				HasTrunkOnTop ? Rect : Rect.Shrink(0, 0, 0, Height / 2),
				true, YayaConst.CLIMB_TAG
			);
		}


		protected override void DrawTrunk () {
			if (!HasTrunkOnTop) base.DrawTrunk();
		}


		protected override void DrawLeaf () {
			CellRenderer.Draw(LeafArtworkCode, Rect.Shift(GetLeafShiftY(0), 0));
		}


	}
}
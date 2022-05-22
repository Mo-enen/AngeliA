using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eTreeWillow : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 2".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 2".AngeHash();
		private static readonly int LEAF_CODE = "Leaf Willow".AngeHash();

		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;


		public override void FrameUpdate () {
			int frame = Game.GlobalFrame;
			base.FrameUpdate();
			// Leaf
			if (IsBigTree) {
				// Big
				CellRenderer.Draw(LEAF_CODE, new(
					X + AOffset(frame, TreesOnTop), Y + Const.CELL_SIZE / 2, Const.CELL_SIZE, Const.CELL_SIZE
				));
				CellRenderer.Draw(LEAF_CODE, new(
					X + AOffset(frame, TreesOnTop - 1) - Const.CELL_SIZE, Y + Const.CELL_SIZE / 2, Const.CELL_SIZE, Const.CELL_SIZE
				));
				CellRenderer.Draw(LEAF_CODE, new(
					X + AOffset(frame, TreesOnTop + 1) + Const.CELL_SIZE, Y + Const.CELL_SIZE / 2, Const.CELL_SIZE, Const.CELL_SIZE
				));
			} else {
				// Small
				CellRenderer.Draw(LEAF_CODE, new(
					X + AOffset(frame, TreesOnTop), Y + Const.CELL_SIZE / 2, Const.CELL_SIZE, Const.CELL_SIZE
				));
			}
		}


		private int AOffset (int frame, int offset) => -(Mathf.PingPong(frame + offset * 40f, 120f) / 6f).RoundToInt();


	}
}
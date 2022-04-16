using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eTreePalm : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 1".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 1".AngeHash();
		private static readonly int LEAF_CODE = "Leaf Palm".AngeHash();

		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			// Leaf
			if (TreesOnTop == 0) {
				CellRenderer.Draw(LEAF_CODE, new(
					X, Y + Const.CELL_SIZE / 2 + AOffset(frame, 0), Const.CELL_SIZE, Const.CELL_SIZE
				));
				CellRenderer.Draw(LEAF_CODE, new(
					X - Const.CELL_SIZE, Y + Const.CELL_SIZE / 2 + AOffset(frame, -1), Const.CELL_SIZE, Const.CELL_SIZE
				));
				CellRenderer.Draw(LEAF_CODE, new(
					X + Const.CELL_SIZE, Y + Const.CELL_SIZE / 2 + AOffset(frame, +1), Const.CELL_SIZE, Const.CELL_SIZE
				));
			}
		}


		private int AOffset (int frame, int offset) => -(Mathf.PingPong(frame + offset * 40f, 120f) / 6f).RoundToInt();


	}
}
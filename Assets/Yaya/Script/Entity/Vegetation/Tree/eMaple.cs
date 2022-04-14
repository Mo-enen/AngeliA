using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eMaple : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 0".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 0".AngeHash();
		private static readonly int LEAF_CODE = "Leaf Maple".AngeHash();

		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			// Leaf
			if (IsBigTree) {
				// Big
				CellRenderer.Draw(LEAF_CODE, new(
					X, Y + Const.CELL_SIZE / 2 - (Mathf.PingPong(frame + TreesOnTop * 40f, 120f) / 6f).RoundToInt(), Const.CELL_SIZE, Const.CELL_SIZE
				));
				if (TreesOnTop > 0) {
					// 3
					CellRenderer.Draw(LEAF_CODE, new(
						X - Const.CELL_SIZE, Y + Const.CELL_SIZE / 2 - (Mathf.PingPong(frame + TreesOnTop * 40f - 40f, 120f) / 6f).RoundToInt(), Const.CELL_SIZE, Const.CELL_SIZE
					));
					CellRenderer.Draw(LEAF_CODE, new(
						X + Const.CELL_SIZE, Y + Const.CELL_SIZE / 2 - (Mathf.PingPong(frame + TreesOnTop * 40f + 40f, 120f) / 6f).RoundToInt(), Const.CELL_SIZE, Const.CELL_SIZE
					));
				}
				if (HasTreesOnBottom) {
					// 2
					CellRenderer.Draw(LEAF_CODE, new(
						X - Const.CELL_SIZE * 2 / 3 + 6, Y - 36 - (Mathf.PingPong(frame + TreesOnTop * 40f - 40f, 120f) / 6f).RoundToInt(), Const.CELL_SIZE, Const.CELL_SIZE
					));
					CellRenderer.Draw(LEAF_CODE, new(
						X + Const.CELL_SIZE * 2 / 3 - 6, Y - 36 - (Mathf.PingPong(frame + TreesOnTop * 40f + 40f, 120f) / 6f).RoundToInt(), Const.CELL_SIZE, Const.CELL_SIZE
					));
				}
			} else {
				// Small
				CellRenderer.Draw(LEAF_CODE, new(
					X, Y + Const.CELL_SIZE / 2 - (Mathf.PingPong(frame, 120f) / 6f).RoundToInt(), Const.CELL_SIZE, Const.CELL_SIZE
				));
			}
		}


	}
}
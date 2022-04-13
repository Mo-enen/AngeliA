using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eWillow : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 2".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 2".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Willow 0".AngeHash(), "Leaf Willow 1".AngeHash(), "Leaf Willow 2".AngeHash(), };

		private const int SPAN = 2;
		private const int MAX_SPAN = SPAN * Const.CELL_SIZE;

		public override RectInt Bounds => Rect.Expand(MAX_SPAN, MAX_SPAN, 0, Const.CELL_SIZE / 2);
		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;
		protected override int LeafCountMin => 1;
		protected override int LeafCountMax => 1;


		protected override void DrawLeaf (int frame, int code, Vector2Int shift) {
			if (!HasTreesOnBottom && TreesOnTop > 0) return;
			int stepY = 3 - TreesOnTop;
			for (int lIndex = 0; lIndex < SPAN; lIndex++) {
				code = LEAF_CODES[(shift.x + lIndex).UMod(LEAF_CODES.Length)];
				int deltaX = (int)Util.Remap(
					0f, 120f, -12f, 12f,
					Mathf.PingPong(frame + shift.x + lIndex * 24 + stepY * 12, 120)
				);
				CellRenderer.Draw(
					code,
					X + Const.CELL_SIZE / 2 + lIndex * Const.CELL_SIZE + deltaX,
					Y,
					Const.CELL_SIZE, Const.CELL_SIZE
				);
				CellRenderer.Draw(
					code,
					X + Const.CELL_SIZE / 2 - lIndex * Const.CELL_SIZE + deltaX,
					Y,
					-Const.CELL_SIZE, Const.CELL_SIZE
				);
			}
		}


		protected override int GetLeafCode (int index) => LEAF_CODES[index.UMod(LEAF_CODES.Length)];


	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePalm : eTree {

		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 1".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 1".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Palm 0".AngeHash(), "Leaf Palm 1".AngeHash(), "Leaf Palm 2".AngeHash(), };

		private int SPAN => LeafSize * 2;
		private const int LEAF_LENGTH = 2;

		public override RectInt Bounds => Rect.Expand(LEAF_LENGTH * SPAN, LEAF_LENGTH * SPAN, 0, Const.CELL_SIZE / 2);
		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;
		protected override int LeafCountMin => 1;
		protected override int LeafCountMax => 1;


		protected override void DrawLeaf (int frame, int code, Vector2Int shift) {
			if (TreesOnTop != 0 && (HasTreesOnBottom || TreesOnTop > 0)) return;
			for (int lIndex = 0; lIndex < LEAF_CODES.Length; lIndex++) {
				code = LEAF_CODES[lIndex];
				int x = X + Const.CELL_SIZE / 2;
				int y = Y + Const.CELL_SIZE - lIndex * LeafSize / 3;
				int offsetY = lIndex % 2 == 0 ? LeafSize / 11 : -LeafSize / 11;
				for (int i = 0; i < LEAF_LENGTH; i++) {
					int rot = (int)Util.Remap(
						0f, 120f, 0f, 12f,
						Mathf.PingPong(frame + shift.x + lIndex * 96 - i * 12, 120)
					);
					CellRenderer.Draw(
						code, x, y + offsetY,
						0 - 998 * i, 500, rot,
						SPAN, SPAN
					);
					rot = (int)Util.Remap(
						0f, 120f, 0f, 12f,
						Mathf.PingPong(frame + shift.x + lIndex * 32 - i * 6 + 49, 120)
					);
					CellRenderer.Draw(
						code, x, y - offsetY,
						0 - 998 * i, 500, -rot,
						-SPAN, SPAN
					);
				}
			}
		}


		protected override int GetLeafCode (int index) => LEAF_CODES[index.UMod(LEAF_CODES.Length)];


	}
}
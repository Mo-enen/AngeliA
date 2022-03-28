using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eWillow : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 2".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 2".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Willow 0".AngeHash(), "Leaf Willow 1".AngeHash(), "Leaf Willow 2".AngeHash(), };

		private const int SPAN = 6;
		private int MaxSpan => SPAN * LeafSize * 2 / 3;

		public override RectInt Bounds => Rect.Expand(MaxSpan, MaxSpan, 0, LeafSize / 2);
		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;
		protected override int LeafCountMin => 1;
		protected override int LeafCountMax => 1;


		protected override void DrawLeaf (int frame, int code, int step, Vector2Int shift) {
			if (step != Tall - 1) return;
			int basicY = Y + step * Const.CELL_SIZE + Const.CELL_SIZE - LeafSize / 2;
			int countY = (Tall - 1) * Const.CELL_SIZE / LeafSize;
			for (int lIndex = 0; lIndex < SPAN; lIndex++) {
				code = LEAF_CODES[(shift.x + lIndex).UMod(LEAF_CODES.Length)];
				for (int i = 0; i < countY; i++) {
					int offsetX = lIndex * LeafSize * 2 / 3;
					const int SHRINK_COUNT = 4;
					if (i < SHRINK_COUNT) {
						offsetX -= (int)Util.Remap(
							0, SHRINK_COUNT,
							SPAN * LeafSize / 3, 0,
							i
						);
					}
					int deltaX = (int)Util.Remap(
						0f, 120f, -12f, 12f,
						Mathf.PingPong(frame + shift.x + lIndex * 24 + i * 12, 120)
					);
					CellRenderer.Draw(
						code,
						X + Const.CELL_SIZE / 2 + offsetX + deltaX,
						basicY - i * LeafSize,
						LeafSize, LeafSize
					);
					CellRenderer.Draw(
						code,
						X + Const.CELL_SIZE / 2 - offsetX + deltaX,
						basicY - i * LeafSize,
						-LeafSize, LeafSize
					);
				}
			}
		}


		protected override int GetLeafCode (int index) => LEAF_CODES[index.UMod(LEAF_CODES.Length)];


	}
}
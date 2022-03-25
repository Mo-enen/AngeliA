using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eMaple : eTree {

		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 0".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 0".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Maple 0".AngeHash(), "Leaf Maple 1".AngeHash(), "Leaf Maple 2".AngeHash(), };

		private const int SPAN = Const.CELL_SIZE * 3;

		public override RectInt Bounds => Rect.Expand(SPAN / 2, SPAN / 2, 0, LeafSize / 2);
		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;
		protected override int LeafCountMin => 6;
		protected override int LeafCountMax => 12;


		protected override void DrawLeaf (int frame, int code, int step, Vector2Int shift) {
			int xMin = step < Tall - 1 ? Const.CELL_SIZE / 2 - SPAN / 2 : 0;
			int xMax = step < Tall - 1 ? Const.CELL_SIZE / 2 + SPAN / 2 : Const.CELL_SIZE;
			CellRenderer.Draw(
				code,
				X + LeafSize / 2 + (int)Util.Remap(0, Const.CELL_SIZE, xMin, xMax - LeafSize, shift.x),
				Y + step * Const.CELL_SIZE + shift.y + LeafSize / 2,
				500, 500,
				(int)Util.Remap(0f, 120f, -6f, 6f, Mathf.PingPong(frame + shift.x, 120)),
				LeafSize * 5 / 2, LeafSize * 5 / 2
			);
		}


		protected override int GetLeafCode (int index) => LEAF_CODES[index.UMod(LEAF_CODES.Length)];


	}

	public class ePalm : eTree {

		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 1".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 1".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Palm 0".AngeHash(), "Leaf Palm 1".AngeHash(), "Leaf Palm 2".AngeHash(), };

		private int SPAN => LeafSize * 2;
		private const int LEAF_LENGTH = 2;

		public override RectInt Bounds => Rect.Expand(
			LEAF_LENGTH * SPAN,
			LEAF_LENGTH * SPAN,
			0, LeafSize / 2
		);
		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;
		protected override int LeafCountMin => 1;
		protected override int LeafCountMax => 1;


		protected override void DrawLeaf (int frame, int code, int step, Vector2Int shift) {
			if (step != Tall - 1) return;
			for (int lIndex = 0; lIndex < LEAF_CODES.Length; lIndex++) {
				code = LEAF_CODES[lIndex];
				int x = X + Const.CELL_SIZE / 2;
				int y = Y + step * Const.CELL_SIZE + Const.CELL_SIZE - lIndex * LeafSize / 3;
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

	public class ePine : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 0".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 0".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Pine 0".AngeHash(), "Leaf Pine 1".AngeHash(), "Leaf Pine 2".AngeHash(), };

		private const int MAX_SPAN = Const.CELL_SIZE * 4;

		public override RectInt Bounds => Rect.Expand(MAX_SPAN / 2, MAX_SPAN / 2, 0, LeafSize / 2);
		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;
		protected override int LeafCountMin => 8;
		protected override int LeafCountMax => 14;


		protected override void DrawLeaf (int frame, int code, int step, Vector2Int shift) {
			int SPAN = (int)Util.Remap(0, Tall - 1, Const.CELL_SIZE * 4, Const.CELL_SIZE, step);
			int xMin = step < Tall - 1 ? Const.CELL_SIZE / 2 - SPAN / 2 : 0;
			int xMax = step < Tall - 1 ? Const.CELL_SIZE / 2 + SPAN / 2 : Const.CELL_SIZE;
			CellRenderer.Draw(
				code,
				X + LeafSize / 2 + (int)Util.Remap(0, Const.CELL_SIZE, xMin, xMax - LeafSize, shift.x),
				Y + step * Const.CELL_SIZE + shift.y + LeafSize / 2,
				500, 500,
				(int)Util.Remap(0f, 96f, -4f, 4f, Mathf.PingPong(frame + shift.x, 96f)),
				LeafSize * 3 / 2, LeafSize * 3 / 2
			);
		}


		protected override int GetLeafCode (int index) => LEAF_CODES[index.UMod(LEAF_CODES.Length)];


	}

	public class ePoplar : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 0".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 0".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Poplar 0".AngeHash(), "Leaf Poplar 1".AngeHash(), "Leaf Poplar 2".AngeHash(), };

		private const int SPAN = Const.CELL_SIZE * 3;

		public override RectInt Bounds => Rect.Expand(SPAN / 2, SPAN / 2, 0, LeafSize / 2);
		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;


		protected override void DrawLeaf (int frame, int code, int step, Vector2Int shift) {
			int xMin = step < Tall - 1 ? Const.CELL_SIZE / 2 - SPAN / 2 : 0;
			int xMax = step < Tall - 1 ? Const.CELL_SIZE / 2 + SPAN / 2 : Const.CELL_SIZE;
			CellRenderer.Draw(
				code,
				X + LeafSize / 2 + (int)Util.Remap(0, Const.CELL_SIZE, xMin, xMax - LeafSize, shift.x),
				Y + step * Const.CELL_SIZE + shift.y + LeafSize / 2,
				500, 500,
				(int)Util.Remap(0f, 120f, -6f, 6f, Mathf.PingPong(frame + shift.x, 120)),
				LeafSize * 5 / 2, LeafSize * 5 / 2
			);
		}


		protected override int GetLeafCode (int index) => LEAF_CODES[index.UMod(LEAF_CODES.Length)];


	}

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

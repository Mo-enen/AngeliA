using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
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
}
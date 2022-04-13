using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePoplar : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 0".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 0".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Poplar 0".AngeHash(), "Leaf Poplar 1".AngeHash(), "Leaf Poplar 2".AngeHash(), };

		private const int SPAN = Const.CELL_SIZE * 3;

		public override RectInt Bounds => Rect.Expand(SPAN / 2, SPAN / 2, 0, Const.CELL_SIZE / 2);
		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;

		protected override void DrawLeaf (int frame, int code, Vector2Int shift) {
			if (!HasTreesOnBottom && TreesOnTop > 0) return;
			int xMin = TreesOnTop != 0 ? Const.CELL_SIZE / 2 - SPAN / 2 : 0;
			int xMax = TreesOnTop != 0 ? Const.CELL_SIZE / 2 + SPAN / 2 : Const.CELL_SIZE;
			CellRenderer.Draw(
				code,
				X + LeafSize / 2 + (int)Util.Remap(0, Const.CELL_SIZE, xMin, xMax - LeafSize, shift.x),
				Y + shift.y + LeafSize / 2,
				500, 500,
				(int)Util.Remap(0f, 120f, -6f, 6f, Mathf.PingPong(frame + shift.x, 120)),
				LeafSize * 5 / 2, LeafSize * 5 / 2
			);
		}


		protected override int GetLeafCode (int index) => LEAF_CODES[index.UMod(LEAF_CODES.Length)];


	}
}
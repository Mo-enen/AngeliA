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


		protected override void DrawLeaf (int frame, int code, Vector2Int shift) {
			if (!HasTreesOnBottom && TreesOnTop > 0) return;
			int SPAN = (int)Util.RemapUnclamped(
				0, 7,
				Const.CELL_SIZE, Const.CELL_SIZE * 4,
				Mathf.Clamp(TreesOnTop, 0, 7)
			);
			int xMin = TreesOnTop > 0 ? Const.CELL_SIZE / 2 - SPAN / 2 : 0;
			int xMax = TreesOnTop > 0 ? Const.CELL_SIZE / 2 + SPAN / 2 : Const.CELL_SIZE;
			CellRenderer.Draw(
				code,
				X + LeafSize / 2 + (int)Util.Remap(0, Const.CELL_SIZE, xMin, xMax - LeafSize, shift.x),
				Y + shift.y + LeafSize / 2,
				500, 500,
				(int)Util.Remap(0f, 96f, -4f, 4f, Mathf.PingPong(frame + shift.x, 96f)),
				LeafSize * 3 / 2, LeafSize * 3 / 2
			);
		}


		protected override int GetLeafCode (int index) => LEAF_CODES[index.UMod(LEAF_CODES.Length)];


	}
}
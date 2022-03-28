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
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eLadder : eClimbable {

		private static readonly int LADDER_CODE = "Ladder".AngeHash();

		public override RectInt Rect => new(X + Const.CELL_SIZE / 4, Y, Const.CELL_SIZE / 2, Const.CELL_SIZE);
		public override RectInt Bounds => new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE);
		public override int Capacity => 128;

		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(LADDER_CODE, new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE));
		}

	}
}

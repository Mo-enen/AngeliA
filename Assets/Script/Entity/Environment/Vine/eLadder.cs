using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eLadder : eVine {

		private static readonly int VINE_CODE = "Ladder".AngeHash();

		public override RectInt Rect => new(X + Const.CELL_SIZE / 4, Y, Const.CELL_SIZE / 2, Const.CELL_SIZE);

		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(VINE_CODE, new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE));
		}

	}
}

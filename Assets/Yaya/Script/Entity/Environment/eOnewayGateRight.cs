using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eOnewayGateRight : eOneway {



		private static readonly int ONEWAY_CODE = "Oneway Gate".ACode();

		public override Direction4 GateDirection => Direction4.Right;


		public override void FrameUpdate (int frame) {
			var rect = Rect;
			CellRenderer.Draw(
				ONEWAY_CODE,
				rect.x + rect.width / 2,
				rect.y + rect.height / 2,
				500, 500,
				GateDirection == Direction4.Up ? 0 : GateDirection == Direction4.Down ? 180 : GateDirection == Direction4.Left ? 270 : 90,
				rect.width,
				rect.height
			);
			base.FrameUpdate(frame);
		}


	}
}

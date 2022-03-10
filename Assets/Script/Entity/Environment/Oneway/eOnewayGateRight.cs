using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eOnewayGateRight : eOneway {



		private static readonly int ONEWAY_CODE = "Oneway Gate".AngeHash();

		public override Direction4 GateDirection => Direction4.Right;


		public override void FillPhysics (int frame) {
			CellPhysics.FillEntity(PhysicsLayer.Environment, this, true, Const.ONEWAY_RIGHT_TAG);
		}


		public override void FrameUpdate (int frame) {
			var rect = Rect;
			int rotDelta = 0;
			if (frame < ReboundFrame + 4) {
				rect.x -= (ReboundFrame - frame + 4) * 8;
				rotDelta = (ReboundFrame - frame + 4) * 2 * (frame % 2 == 0 ? -1 : 1);
			}
			CellRenderer.Draw(
				ONEWAY_CODE,
				rect.x + rect.width / 2,
				rect.y + rect.height / 2,
				500, 500, 90 + rotDelta,
				-rect.width,
				rect.height
			);
			base.FrameUpdate(frame);
		}


	}
}

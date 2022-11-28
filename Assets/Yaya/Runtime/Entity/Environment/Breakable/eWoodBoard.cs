using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eWoodBoard : eBreakableRigidbody {

		public override int PhysicsLayer => YayaConst.LAYER_ENVIRONMENT;
		protected override bool DestroyWhenInsideGround => true;
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, base.Rect);
		}

	}
}

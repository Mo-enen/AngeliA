using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaGame {
	public class eWoodBoard : BreakableRigidbody, ICombustible {
		public int BurnedDuration => 30;
		public int BurnStartFrame { get; set; }
		protected override int PhysicsLayer => Const.LAYER_ENVIRONMENT;
		protected override bool DestroyWhenInsideGround => true;
		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, base.Rect);
		}

	}
}

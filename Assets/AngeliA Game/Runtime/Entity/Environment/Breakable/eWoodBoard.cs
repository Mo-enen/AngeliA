using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaGame {
	public class eWoodBoard : BreakableRigidbody, ICombustible {
		public int BurnedDuration => 30;
		int ICombustible.BurnStartFrame { get; set; }
		protected override int PhysicsLayer => Const.LAYER_ENVIRONMENT;
		protected override bool DestroyWhenInsideGround => true;
	}
}

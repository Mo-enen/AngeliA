using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaFramework {
	public class WoodBoard : BreakableRigidbody, ICombustible {
		public int BurnedDuration => 30;
		int ICombustible.BurnStartFrame { get; set; }
		protected override int PhysicalLayer => PhysicsLayer.ENVIRONMENT;
		protected override bool DestroyWhenInsideGround => true;
	}
}

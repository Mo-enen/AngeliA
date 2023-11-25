using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class Box : BreakableRigidbody, ICombustible {
		public int BurnedDuration => 320;
		int ICombustible.BurnStartFrame { get; set; }
	}
}

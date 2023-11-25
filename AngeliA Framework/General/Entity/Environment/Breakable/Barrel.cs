using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class Barrel : BreakableRigidbody, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	[EntityAttribute.Capacity(32)]
	public class eBarrel : BreakableRigidbody, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}

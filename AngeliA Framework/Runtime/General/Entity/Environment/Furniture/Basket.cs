using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaFramework {
	public class Basket : Furniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}

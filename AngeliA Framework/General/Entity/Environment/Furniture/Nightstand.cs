using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class Nightstand : Furniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}

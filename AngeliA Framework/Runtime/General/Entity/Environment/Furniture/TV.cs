using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaFramework {
	public class TV : Furniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eBedWoodA : Bed, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eBedWoodB : Bed, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}

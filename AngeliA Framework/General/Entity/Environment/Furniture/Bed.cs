using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class BedWoodA : Bed, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class BedWoodB : Bed, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}

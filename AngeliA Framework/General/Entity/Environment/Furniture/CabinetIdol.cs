using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class CabinetIdolWood : CabinetIdol, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}

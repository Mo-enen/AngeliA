using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaGeneral {
	public class eTV : Furniture, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
}

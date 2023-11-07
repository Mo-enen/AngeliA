using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class Fridge : OpenableFurniture {
		protected override Direction3 ModuleType => Direction3.Vertical;
	}
}

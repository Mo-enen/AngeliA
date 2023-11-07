using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class WoodStoneDoorFront : Door {
		protected override bool IsFrontDoor => true;
	}
	public class WoodStoneDoorBack : Door {
		protected override bool IsFrontDoor => false;
	}
	public class WoodDoorFront : Door {
		protected override bool IsFrontDoor => true;
	}
	public class WoodDoorBack : Door {
		protected override bool IsFrontDoor => false;
	}
}
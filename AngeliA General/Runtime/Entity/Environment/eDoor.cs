using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGeneral {
	public class eWoodStoneDoorFront : Door {
		protected override bool IsFrontDoor => true;
	}
	public class eWoodStoneDoorBack : Door {
		protected override bool IsFrontDoor => false;
	}
	public class eWoodDoorFront : Door {
		protected override bool IsFrontDoor => true;
	}
	public class eWoodDoorBack : Door {
		protected override bool IsFrontDoor => false;
	}
}
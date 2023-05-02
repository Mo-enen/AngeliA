using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaGame {


	public class eWoodStoneDoorFront : Door {
		protected override bool IsFrontDoor => true;
		private static readonly int OPEN_CODE = "WoodStoneDoorFront Open".AngeHash();
		protected override int ArtworkCode_Open => OPEN_CODE;
	}


	public class eWoodStoneDoorBack : Door {
		protected override bool IsFrontDoor => false;
		private static readonly int OPEN_CODE = "WoodStoneDoorBack Open".AngeHash();
		protected override int ArtworkCode_Open => OPEN_CODE;
	}



	public class eWoodDoorFront : Door {
		protected override bool IsFrontDoor => true;
		private static readonly int OPEN_CODE = "WoodDoorFront Open".AngeHash();
		protected override int ArtworkCode_Open => OPEN_CODE;
	}


	public class eWoodDoorBack : Door {
		protected override bool IsFrontDoor => false;
		private static readonly int OPEN_CODE = "WoodDoorBack Open".AngeHash();
		protected override int ArtworkCode_Open => OPEN_CODE;
	}


}
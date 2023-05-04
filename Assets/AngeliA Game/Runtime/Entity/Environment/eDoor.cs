using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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




	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public class ePortalFront : ePortal {
		private static readonly int CIRCLE_CODE = "PortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "PortalFlame".AngeHash();
		protected override int CircleCode => CIRCLE_CODE;
		protected override int FlameCode => FLAME_CODE;
		protected override bool IsFrontDoor => true;
	}


	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public class ePortalBack : ePortal {
		private static readonly int CIRCLE_CODE = "PortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "PortalFlame".AngeHash();
		protected override int CircleCode => CIRCLE_CODE;
		protected override int FlameCode => FLAME_CODE;
		protected override bool IsFrontDoor => false;
	}


	[EntityAttribute.Capacity(1)]
	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.DontSpawnFromWorld]
	public class eCheckPointPortal : ePortal {

		private static readonly int CIRCLE_CODE = "CheckPointPortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "CheckPointPortalFlame".AngeHash();
		protected override int CircleCode => CIRCLE_CODE;
		protected override int FlameCode => FLAME_CODE;
		protected override Vector3Int TargetGlobalPosition => CheckPoint.LastInvokedCheckPointUnitPosition.ToGlobal() + new Vector3Int(Const.HALF, 0, 0);

		[AfterGameInitialize]
		public static void Initialize () {
			CheckPoint.BackPortalEntityID = typeof(eCheckPointPortal).AngeHash();
		}

		public override bool Invoke (Player player) {
			bool result = base.Invoke(player);
			if (result) Active = false;
			return result;
		}

	}


}
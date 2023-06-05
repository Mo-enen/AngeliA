using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {



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

		public static readonly int TYPE_ID = typeof(eCheckPointPortal).AngeHash();
		private static readonly int CIRCLE_CODE = "CheckPointPortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "CheckPointPortalFlame".AngeHash();
		protected override int CircleCode => CIRCLE_CODE;
		protected override int FlameCode => FLAME_CODE;
		protected override Vector3Int TargetGlobalPosition => CheckPoint.LastInvokedCheckPointUnitPosition.ToGlobal() + new Vector3Int(Const.HALF, 0, 0);
		protected override bool IsFrontDoor => TargetGlobalPosition.z <= Stage.ViewZ;
		private int InvokeFrame = -1;


		[AfterGameInitialize]
		public static void Initialize () {
			CheckAltar.BackPortalEntityID = typeof(eCheckPointPortal).AngeHash();
		}


		public override void OnActivated () {
			base.OnActivated();
			InvokeFrame = -1;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (InvokeFrame >= 0 && Game.GlobalFrame > InvokeFrame + 30) {
				Active = false;
				InvokeFrame = -1;
			}
		}


		public override bool Invoke (Player player) {
			bool result = base.Invoke(player);
			if (result) {
				InvokeFrame = Game.GlobalFrame;
				CheckPoint.ClearLastInvoke();
			}
			return result;
		}

	}


}
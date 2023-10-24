using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public class ePortalFront : Portal {
		private static readonly int CIRCLE_CODE = "PortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "PortalFlame".AngeHash();
		protected override int CircleCode => CIRCLE_CODE;
		protected override int FlameCode => FLAME_CODE;
		protected override bool IsFrontDoor => true;
	}


	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public class ePortalBack : Portal {
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
	public class eCheckPointPortal : Portal {

		public static readonly int TYPE_ID = typeof(eCheckPointPortal).AngeHash();
		private static readonly int CIRCLE_CODE = "CheckPointPortalCircle".AngeHash();
		private static readonly int FLAME_CODE = "CheckPointPortalFlame".AngeHash();
		protected override int CircleCode => CIRCLE_CODE;
		protected override int FlameCode => FLAME_CODE;
		protected override Vector3Int TargetGlobalPosition => (CheckPoint.TurnBackUnitPosition ?? default).ToGlobal() + new Vector3Int(Const.HALF, 0, 0);
		protected override bool IsFrontDoor => TargetGlobalPosition.z <= Stage.ViewZ;
		private int InvokeFrame = -1;


		[OnGameInitialize(64)]
		public static void Initialize () => CheckPoint.BackPortalEntityID = typeof(eCheckPointPortal).AngeHash();


		public override void OnActivated () {
			base.OnActivated();
			InvokeFrame = -1;
			if (!CheckPoint.TurnBackUnitPosition.HasValue) Active = false;
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
			}
			return result;
		}


	}
}

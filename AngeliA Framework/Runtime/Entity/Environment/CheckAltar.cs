using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.Capacity(1)]
	public abstract class CheckAltar : CheckPoint, IGlobalPosition, IActionTarget {




		#region --- VAR ---


		// Api
		public static int BackPortalEntityID { get; set; } = 0;
		protected sealed override bool OnlySpawnWhenUnlocked => false;

		// Data
		private int LinkedCheckPointID { get; init; } = 0;


		#endregion




		#region --- MSG ---


		public CheckAltar () => LinkedCheckPointID = GetLinkedCheckPointID();


		[OnGameInitialize(-64)]
		public static void BeforeGameInitializeLater () {
			foreach (var type in typeof(CheckAltar).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not CheckAltar altar) continue;
				Link(type.AngeHash(), altar.LinkedCheckPointID);
			}
		}


		public override void OnActivated () {
			base.OnActivated();
			Height = Const.CEL * 2;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (
				LastInvokedCheckPointID == LinkedCheckPointID &&
				Stage.GetSpawnedEntityCount(BackPortalEntityID) == 0
			) {
				Stage.SpawnEntity(BackPortalEntityID, X, Y + Const.CEL * 3);
			}
		}


		protected override void OnPlayerTouched (Vector3Int unitPos) {
			base.OnPlayerTouched(unitPos);
			Unlock(LinkedCheckPointID);
		}


		#endregion




		#region --- API ---


		bool IActionTarget.AllowInvoke () => false;


		protected abstract int GetLinkedCheckPointID ();


		#endregion




	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.Bounds(-Const.CEL / 2, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.ForceUpdate]
	public abstract class ePlayer : eCharacter, ICameraTarget {


		// Api
		public bool AvailableForCamera => !InAir || Movement.IsFlying || Y < LastStableY;

		// Data
		private static readonly HitInfo[] Collects = new HitInfo[8];
		private int LastStableY = 0;


		// MSG
		public override void OnActived () {
			base.OnActived();
			LastStableY = Y;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			// Cache
			if (!InAir || Movement.IsFlying) LastStableY = Y;
			// Collect
			int count = CellPhysics.OverlapAll(
				Collects, YayaConst.MASK_ENTITY, Rect, this, OperationMode.TriggerOnly
			);
			for (int i = 0; i < count; i++) {
				var hit = Collects[i];
				if (hit.Entity is not eCollectable col) continue;
				bool success = col.OnCollect(this);
				if (success) {
					hit.Entity.Active = false;
				}
			}
		}


	}
}
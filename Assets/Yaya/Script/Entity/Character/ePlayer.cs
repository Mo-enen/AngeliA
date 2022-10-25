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
	public abstract class ePlayer : eCharacter {


		// Data
		private static readonly HitInfo[] Collects = new HitInfo[8];


		// MSG
		public override void FillPhysics () {
			if (FrameStep.HasStep(YayaConst.STEP_ROUTE)) return;
			base.FillPhysics();
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
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
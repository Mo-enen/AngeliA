using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {



	public abstract class eHiddenBlock : Entity {


		// Api
		public bool IsHidden { get; private set; } = true;
		protected virtual bool TriggerFromBottom => true;
		protected virtual bool TriggerFromTop => false;
		protected virtual bool TriggerFromLeft => false;
		protected virtual bool TriggerFromRight => false;

		// Data
		private static readonly PhysicsCell[] c_Checks = new PhysicsCell[8];


		// MSG
		public override void OnActivated () {
			base.OnActivated();
			IsHidden = true;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			if (!IsHidden) {
				CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, false);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Check for Trigger
			if (IsHidden) {
				int count = CellPhysics.OverlapAll(c_Checks, Const.MASK_RIGIDBODY, Rect, this);
				for (int i = 0; i < count; i++) {
					var hit = c_Checks[i];
					if (hit.Entity is not Player player) continue;
					if (
						(TriggerFromBottom && player.VelocityY > 0) ||
						(TriggerFromTop && player.VelocityY < 0) ||
						(TriggerFromLeft && player.VelocityX > 0) ||
						(TriggerFromRight && player.VelocityX < 0)
					) {
						IsHidden = false;
						OnExposed();
						break;
					}
				}
			} else {
				CellRenderer.Draw(TypeID, Rect);
			}
		}


		protected virtual void OnExposed () { }


	}
}
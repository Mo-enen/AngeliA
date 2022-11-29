using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class TestBlock : eHiddenBlock {


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (!IsHidden) CellRenderer.Draw(Const.PIXEL, Rect);
		}


	}
	public abstract class eHiddenBlock : Entity {


		// Api
		public bool IsHidden { get; private set; } = true;
		protected virtual bool TriggerFromBottom => true;
		protected virtual bool TriggerFromTop => false;
		protected virtual bool TriggerFromLeft => false;
		protected virtual bool TriggerFromRight => false;
		protected virtual CharacterIdentity TriggerPermission => CharacterIdentity.Player;

		// Data
		private static readonly HitInfo[] c_Checks = new HitInfo[8];


		// MSG
		public override void OnActived () {
			base.OnActived();
			IsHidden = true;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			if (!IsHidden) {
				CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, false);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Check for Trigger
			if (IsHidden) {
				int count = CellPhysics.OverlapAll(c_Checks, YayaConst.MASK_RIGIDBODY, Rect, this);
				var blockRect = Rect;
				for (int i = 0; i < count; i++) {
					var hit = c_Checks[i];
					if (hit.Entity is not eCharacter ch) continue;
					if (((int)TriggerPermission & (int)ch.Identity) == 0) continue;
					var hitPrevRect = ch.PrevRect;
					if (
						(TriggerFromBottom && hitPrevRect.yMax <= blockRect.yMin) ||
						(TriggerFromTop && hitPrevRect.yMin >= blockRect.yMax) ||
						(TriggerFromLeft && hitPrevRect.xMax <= blockRect.xMin) ||
						(TriggerFromRight && hitPrevRect.xMin >= blockRect.xMax)
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
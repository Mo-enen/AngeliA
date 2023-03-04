using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	public abstract partial class eCharacter {


		// Const
		const int ACTION_SCAN_RANGE = Const.HALF;
		const int ACTION_SCAN_FREQUENCY = 6;

		// Api
		public ActionEntity CurrentActionTarget { get; private set; } = null;
		public bool LockingInput => CurrentActionTarget != null && CurrentActionTarget.LockInput;

		// Data
		private readonly PhysicsCell[] c_ScanHits = new PhysicsCell[16];
		private bool RequireRefresh = false;



		// MSG
		public void OnActived_Action () {
			CurrentActionTarget = null;
			RequireRefresh = true;
		}


		public void PhysicsUpdate_Action () {
			// Search for Active Trigger
			if (Game.GlobalFrame % ACTION_SCAN_FREQUENCY != 0 && !RequireRefresh) return;
			RequireRefresh = false;
			CurrentActionTarget = null;
			int count = CellPhysics.OverlapAll(
				c_ScanHits,
				YayaConst.MASK_ENTITY,
				Rect.Expand(ACTION_SCAN_RANGE, ACTION_SCAN_RANGE, 0, ACTION_SCAN_RANGE),
				this,
				OperationMode.ColliderAndTrigger
			);
			int dis = int.MaxValue;
			var sourceRect = Rect;
			int sourceX = sourceRect.x + sourceRect.width / 2;
			Entity result = null;
			for (int i = 0; i < count; i++) {
				var hit = c_ScanHits[i];
				if (hit.Entity is not ActionEntity act) continue;
				if (!act.AllowInvoke(this)) continue;
				// Comparer X Distance
				int _dis =
					sourceX >= hit.Rect.xMin && sourceX <= hit.Rect.xMax ? 0 :
					sourceX > hit.Rect.xMax ? Mathf.Abs(sourceX - hit.Rect.xMax) :
					Mathf.Abs(sourceX - hit.Rect.xMin);
				if (_dis < dis) {
					dis = _dis;
					result = hit.Entity;
				} else if (_dis == dis && result != null) {
					// Comparer Y Distance
					if (hit.Entity.Y < result.Y) {
						dis = _dis;
						result = hit.Entity;
					} else if (hit.Entity.Rect.y == result.Rect.y) {
						// Comparer Size
						if (hit.Entity.Width * hit.Entity.Height < result.Width * result.Height) {
							dis = _dis;
							result = hit.Entity;
						}
					}
				}
			}
			CurrentActionTarget = result as ActionEntity;
		}


		// API
		public bool InvokeAction () {
			if (CurrentActionTarget == null) return false;
			return CurrentActionTarget.Invoke(this);
		}


		public void CancelInvokeAction () => CurrentActionTarget?.CancelInvoke(this);



	}
}
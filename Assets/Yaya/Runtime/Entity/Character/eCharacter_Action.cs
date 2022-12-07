using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	public interface IActionEntity {

		public int HighlightFrame { get; set; }
		public bool Invoke (Entity target);
		public void CancelInvoke (Entity target);
		public bool AllowInvoke (Entity target) => true;

		public bool LockInput => false;
		public bool IsHighlighted => Game.GlobalFrame <= HighlightFrame + 1;
		public void Highlight () => HighlightFrame = Game.GlobalFrame;
		public static void HighlightBlink (Cell cell, IActionEntity iAct) {
			if (!iAct.IsHighlighted || Game.GlobalFrame % 30 > 15) return;
			const int OFFSET = Const.CEL / 20;
			cell.Width += OFFSET * 2;
			cell.Height += OFFSET * 2;
		}
	}


	public abstract partial class eCharacter {


		// Api
		public IActionEntity CurrentActionTarget { get; private set; } = null;
		public bool LockingInput => CurrentActionTarget != null && CurrentActionTarget.LockInput;

		// Ser
		[SerializeField] int ActionScanRange = Const.CEL / 2;
		[SerializeField] int ActionScanFrequency = 6;

		// Data
		private readonly HitInfo[] c_ScanHits = new HitInfo[16];
		private bool RequireRefresh = false;



		// MSG
		public void OnActived_Action () {
			CurrentActionTarget = null;
			ActionScanFrequency = ActionScanFrequency.Clamp(1, int.MaxValue);
			RequireRefresh = true;
		}


		public void Update_Action () {
			// Search for Active Trigger
			if (Game.GlobalFrame % ActionScanFrequency == 0 || RequireRefresh) {
				RequireRefresh = false;
				CurrentActionTarget = null;
				int count = CellPhysics.OverlapAll(
					c_ScanHits,
					YayaConst.MASK_ENTITY,
					Rect.Expand(ActionScanRange, ActionScanRange, 0, ActionScanRange),
					this,
					OperationMode.ColliderAndTrigger
				);
				int dis = int.MaxValue;
				var sourceRect = Rect;
				int sourceX = sourceRect.x + sourceRect.width / 2;
				Entity result = null;
				for (int i = 0; i < count; i++) {
					var hit = c_ScanHits[i];
					if (hit.Entity is not IActionEntity act) continue;
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
				CurrentActionTarget = result as IActionEntity;
			}
			// Highlight
			if (CurrentActionTarget != null && this is ePlayer) CurrentActionTarget.Highlight();
		}


		// API
		public bool InvokeAction () {
			if (CurrentActionTarget == null) return false;
			if (this is ePlayer && !FrameInput.GameKeyDown(GameKey.Action)) return false;
			return CurrentActionTarget.Invoke(this);
		}


		public void CancelInvokeAction () => CurrentActionTarget?.CancelInvoke(this);



	}
}
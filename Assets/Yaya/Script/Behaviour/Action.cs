using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	[System.Serializable]
	public class Action {




		#region --- VAR ---


		// Api
		public IActionEntity CurrentTarget { get; private set; } = null;
		public Entity Source { get; private set; } = null;

		// Ser
		[SerializeField] int ScanRange = Const.CELL_SIZE / 2;
		[SerializeField] int ScanFrequency = 6;

		// Data
		private readonly HitInfo[] c_ScanHits = new HitInfo[16];
		private bool RequireRefresh = false;


		#endregion




		#region --- MSG ---


		public void OnActived (Entity source) {
			Source = source;
			CurrentTarget = null;
			ScanFrequency = ScanFrequency.Clamp(1, int.MaxValue);
			RequireRefresh = true;
		}


		public void Update () {
			// Search for Active Trigger
			if (Game.GlobalFrame % ScanFrequency == 0 || RequireRefresh) {
				RequireRefresh = false;
				CurrentTarget = null;
				int count = CellPhysics.OverlapAll(
					c_ScanHits,
					YayaConst.MASK_ENTITY,
					Source.GlobalBounds.Expand(ScanRange, ScanRange, 0, ScanRange),
					Source,
					OperationMode.ColliderAndTrigger
				);
				int dis = int.MaxValue;
				var sourceRect = Source.Rect;
				int sourceX = sourceRect.x + sourceRect.width / 2;
				Entity result = null;
				for (int i = 0; i < count; i++) {
					var hit = c_ScanHits[i];
					if (hit.Entity is not IActionEntity) continue;
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
				CurrentTarget = result as IActionEntity;
			}
			// Highlight
			if (CurrentTarget != null && Source is ePlayer) CurrentTarget.Highlight();
		}


		#endregion




		#region --- API ---


		public bool Invoke () {
			if (CurrentTarget == null) return false;
			if (Source is ePlayer && !FrameInput.KeyDown(CurrentTarget.InvokeKey)) return false;
			return CurrentTarget.Invoke(Source);
		}


		public void CancelInvoke () => CurrentTarget?.CancelInvoke(Source);


		#endregion




		#region --- LGC ---




		#endregion




	}
}
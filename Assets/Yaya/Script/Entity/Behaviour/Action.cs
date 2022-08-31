using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	[System.Serializable]
	public class Action : EntityBehaviour<Entity> {




		#region --- VAR ---


		// Api
		public IActionEntity CurrentTarget { get; private set; } = null;

		// Ser
		[SerializeField] int ScanRange = Const.CELL_SIZE / 4;
		[SerializeField] int ScanFrequency = 6;

		// Data
		[System.NonSerialized] readonly HitInfo[] c_ScanHits = new HitInfo[16];


		#endregion




		#region --- MSG ---


		public override void Initialize (Entity source) {
			base.Initialize(source);
			CurrentTarget = null;
			ScanFrequency = ScanFrequency.Clamp(1, int.MaxValue);
		}


		public override void Update () {
			base.Update();
			// Search for Active Trigger
			if (Game.GlobalFrame % ScanFrequency == 0) {
				CurrentTarget = null;
				int count = CellPhysics.OverlapAll(
					c_ScanHits,
					YayaConst.MASK_ENTITY,
					Source.Rect.Expand(ScanRange),
					Source,
					OperationMode.ColliderAndTrigger
				);
				int dis = int.MaxValue;
				for (int i = 0; i < count; i++) {
					var hit = c_ScanHits[i];
					if (hit.Entity is IActionEntity eAct) {
						int _dis = Util.SqrtDistance(
							hit.Entity.X + hit.Entity.Width / 2,
							hit.Entity.Y + hit.Entity.Height / 2,
							Source.X, Source.Y
						);
						if (_dis < dis) {
							dis = _dis;
							CurrentTarget = eAct;
						}
					}
				}
			}
			// Highlight
			if (CurrentTarget != null) CurrentTarget.Highlight();
		}


		#endregion




		#region --- API ---


		public bool Invoke () {
			if (CurrentTarget == null) return false;
			CurrentTarget.Invoke(Source);
			return true;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}
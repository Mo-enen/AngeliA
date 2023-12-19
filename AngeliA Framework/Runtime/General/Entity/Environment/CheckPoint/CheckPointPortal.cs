using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.DontSpawnFromWorld]
	public class CheckPointPortal : CircleFlamePortal {

		protected override Int3 TargetGlobalPosition => TargetUnitPosition.ToGlobal();
		public static readonly int TYPE_ID = typeof(CheckPointPortal).AngeHash();
		private Int3 TargetUnitPosition;
		private int TargetCheckPointID;
		private int InvokeFrame = -1;

		// MSG
		public override void OnActivated () {
			base.OnActivated();
			InvokeFrame = -1;
		}

		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (InvokeFrame >= 0 && Game.GlobalFrame > InvokeFrame + 30) {
				Active = false;
				InvokeFrame = -1;
			}
		}

		public override void FrameUpdate () {

			base.FrameUpdate();

			// Draw Cp Icon
			if (CellRenderer.TryGetSprite(TargetCheckPointID, out var sprite)) {
				const int SIZE = 196;
				var rect = new IRect(Rect.CenterX() - SIZE / 2, Rect.CenterY() - SIZE / 2, SIZE, SIZE);
				var tint = Color.LerpUnclamped(Const.WHITE_0, Const.WHITE, (Game.GlobalFrame - SpawnFrame).PingPong(60) / 60f);
				CellRenderer.Draw(TargetCheckPointID, rect.Fit(sprite), tint, RenderingMaxZ + 1);
			}
		}

		public override bool Invoke () {
			bool result = base.Invoke();
			if (result) {
				InvokeFrame = Game.GlobalFrame;
			}
			return result;
		}

		// API
		public void SetCheckPoint (int checkPointID, Int3 unitPosition) {
			TargetUnitPosition = unitPosition;
			TargetCheckPointID = checkPointID;
		}

	}
}
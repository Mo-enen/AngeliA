using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaFramework {
	public class GrandfatherClockA : GrandfatherClock { }
	public class GrandfatherClockB : GrandfatherClock { }
	public abstract class GrandfatherClock : Furniture, ICombustible {

		private static readonly int HAND_CODE = "Clock Hand".AngeHash();
		private static readonly int PENDULUM_LEG_CODE = "Clock Pendulum Leg".AngeHash();
		private static readonly int PENDULUM_HEAD_CODE = "Clock Pendulum Head".AngeHash();

		protected override Direction3 ModuleType => Direction3.Vertical;
		int ICombustible.BurnStartFrame { get; set; }


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Hands
			if (Pose == FittingPose.Up) {
				DrawClockHands(Rect.Shrink(36), HAND_CODE, 16, 8);
			} else if (Pose == FittingPose.Single) {
				DrawClockHands(Rect.Shrink(36).Shift(0, 24), HAND_CODE, 16, 8);
			}
			// Pendulum
			if (Pose == FittingPose.Mid) {
				DrawClockPendulum(
					artCodeLeg: PENDULUM_LEG_CODE,
					artCodeHead: PENDULUM_HEAD_CODE,
					x: X + Width / 2,
					y: Y + Height - 18,
					length: 156,
					thickness: 16,
					headSize: 64,
					maxRot: 12,
					deltaX: 16
				);
			} else if (Pose == FittingPose.Down) {
				DrawClockPendulum(
					artCodeLeg: PENDULUM_LEG_CODE,
					artCodeHead: PENDULUM_HEAD_CODE,
					x: X + Width / 2,
					y: Y + Height - 18,
					length: 112,
					thickness: 16,
					headSize: 64,
					maxRot: 12,
					deltaX: 16
				);
			}
		}


	}
}

using System.Collections;
using System.Collections.Generic;

namespace AngeliA; 

public abstract class GrandfatherClock : Furniture, ICombustible {

	private static readonly SpriteCode HAND_CODE = "Clock Hand";
	private static readonly SpriteCode PENDULUM_LEG_CODE = "Clock Pendulum Leg";
	private static readonly SpriteCode PENDULUM_HEAD_CODE = "Clock Pendulum Head";

	protected override Direction3 ModuleType => Direction3.Vertical;
	int ICombustible.BurnStartFrame { get; set; }

	public override void LateUpdate () {
		base.LateUpdate();
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

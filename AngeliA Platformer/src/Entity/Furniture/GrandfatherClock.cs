using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Furniture that works as a combustible and verticaly expanding GrandfatherClock
/// </summary>
public abstract class GrandfatherClock : Furniture, ICombustible {

	private static readonly SpriteCode HAND_CODE = "Clock Hand";
	private static readonly SpriteCode PENDULUM_LEG_CODE = "Clock Pendulum Leg";
	private static readonly SpriteCode PENDULUM_HEAD_CODE = "Clock Pendulum Head";

	protected sealed override Direction3 ModuleType => Direction3.Vertical;
	int ICombustible.BurnStartFrame { get; set; }

	public override void LateUpdate () {
		base.LateUpdate();
		// Hands
		if (Pose == FittingPose.Up) {
			FrameworkUtil.DrawClockHands(Rect.Shrink(36), HAND_CODE, 16, 8, Color32.WHITE);
		} else if (Pose == FittingPose.Single) {
			FrameworkUtil.DrawClockHands(Rect.Shrink(36).Shift(0, 24), HAND_CODE, 16, 8, Color32.WHITE);
		}
		// Pendulum
		if (Pose == FittingPose.Mid) {
			FrameworkUtil.DrawClockPendulum(
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
			FrameworkUtil.DrawClockPendulum(
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

using AngeliA;

namespace AngeliA.Platformer;

public class PoseAttack_WaveSingleHanded_SmashOnly : PoseAttack_WaveSingleHanded {
	public static new readonly int TYPE_ID = typeof(PoseAttack_WaveSingleHanded_SmashOnly).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Smash();
	}
	public static void Smash () {
		float ease01 = AttackEase;

		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
			ResetShoulderAndUpperArmPos();
		} else {
			AttackHeadDown(ease01);
			ResetShoulderAndUpperArmPos();
			// Left Side
			if (
				AnimationType == CharacterAnimationType.Idle ||
				AnimationType == CharacterAnimationType.SquatIdle ||
				AnimationType == CharacterAnimationType.SquatMove
			) {
				UpperArmL.LimbRotate(-15 - (int)(ease01 * 48), 500);
				LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
			}
		}

		// Upper Arm R
		const int STEP = 55;
		(int from, int to) = Attackness.AimingDirection switch {
			Direction8.Bottom => (-185 + STEP + STEP, -9 + STEP + STEP),
			Direction8.BottomLeft or Direction8.BottomRight => (-185 + STEP, -9 + STEP),
			Direction8.Left or Direction8.Right => (-185, -9),
			Direction8.TopLeft or Direction8.TopRight => (-185 - STEP, -9 - STEP),
			Direction8.Top => (-185 - STEP - STEP, -9 - STEP - STEP),
			_ => (-185, -9),
		};
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(from, to, ease01));
		UpperArmR.Height += A2G;
		LowerArmR.LimbRotate(0);
		LowerArmR.Height += A2G;


		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		AttackLegShake(ease01);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(-80, 100, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1100, 1400, ease01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();

	}
}

public class PoseAttack_WaveSingleHanded : PoseAnimation {

	public static readonly int TYPE_ID = typeof(PoseAttack_WaveSingleHanded).AngeHash();

	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		switch (Attackness.LastAttackCharged ? 0 : Attackness.AttackStyleIndex % 4) {
			default:
				SmashDown();
				break;
			case 1:
				SmashUp();
				break;
			case 2:
				SlashIn();
				break;
			case 3:
				SlashOut();
				break;
		}
	}

	public static void SmashDown () {

		float ease01 = AttackEase;
		float ease010 = Util.PingPong(AttackEase, 0.5f) * 2f;

		Rendering.PoseRootX += FacingSign * (int)Util.LerpUnclamped(0f, 4f, ease01) * A2G;
		Rendering.PoseRootY += (int)Util.LerpUnclamped(1f, -4f, ease01) * A2G;
		Body.Rotation = FacingSign * (int)Util.LerpUnclamped(-50f, 50f, ease01);
		Head.Rotation = FacingSign * (int)Util.LerpUnclamped(20f, -10f, ease01);
		Body.Height = (int)Util.LerpUnclamped(Body.Height, Body.Height * 2 / 3, ease01);
		Head.Y = Body.Y + Body.Height;
		Rendering.BodyTwist = FacingSign * (int)Util.LerpUnclamped(1000, -1000, ease01);
		Rendering.HeadTwist = FacingSign * (int)Util.LerpUnclamped(260, -260, ease01);

		// Arm
		ResetShoulderAndUpperArmPos();

		UpperArmL.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-50f, 100f, ease01));
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-220f, 50f, ease01));
		UpperArmL.Height -= A2G / 2;
		UpperArmR.Height += A2G;

		LowerArmL.LimbRotate((int)Util.LerpUnclamped(0f, -20f, ease01));
		LowerArmR.LimbRotate((int)Util.LerpUnclamped(0f, 20f, ease01));

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Leg
		if (Target.IsGrounded) {
			var uLegA = FacingRight ? UpperLegL : UpperLegR;
			var uLegB = FacingRight ? UpperLegR : UpperLegL;
			var lLegA = FacingRight ? LowerLegL : LowerLegR;
			var lLegB = FacingRight ? LowerLegR : LowerLegL;
			uLegA.LimbRotate(FacingSign * (int)Util.LerpUnclamped(0, 60f, ease01));
			uLegB.LimbRotate(FacingSign * (int)Util.LerpUnclamped(0, -55f, ease01));
			lLegA.LimbRotate(FacingSign * (int)Util.LerpUnclamped(0, 5, ease01));
			lLegB.LimbRotate(FacingSign * (int)Util.LerpUnclamped(0, 110f, ease01));
		}

		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(-80f, 200f, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1000, 1500, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(500, 1000, ease01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;

	}

	public static void SmashUp () {

		float ease01 = AttackEase;
		float ease010 = Util.PingPong(AttackEase, 0.5f) * 2f;

		Rendering.PoseRootX += FacingSign * (int)Util.LerpUnclamped(4f, -4f, ease01) * A2G;
		Rendering.PoseRootY += (int)Util.LerpUnclamped(-4f, 1.5f, ease010) * A2G;
		Body.Rotation = FacingSign * (int)Util.LerpUnclamped(50f, -30f, ease01);
		Head.Rotation = FacingSign * (int)Util.LerpUnclamped(-10f, 20f, ease01);
		Body.Height = (int)Util.LerpUnclamped(Body.Height * 2 / 3, Body.Height, ease010);
		Head.Y = Body.Y + Body.Height;
		Rendering.BodyTwist = FacingSign * (int)Util.LerpUnclamped(-1000, 200, ease01);
		Rendering.HeadTwist = FacingSign * (int)Util.LerpUnclamped(-260, 100, ease01);

		// Arm
		ResetShoulderAndUpperArmPos();

		UpperArmL.LimbRotate(FacingSign * (int)Util.LerpUnclamped(100f, 60f, ease01));
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(50f, -90f, ease01));
		UpperArmL.Height -= (int)Util.LerpUnclamped(A2G / 2, 0, ease01);
		UpperArmR.Height += (int)Util.LerpUnclamped(A2G, -A2G, ease01);

		LowerArmL.LimbRotate((int)Util.LerpUnclamped(-20f, 0f, ease01));
		LowerArmR.LimbRotate((int)Util.LerpUnclamped(20f, 5f, ease01));

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Leg
		if (Target.IsGrounded) {
			var uLegA = FacingRight ? UpperLegL : UpperLegR;
			var uLegB = FacingRight ? UpperLegR : UpperLegL;
			var lLegA = FacingRight ? LowerLegL : LowerLegR;
			var lLegB = FacingRight ? LowerLegR : LowerLegL;
			uLegA.LimbRotate(FacingSign * (int)Util.LerpUnclamped(60f, -55f, ease01));
			uLegB.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-55f, -60f, ease01));
			uLegA.Height = uLegA.Height * 2 / 3;
			lLegA.LimbRotate(FacingSign * (int)Util.LerpUnclamped(5f, 90f, ease01));
			lLegB.LimbRotate(FacingSign * (int)Util.LerpUnclamped(110f, 5f, ease01));
		}

		FootL.LimbRotate(-FacingSign);
		FootR.LimbRotate(-FacingSign);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(180f, -45f, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(-1000, -1500, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(800, 1000, ease01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		UpperArmR.Z = UpperArmR.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		LowerArmR.Z = LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND + 2;

	}

	public static void SlashIn () {

		float frame01 = AttackLerp;
		float ease01 = AttackEase;
		float ease010 = Util.PingPong(ease01 * 2f, 1f);

		AttackHeadDown(ease01, 1400, 500, 500);
		ResetShoulderAndUpperArmPos();

		Rendering.BodyTwist = -FacingSign * (int)(frame01 * 2000 - 1000);
		Rendering.HeadTwist = -FacingSign * (int)(frame01 * 500 - 250);

		// Left Side
		if (
			AnimationType == CharacterAnimationType.Idle ||
			AnimationType == CharacterAnimationType.SquatIdle ||
			AnimationType == CharacterAnimationType.SquatMove
		) {
			UpperArmL.LimbRotate(15 + (int)(ease01 * 48), 500);
			LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
		}

		// Upper Arm R
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(-100, 65, ease01));
		UpperArmR.Height = (int)(UpperArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		LowerArmR.LimbRotate(0);
		LowerArmR.Height = (int)(LowerArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		AttackLegShake(ease01);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(67, 224, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(1300, 100, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(600, 200, frame01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		UpperArmR.Z = (frame01 < 0.5f ? -1 : 1) * UpperArmR.Z.Abs();
		LowerArmR.Z = (frame01 < 0.5f ? -1 : 1) * LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;
	}

	public static void SlashOut () {

		float frame01 = AttackLerp;
		float ease01 = AttackEase;
		float ease010 = Util.PingPong(ease01 * 2f, 1f);

		AttackHeadDown(ease01, -500, 500, -1000);
		ResetShoulderAndUpperArmPos();

		Rendering.BodyTwist = FacingSign * (int)(frame01 * 2000 - 1000);
		Rendering.HeadTwist = FacingSign * (int)(frame01 * 500 - 250);

		// Left Side
		if (
			AnimationType == CharacterAnimationType.Idle ||
			AnimationType == CharacterAnimationType.SquatIdle ||
			AnimationType == CharacterAnimationType.SquatMove
		) {
			UpperArmL.LimbRotate(15 + (int)(ease01 * 48), 500);
			LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
		}

		// Upper Arm R
		UpperArmR.LimbRotate(FacingSign * (int)Util.LerpUnclamped(178, -50, ease01));
		UpperArmR.Height = (int)(UpperArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));
		LowerArmR.LimbRotate(0);
		LowerArmR.Height = (int)(LowerArmR.Height * Util.LerpUnclamped(1.2f, 0.1f, ease010));

		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);
		HandR.Width += HandR.Width.Sign() * A2G;
		HandR.Height += HandR.Height.Sign() * A2G;

		// Leg
		AttackLegShake(ease01);

		// Grab
		Rendering.HandGrabRotationL = Rendering.HandGrabRotationR =
			FacingSign * (int)Util.LerpUnclamped(197, 128, ease01);
		Rendering.HandGrabScaleL = Rendering.HandGrabScaleR =
			FacingSign * (int)Util.LerpUnclamped(-1300, -100, ease010);
		Rendering.HandGrabAttackTwistL = Rendering.HandGrabAttackTwistR =
			(int)Util.LerpUnclamped(600, 200, frame01);

		// Z
		UpperArmL.Z = UpperArmL.Z.Abs();
		LowerArmL.Z = LowerArmL.Z.Abs();
		UpperArmR.Z = (frame01 < 0.5f ? -1 : 1) * UpperArmR.Z.Abs();
		LowerArmR.Z = (frame01 < 0.5f ? -1 : 1) * LowerArmR.Z.Abs();
		HandR.Z = POSE_Z_HAND;
	}

}

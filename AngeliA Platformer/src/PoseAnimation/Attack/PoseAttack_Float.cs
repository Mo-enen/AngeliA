﻿using AngeliA;

namespace AngeliA.Platformer;

public class PoseAttack_Float : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PoseAttack_Float).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Wave();
	}
	public static void Wave () {

		float ease01 = AttackEase;

		Body.Rotation = FacingSign * (int)((ease01 - 0.3f) * 15);
		Head.Rotation = -Body.Rotation / 2;

		if (IsChargingAttack) {
			AttackHeadDown(ease01, 100, 800, 100);
		} else {
			AttackHeadDown(ease01, 200, 300, 300);
		}
		ResetShoulderAndUpperArmPos(!FacingRight, FacingRight);

		var uArmB = FacingRight ? UpperArmR : UpperArmL;
		var lArmB = FacingRight ? LowerArmR : LowerArmL;
		var handB = FacingRight ? HandR : HandL;

		// Arm Right
		uArmB.LimbRotate((int)Util.LerpUnclamped(FacingSign * -180, FacingSign * -90, ease01));
		lArmB.LimbRotate((int)((1f - ease01) * -10 * FacingSign));
		handB.LimbRotate(FacingSign);

		// Z
		handB.Z = POSE_Z_HAND;

		// Grab
		int handScale = (int)Util.LerpUnclamped(700, 800, ease01);
		Rendering.HandGrabRotationL.Override(0);
		Rendering.HandGrabRotationR.Override(0);
		Rendering.HandGrabScaleL.Override(handScale);
		Rendering.HandGrabScaleR.Override(handScale);

		// Leg
		AttackLegShake(ease01);
	}
}

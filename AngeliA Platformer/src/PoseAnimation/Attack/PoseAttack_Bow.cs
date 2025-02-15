using AngeliA;

namespace AngeliA.Platformer;

public class PoseAttack_Bow : PoseAnimation {
	public static readonly int TYPE_ID = typeof(PoseAttack_Bow).AngeHash();
	public override void Animate (PoseCharacterRenderer renderer) {
		base.Animate(renderer);
		Bow();
	}
	public static void Bow () {

		float ease01 =
			IsChargingAttack ? 1f - AttackEase :
			Attackness.LastAttackCharged ? 1f :
			AttackEase;
		if (!IsChargingAttack) {
			AttackLegShake(ease01);
			AttackHeadDown(ease01, 0, 200, 0);
		}
		ResetShoulderAndUpperArmPos();

		// Arm
		int rotUpperL = 0;
		int rotUpperR = 0;
		int rotLowerL = 0;
		int rotLowerR = 0;
		int grabRot = 0;
		int grabScl = 1000;

		switch (Attackness.AimingDirection) {


			case Direction8.Left: {
				// L
				rotUpperL = 90;
				rotUpperR = -90;
				rotLowerL = 0;
				rotLowerR = 180;
				grabRot = 0;
				grabScl = -1000;
				break;
			}
			case Direction8.Right: {
				// R
				rotUpperL = 90;
				rotUpperR = -90;
				rotLowerL = -180;
				rotLowerR = 0;
				grabRot = 0;
				grabScl = 1000;
				break;
			}
			case Direction8.Top: {
				// T
				rotUpperL = -20;
				rotUpperR = 20;
				rotLowerL = FacingRight ? 0 : -140;
				rotLowerR = FacingRight ? 140 : 0;
				grabRot = -90;
				grabScl = 1000;
				break;
			}
			case Direction8.Bottom: {
				// B
				rotUpperL = -20;
				rotUpperR = 20;
				rotLowerL = FacingRight ? -140 : 0;
				rotLowerR = FacingRight ? 0 : 140;
				grabRot = 90;
				grabScl = 1000;
				break;
			}

			case Direction8.TopLeft: {
				// TL
				rotUpperL = 135;
				rotUpperR = -45 + 10;
				rotLowerL = 0;
				rotLowerR = 180 - 20;
				grabRot = 45;
				grabScl = -1000;
				break;
			}
			case Direction8.TopRight: {
				// TR
				rotUpperL = 45 - 10;
				rotUpperR = -135;
				rotLowerL = -180 + 20;
				rotLowerR = 0;
				grabRot = -45;
				grabScl = 1000;
				break;
			}
			case Direction8.BottomLeft: {
				// BL
				rotUpperL = 45;
				rotUpperR = -135;
				rotLowerL = 0;
				rotLowerR = 180;
				grabRot = -45;
				grabScl = -1000;
				break;
			}
			case Direction8.BottomRight: {
				// BR
				rotUpperL = 135;
				rotUpperR = -45;
				rotLowerL = -180;
				rotLowerR = 0;
				grabRot = 45;
				grabScl = 1000;
				break;
			}

		}

		// Arm Hand
		UpperArmL.LimbRotate((int)Util.LerpUnclamped(UpperArmL.Rotation, rotUpperL, ease01));
		UpperArmR.LimbRotate((int)Util.LerpUnclamped(UpperArmR.Rotation, rotUpperR, ease01));
		LowerArmL.LimbRotate((int)Util.LerpUnclamped(LowerArmL.Rotation, rotLowerL, ease01));
		LowerArmR.LimbRotate((int)Util.LerpUnclamped(LowerArmR.Rotation, rotLowerR, ease01));
		HandL.LimbRotate(FacingSign);
		HandR.LimbRotate(FacingSign);

		// Z
		ShoulderL.Z = ShoulderR.Z = FrontSign * (POSE_Z_HAND - 3);
		UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
		LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
		HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

		// Grab
		Rendering.HandGrabRotationL.Override(grabRot);
		Rendering.HandGrabRotationR.Override(grabRot);
		Rendering.HandGrabScaleL.Override(grabScl);
		Rendering.HandGrabScaleR.Override(grabScl);

	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {

	public class PoseHandheld_Double : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			if (!Target.IsChargingAttack) {
				// Holding
				ResetShoulderAndUpperArm();

				int twistShift = Target.BodyTwist / 50;
				UpperArmL.LimbRotate((FacingRight ? -42 : 29) - twistShift);
				UpperArmR.LimbRotate((FacingRight ? -29 : 42) - twistShift);
				UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
				UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

				LowerArmL.LimbRotate((FacingRight ? -28 : -48) + twistShift / 2);
				LowerArmR.LimbRotate((FacingRight ? 48 : 28) + twistShift / 2);
				LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1592 : 724) / 1000;
				LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1592) / 1000;

				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);

				// Z
				int signZ = Body.FrontSide ? 1 : -1;
				UpperArmL.Z = LowerArmL.Z = signZ * UpperArmL.Z.Abs();
				UpperArmR.Z = LowerArmR.Z = signZ * UpperArmR.Z.Abs();
				HandL.Z = HandR.Z = signZ * POSE_Z_HAND;

				// Grab Rotation
				Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * 1000;
				Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * (
					30 - CurrentAnimationFrame.PingPong(120) / 30
					+ Target.DeltaPositionY.Clamp(-24, 24) / 5
				) - Target.DeltaPositionX.Clamp(-24, 24) / 4;
			} else {
				// Charging
				float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());
				AttackHeadDown(ease01, 100, 800, 1000, 100);
				ResetShoulderAndUpperArm();

				int upperRotA = (int)Mathf.LerpUnclamped(180, 42, ease01);
				int upperRotB = (int)Mathf.LerpUnclamped(180, 29, ease01);
				int lowerRotA = (int)Mathf.LerpUnclamped(0, 28, ease01);
				int lowerRotB = (int)Mathf.LerpUnclamped(-98, 14, ease01);

				UpperArmL.LimbRotate(FacingRight ? -upperRotA : upperRotB);
				UpperArmR.LimbRotate(FacingRight ? -upperRotB : upperRotA);
				UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
				UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

				LowerArmL.LimbRotate(FacingRight ? -lowerRotA : -lowerRotB);
				LowerArmR.LimbRotate(FacingRight ? lowerRotB : lowerRotA);
				LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1592 : 724) / 1000;
				LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1592) / 1000;

				// Upper Arm
				UpperArmL.Height += A2G;
				UpperArmR.Height += A2G;
				LowerArmL.Height += A2G;
				LowerArmR.Height += A2G;

				HandL.LimbRotate(FacingSign);
				HandL.Width += HandL.Width.Sign() * A2G;
				HandL.Height += HandL.Height.Sign() * A2G;

				HandR.LimbRotate(FacingSign);
				HandR.Width += HandR.Width.Sign() * A2G;
				HandR.Height += HandR.Height.Sign() * A2G;

				// Leg
				AttackLegShake(ease01);

				// Grab Rotation
				Target.HandGrabRotationL = Target.HandGrabRotationR =
					FacingSign * (int)Mathf.LerpUnclamped(-37, 100, ease01);
				Target.HandGrabScaleL = Target.HandGrabScaleR =
					FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

				// Z
				UpperArmL.Z = UpperArmL.Z.Abs();
				UpperArmR.Z = UpperArmR.Z.Abs();
				LowerArmL.Z = LowerArmL.Z.Abs();
				LowerArmR.Z = LowerArmR.Z.Abs();
				HandL.Z = POSE_Z_HAND;
				HandR.Z = POSE_Z_HAND;
			}
		}
	}

	public class PoseHandheld_Bow : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			if (!Target.IsChargingAttack) {
				// Holding
				ResetShoulderAndUpperArm();

				int twistShift = Target.BodyTwist / 50;
				UpperArmL.LimbRotate((FacingRight ? -42 : 29) - twistShift);
				UpperArmR.LimbRotate((FacingRight ? -29 : 42) - twistShift);
				UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
				UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

				LowerArmL.LimbRotate((FacingRight ? -28 : -48) + twistShift / 2);
				LowerArmR.LimbRotate((FacingRight ? 48 : 28) + twistShift / 2);
				LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1592 : 724) / 1000;
				LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1592) / 1000;

				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);

				// Z
				int signZ = Body.FrontSide ? 1 : -1;
				UpperArmL.Z = LowerArmL.Z = signZ * UpperArmL.Z.Abs();
				UpperArmR.Z = LowerArmR.Z = signZ * UpperArmR.Z.Abs();
				HandL.Z = HandR.Z = signZ * POSE_Z_HAND;

				// Grab Rotation
				Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * 1000;
				Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * (
					30 - CurrentAnimationFrame.PingPong(120) / 30
					+ Target.DeltaPositionY.Clamp(-24, 24) / 5
				) - Target.DeltaPositionX.Clamp(-24, 24) / 4;
			} else {
				// Charging
				float ease01 = Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());

				AttackHeadDown(ease01, 0, 200, -1000, 0);
				ResetShoulderAndUpperArm();

				// Upper Arm
				int rotUA = FacingRight ? UpperArmL.Rotation : -UpperArmR.Rotation;
				int rotUB = FacingRight ? UpperArmR.Rotation : -UpperArmL.Rotation;
				rotUA = (int)Mathf.LerpUnclamped(rotUA, 90, ease01);
				rotUB = (int)Mathf.LerpUnclamped(rotUB, -90, ease01);
				UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
				UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);

				LowerArmL.LimbRotate(FacingRight ? -90 - rotUA : -0);
				LowerArmR.LimbRotate(FacingRight ? 0 : 90 + rotUA);

				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);

				// Leg
				AttackLegShake(ease01);

				// Z
				UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
				LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
				HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

				// Grab
				Target.HandGrabRotationL = Target.HandGrabRotationR = FacingRight ? 0 : 180;
				Target.HandGrabScaleL = Target.HandGrabScaleR = 1000;
			}
		}
	}

	public class PoseHandheld_Firearm : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			if (!Target.IsChargingAttack) {
				// Holding
				ResetShoulderAndUpperArm();

				int twistShift = Target.BodyTwist / 50;
				UpperArmL.LimbRotate((FacingRight ? -42 : 29) - twistShift);
				UpperArmR.LimbRotate((FacingRight ? -29 : 42) - twistShift);
				UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
				UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

				LowerArmL.LimbRotate((FacingRight ? -28 : -48) + twistShift / 2);
				LowerArmR.LimbRotate((FacingRight ? 48 : 28) + twistShift / 2);
				LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1592 : 724) / 1000;
				LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1592) / 1000;

				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);

				// Z
				int signZ = Body.FrontSide ? 1 : -1;
				UpperArmL.Z = LowerArmL.Z = signZ * UpperArmL.Z.Abs();
				UpperArmR.Z = LowerArmR.Z = signZ * UpperArmR.Z.Abs();
				HandL.Z = HandR.Z = signZ * POSE_Z_HAND;

				// Grab Rotation
				Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * 1000;
				Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * (
					30 - CurrentAnimationFrame.PingPong(120) / 30
					+ Target.DeltaPositionY.Clamp(-24, 24) / 5
				) - Target.DeltaPositionX.Clamp(-24, 24) / 4;
			} else {
				// Charging
				float ease01 = Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());

				AttackHeadDown(ease01, 0, 200, -1000, 0);
				ResetShoulderAndUpperArm();

				// Upper Arm
				int rotUA = FacingRight ? UpperArmL.Rotation : -UpperArmR.Rotation;
				int rotUB = FacingRight ? UpperArmR.Rotation : -UpperArmL.Rotation;
				rotUA = (int)Mathf.LerpUnclamped(rotUA, -90, ease01);
				rotUB = (int)Mathf.LerpUnclamped(rotUB, -90, ease01);
				UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
				UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
				UpperArmL.Height += FacingRight ? 2 * A2G : 2 * -A2G;
				UpperArmR.Height += FacingRight ? 2 * -A2G : 2 * A2G;

				int rotLA = -90 - rotUA;
				int rotLB = (int)Mathf.LerpUnclamped(0, 0, ease01);
				LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
				LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
				LowerArmL.Height += FacingRight ? A2G : -A2G;
				LowerArmR.Height += FacingRight ? -A2G : A2G;

				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);

				// Leg
				AttackLegShake(ease01);

				// Z
				UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
				LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
				HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

				// Grab
				Target.HandGrabRotationL = Target.HandGrabRotationR = FacingRight ? 0 : 180;
				Target.HandGrabScaleL = Target.HandGrabScaleR = 1000;

			}
		}
	}

	public class PoseHandheld_Pole : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			if (!Target.IsChargingAttack) {
				// Holding
				bool dashing = AnimationType == CharacterAnimationType.Dash;

				ResetShoulderAndUpperArm();

				// Upper Arm
				int twistDelta = Target.BodyTwist / 26;
				UpperArmL.LimbRotate((FacingRight ? -2 : 14) - twistDelta);
				UpperArmR.LimbRotate((FacingRight ? -14 : 2) - twistDelta);
				if (dashing) {
					UpperArmL.Height /= 3;
					UpperArmR.Height /= 3;
				} else {
					int deltaY = (Target.DeltaPositionY / 5).Clamp(-20, 20);
					UpperArmL.Height += deltaY;
					UpperArmR.Height += deltaY;
				}

				// Lower Arm
				LowerArmL.LimbRotate((FacingRight ? -24 : 43) + twistDelta);
				LowerArmR.LimbRotate((FacingRight ? -43 : 24) + twistDelta);
				if (dashing) {
					LowerArmL.Height /= 3;
					LowerArmR.Height /= 3;
				} else {
					int deltaY = (Target.DeltaPositionY / 10).Clamp(-20, 20);
					LowerArmL.Height += deltaY;
					LowerArmR.Height += deltaY;
				}

				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);

				// Z
				HandL.Z = FrontSign * POSE_Z_HAND;
				HandR.Z = FrontSign * POSE_Z_HAND;

				// Grab
				int deltaRot = (Target.DeltaPositionY / 10).Clamp(-10, 10);
				Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * (80 + deltaRot);
				Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * 1000;

			} else {
				// Charging
				float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());

				AttackHeadDown(ease01, 100, 800, 1000, 100);
				ResetShoulderAndUpperArm();

				// Upper Arm
				int uRotA = (int)Mathf.LerpUnclamped(-130, 63, ease01);
				int uRotB = (int)Mathf.LerpUnclamped(-79, 43, ease01);
				UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
				UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);

				// Lower Arm
				int lRotA = (int)Mathf.LerpUnclamped(0, -75, ease01);
				int lRotB = (int)Mathf.LerpUnclamped(-98, 0, ease01);
				LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
				LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);

				// Hand
				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);

				// Leg
				AttackLegShake(ease01);

				// Grab
				Target.HandGrabRotationL = Target.HandGrabRotationR =
					FacingSign * (int)Mathf.LerpUnclamped(-58, 107, ease01);
				Target.HandGrabScaleL = Target.HandGrabScaleR =
					FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

				// Z
				UpperArmL.Z = UpperArmL.Z.Abs();
				LowerArmL.Z = LowerArmL.Z.Abs();
				UpperArmR.Z = UpperArmR.Z.Abs();
				LowerArmR.Z = LowerArmR.Z.Abs();
				HandR.Z = POSE_Z_HAND;
			}
		}
	}

	public class PoseHandheld_MagicPole : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			if (!Target.IsChargingAttack) {
				// Normal

				ResetShoulderAndUpperArm();

				int twistShift = Target.BodyTwist / 50;
				UpperArmL.LimbRotate((FacingRight ? -42 : 29) - twistShift);
				UpperArmR.LimbRotate((FacingRight ? -29 : 42) - twistShift);
				UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1306 : 862) / 1000;
				UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1306) / 1000;

				LowerArmL.LimbRotate((FacingRight ? -28 : -48) + twistShift / 2);
				LowerArmR.LimbRotate((FacingRight ? 48 : 28) + twistShift / 2);
				LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1592 : 724) / 1000;
				LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1592) / 1000;

				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);

				// Z
				int signZ = Body.FrontSide ? 1 : -1;
				UpperArmL.Z = LowerArmL.Z = signZ * UpperArmL.Z.Abs();
				UpperArmR.Z = LowerArmR.Z = signZ * UpperArmR.Z.Abs();
				HandL.Z = HandR.Z = signZ * POSE_Z_HAND;

				// Grab Rotation
				Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * 1000;
				Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * (
					30 - CurrentAnimationFrame.PingPong(120) / 30
					+ Target.DeltaPositionY.Clamp(-24, 24) / 5
				) - Target.DeltaPositionX.Clamp(-24, 24) / 4;

			} else {
				// Charge
				float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());
				AttackHeadDown(ease01, 100, 800, 1000, 100);
				ResetShoulderAndUpperArm();

				// Upper Arm
				float armGrowAmount = 1f - ease01;
				int uRotA = (int)Mathf.LerpUnclamped(-90, 13, ease01);
				int uRotB = (int)Mathf.LerpUnclamped(-69, 33, ease01);
				UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
				UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);
				UpperArmL.Height = (int)(UpperArmL.Height * armGrowAmount * (FacingRight ? 1f : 0.2f));
				UpperArmR.Height = (int)(UpperArmR.Height * armGrowAmount * (FacingRight ? 0.2f : 1f));

				// Lower Arm
				int lRotA = (int)Mathf.LerpUnclamped(0, -25, ease01);
				int lRotB = (int)Mathf.LerpUnclamped(-32, 0, ease01);
				LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
				LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);
				LowerArmL.Height = (int)(LowerArmL.Height * armGrowAmount * (FacingRight ? 1f : 0.2f));
				LowerArmR.Height = (int)(LowerArmR.Height * armGrowAmount * (FacingRight ? 0.2f : 1f));

				// Hand
				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);

				// Grab
				Target.HandGrabRotationL = Target.HandGrabRotationR =
					FacingSign * (int)Mathf.LerpUnclamped(-5, 60, ease01);
				Target.HandGrabScaleL = Target.HandGrabScaleR =
					FacingSign * (int)Mathf.LerpUnclamped(1000, 1100, ease01);

				// Z
				UpperArmL.Z = LowerArmL.Z = FrontSign * UpperArmL.Z.Abs();
				UpperArmR.Z = LowerArmR.Z = FrontSign * UpperArmR.Z.Abs();
				HandL.Z = FrontSign * POSE_Z_HAND;
				HandR.Z = FrontSign * POSE_Z_HAND;

				// Leg
				AttackLegShake(ease01);
			}
		}
	}

	public class PoseHandheld_EachHand : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			if (Target.IsChargingAttack) {
				float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());
				AttackHeadDown(ease01, 100, 800, 1000, 100);
				ResetShoulderAndUpperArm();

				UpperArmL.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-175, 0, ease01));
				UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-185, -9, ease01));
				UpperArmL.Height += A2G;
				UpperArmR.Height += A2G;

				LowerArmL.LimbRotate(0);
				LowerArmR.LimbRotate(0);
				LowerArmL.Height += A2G;
				LowerArmR.Height += A2G;

				HandL.LimbRotate(FacingSign);
				HandL.Width += HandL.Width.Sign() * A2G;
				HandL.Height += HandL.Height.Sign() * A2G;

				HandR.LimbRotate(FacingSign);
				HandR.Width += HandR.Width.Sign() * A2G;
				HandR.Height += HandR.Height.Sign() * A2G;

				// Leg
				AttackLegShake(ease01);

				// Z
				UpperArmL.Z = UpperArmL.Z.Abs();
				UpperArmR.Z = UpperArmR.Z.Abs();
				LowerArmL.Z = LowerArmL.Z.Abs();
				LowerArmR.Z = LowerArmR.Z.Abs();
				HandL.Z = FacingSign * POSE_Z_HAND;
				HandR.Z = -FacingSign * POSE_Z_HAND;

				// Grab
				Target.HandGrabRotationL = FacingSign * (int)Mathf.LerpUnclamped(-70, 110, ease01);
				Target.HandGrabScaleL = FacingSign * (int)Mathf.LerpUnclamped(1000, 1300, ease01);
				Target.HandGrabRotationR = FacingSign * (int)Mathf.LerpUnclamped(-80, 100, ease01);
				Target.HandGrabScaleR = FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

			}
		}
	}

	public class PoseHandheld_Single : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			if (Target.IsChargingAttack) {
				// Charging
				bool isThrowing = Target.EquippingWeaponType == WeaponType.Throwing;
				float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());

				AttackHeadDown(ease01, 100, 800, 1000, 100);
				ResetShoulderAndUpperArm();

				// Upper Arm R
				UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-185, -9, ease01));
				if (!isThrowing) UpperArmR.Height += A2G;
				LowerArmR.LimbRotate(0);
				if (!isThrowing) LowerArmR.Height += A2G;

				HandL.LimbRotate(FacingSign);
				HandR.LimbRotate(FacingSign);
				if (!isThrowing) {
					HandR.Width += HandR.Width.Sign() * A2G;
					HandR.Height += HandR.Height.Sign() * A2G;
				}

				// Leg
				AttackLegShake(ease01);

				// Grab
				Target.HandGrabRotationL = Target.HandGrabRotationR =
					FacingSign * (int)Mathf.LerpUnclamped(-80, 100, ease01);
				Target.HandGrabScaleL = Target.HandGrabScaleR =
					FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

				// Z
				UpperArmL.Z = UpperArmL.Z.Abs();
				UpperArmR.Z = UpperArmR.Z.Abs();
				LowerArmL.Z = LowerArmL.Z.Abs();
				LowerArmR.Z = LowerArmR.Z.Abs();
				HandR.Z = POSE_Z_HAND;
			}
		}
	}

	public class PoseHandheld_Float : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			if (Target.IsChargingAttack) {
				// Charging
				float ease01 = 1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01());
				AttackHeadDown(ease01, 100, 800, 1000, 100);
				ResetShoulderAndUpperArm(!FacingRight, FacingRight);

				var uArmB = FacingRight ? UpperArmR : UpperArmL;
				var lArmB = FacingRight ? LowerArmR : LowerArmL;
				var handB = FacingRight ? HandR : HandL;

				// Arm Right
				uArmB.LimbRotate((int)Mathf.LerpUnclamped(FacingSign * -180, FacingSign * -90, ease01));
				lArmB.LimbRotate((int)((1f - ease01) * -10 * FacingSign));
				handB.LimbRotate(FacingSign);

				// Z
				handB.Z = POSE_Z_HAND;

				// Grab
				Target.HandGrabRotationL = Target.HandGrabRotationR = 0;
				Target.HandGrabScaleL = Target.HandGrabScaleR = (int)Mathf.LerpUnclamped(700, 800, ease01);

				// Leg
				AttackLegShake(ease01);
			}
		}
	}

}
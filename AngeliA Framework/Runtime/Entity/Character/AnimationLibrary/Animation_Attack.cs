using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static partial class AnimationLibrary {


		public static void Attack_Punch () {

			int aFrame = (Game.GlobalFrame - Target.LastAttackFrame).UDivide(5);
			if (aFrame >= 4) return;

			Head.X += FacingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G;

			Body.X += FacingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G / 2;
			Hip.Y -= A2G / 2;
			Body.Y -= A2G / 2;
			Body.Height = Head.Y - Body.Y;

			// Arm
			UpperArmL.PivotX = 0;
			UpperArmR.PivotX = 1000;


			ShoulderL.X = Body.X - Body.SizeX / 2 + Body.Border.left;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderR.X = Body.X + Body.SizeX / 2 - Body.Border.right;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderL.Height = Mathf.Min(ShoulderL.Height, Body.Height);
			ShoulderR.Height = Mathf.Min(ShoulderR.Height, Body.Height);
			ShoulderL.PivotX = 1000;
			ShoulderR.PivotX = 1000;

			UpperArmL.PivotX = 0;
			UpperArmR.PivotX = 1000;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmL.PivotX = 1000;
			UpperArmR.PivotX = 0;



			var uArmRest = FacingRight ? UpperArmL : UpperArmR;
			var lArmRest = FacingRight ? LowerArmL : LowerArmR;
			var handRest = FacingRight ? HandL : HandR;
			var uArmAtt = FacingRight ? UpperArmR : UpperArmL;
			var lArmAtt = FacingRight ? LowerArmR : LowerArmL;
			var handAtt = FacingRight ? HandR : HandL;

			uArmAtt.Z = lArmAtt.Z = handAtt.Z = aFrame == 0 ? -34 : 34;
			uArmRest.Z = lArmRest.Z = handRest.Z = 34;
			uArmRest.Height = uArmRest.SizeY;
			lArmRest.Height = lArmRest.SizeY;
			handRest.Height = handRest.SizeY;
			uArmAtt.Height = uArmAtt.SizeY;
			lArmAtt.Height = lArmAtt.SizeY;
			handAtt.Height = handAtt.SizeY;

			uArmRest.LimbRotate(FacingSign * -50, 1300);
			lArmRest.LimbRotate(FacingSign * -100);
			handRest.LimbRotate(-FacingSign);

			if (aFrame == 0) {
				uArmAtt.X -= FacingSign * uArmAtt.Height;
				uArmAtt.LimbRotate(FacingSign * -90, 0);
				lArmAtt.LimbRotate(0, 0);
				handAtt.Z = 34;
			} else if (aFrame == 1) {
				uArmAtt.X += FacingSign * A2G;
				uArmAtt.LimbRotate(FacingSign * -90, 500);
				lArmAtt.LimbRotate(0, 500);
				handAtt.Width += handAtt.Width.Sign() * A2G * 3 / 2;
				handAtt.Height += handAtt.Height.Sign() * A2G * 3 / 2;
			} else if (aFrame == 2) {
				uArmAtt.LimbRotate(FacingSign * -90, 500);
				lArmAtt.LimbRotate(0, 500);
				handAtt.Width += handAtt.Width.Sign() * A2G * 4 / 3;
				handAtt.Height += handAtt.Height.Sign() * A2G * 4 / 3;
			} else {
				uArmAtt.LimbRotate(FacingSign * -35, 800);
				lArmAtt.LimbRotate(-15, 1000);
			}

			handAtt.LimbRotate(-FacingSign);

			// Leg
			if (AnimatedPoseType == CharacterPoseAnimationType.Idle) {
				if (aFrame == 0) {
					UpperLegL.X -= A2G;
					UpperLegR.X += A2G;
					LowerLegL.X -= A2G;
					LowerLegR.X += A2G;
					FootL.X -= A2G;
					FootR.X += A2G;
				} else if (FacingRight) {
					LowerLegL.X -= A2G;
					FootL.X -= A2G;
				} else {
					LowerLegR.X += A2G;
					FootR.X += A2G;
				}
			}

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}


		public static void Attack_SmashDown () {

			int aFrame = (Game.GlobalFrame - Target.LastAttackFrame).UDivide(5);
			if (aFrame >= 4) return;

			UpperArmL.Z = FrontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = FrontSign * UpperArmR.Z.Abs();
			LowerArmL.Z = FrontSign * LowerArmL.Z.Abs();
			LowerArmR.Z = FrontSign * LowerArmR.Z.Abs();
			HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

			Head.X += FacingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G;
			Head.Y += A2G * 2 * (aFrame == 0 ? 0 : (3 - aFrame) * -2);

			Body.Y -= aFrame * A2G / 4;
			Hip.Y -= aFrame * A2G / 4;
			Body.Height = Head.Y - Body.Y;

			// Arm
			ShoulderL.X = Body.X - Body.SizeX / 2 + Body.Border.left;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderR.X = Body.X + Body.SizeX / 2 - Body.Border.right;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderL.Height = Mathf.Min(ShoulderL.Height, Body.Height);
			ShoulderR.Height = Mathf.Min(ShoulderR.Height, Body.Height);
			ShoulderL.PivotX = 1000;
			ShoulderR.PivotX = 1000;

			UpperArmL.PivotX = 0;
			UpperArmR.PivotX = 1000;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmL.PivotX = 1000;
			UpperArmR.PivotX = 0;

			if (aFrame == 0) {
				UpperArmL.LimbRotate(FacingRight ? 135 : -175);
				UpperArmR.LimbRotate(FacingRight ? 175 : -135);
			} else if (aFrame == 1) {
				UpperArmL.LimbRotate(FacingRight ? -30 : 40);
				UpperArmR.LimbRotate(FacingRight ? -40 : 30);
			} else if (aFrame == 2) {
				UpperArmL.LimbRotate(FacingRight ? -15 : 20);
				UpperArmR.LimbRotate(FacingRight ? -20 : 15);
			} else if (aFrame == 3) {
				UpperArmL.LimbRotate(FacingRight ? -5 : 9);
				UpperArmR.LimbRotate(FacingRight ? -9 : 5);
			}
			UpperArmL.Height += A2G;
			UpperArmR.Height += A2G;

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			if (AnimatedPoseType == CharacterPoseAnimationType.Idle) {
				if (aFrame == 0) {
					UpperLegL.X -= A2G;
					UpperLegR.X += A2G;
					LowerLegL.X -= A2G;
					LowerLegR.X += A2G;
					FootL.X -= A2G;
					FootR.X += A2G;
				} else if (FacingRight) {
					LowerLegL.X -= A2G;
					FootL.X -= A2G;
				} else {
					LowerLegR.X += A2G;
					FootR.X += A2G;
				}
			}

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}


		// Wave
		public static void Attack_Wave () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			Attack_HeadDown(ease01);
			ResetShoulderAndUpperArm();

			// Content
			switch (Target.EquippingWeaponHeld) {
				case WeaponHandHeld.SingleHanded:
					Attack_Wave_SingleHanded(ease01);
					break;
				case WeaponHandHeld.DoubleHanded:
					Attack_Wave_DoubleHanded(ease01);
					break;
				case WeaponHandHeld.OneOnEachHand:
					Attack_Wave_EachHand(ease01);
					break;
				case WeaponHandHeld.Pole:
					Attack_Wave_Polearm(ease01);
					break;
			}

			Attack_LegShake(ease01);

		}


		private static void Attack_Wave_SingleHanded (float ease01) {

			// Left Side
			if (
				AnimatedPoseType == CharacterPoseAnimationType.Idle ||
				AnimatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				AnimatedPoseType == CharacterPoseAnimationType.SquatMove
			) {
				UpperArmL.LimbRotate(-15 - FacingSign * (int)(ease01 * 48), 500);
				LowerArmL.LimbRotate(-100 + FacingSign * (int)(ease01 * 48));
			}

			// Upper Arm R
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-185, -9, ease01));
			UpperArmR.Height += A2G;
			LowerArmR.LimbRotate(0);
			LowerArmR.Height += A2G;

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Z
			UpperArmL.Z = LowerArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = UpperArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(-80, 100, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

		}


		private static void Attack_Wave_DoubleHanded (float ease01) {

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

			// Grab Rotation
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(-80, 100, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

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

			// Z
			UpperArmL.Z = LowerArmL.Z = FrontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = FrontSign * UpperArmR.Z.Abs();
			HandL.Z = FrontSign * POSE_Z_HAND;
			HandR.Z = FrontSign * POSE_Z_HAND;

		}


		private static void Attack_Wave_EachHand (float ease01) {

			UpperArmL.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-175, 0, ease01));
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-185, -9, ease01));

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

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

			// Z
			UpperArmR.Z = FrontSign * UpperArmR.Z.Abs();
			LowerArmR.Z = FrontSign * LowerArmR.Z.Abs();
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

			// Grab
			Target.HandGrabRotationL = FacingSign * (int)Mathf.LerpUnclamped(-70, 110, ease01);
			Target.HandGrabScaleL = FacingSign * (int)Mathf.LerpUnclamped(1000, 1300, ease01);
			Target.HandGrabRotationR = FacingSign * (int)Mathf.LerpUnclamped(-80, 100, ease01);
			Target.HandGrabScaleR = FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

		}


		private static void Attack_Wave_Polearm (float ease01) {

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

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(-58, 107, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

			// Z
			UpperArmL.Z = LowerArmL.Z = FrontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = FrontSign * UpperArmR.Z.Abs();
			HandL.Z = FrontSign * POSE_Z_HAND;
			HandR.Z = FrontSign * POSE_Z_HAND;

		}


		// Poke
		public static void Attack_Poke () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			Attack_HeadDown(ease01, 300, 300, 1000);
			ResetShoulderAndUpperArm();

			// Upper Arm
			int uRotA = (int)Mathf.LerpUnclamped(65, -65, ease01);
			int uRotB = (int)Mathf.LerpUnclamped(65, -65, ease01);
			UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
			UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);
			var shorterU = FacingRight ? UpperArmR : UpperArmL;
			var longerU = FacingRight ? UpperArmL : UpperArmR;
			shorterU.Height = (int)(shorterU.Height * (1f + ease01));
			longerU.Height = (int)(longerU.Height * (1f + ease01));

			// Lower Arm
			LowerArmL.LimbRotate(FacingRight ? -10 : 0);
			LowerArmR.LimbRotate(FacingRight ? 0 : 10);

			// Hand
			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			Attack_LegShake(ease01);

			// Z
			UpperArmL.Z = LowerArmL.Z = FrontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = FrontSign * UpperArmR.Z.Abs();
			HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * 90;
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1000, 1200, ease01);

		}


		// Ranged
		public static void Attack_Ranged () {

			bool isFirearm = Target.EquippingWeaponHeld == WeaponHandHeld.Firearm;
			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			Attack_HeadDown(ease01, 0, 200, -1000);
			ResetShoulderAndUpperArm();

			// Upper Arm
			int rotUA = FacingRight ? UpperArmL.Rotation : -UpperArmR.Rotation;
			int rotUB = FacingRight ? UpperArmR.Rotation : -UpperArmL.Rotation;
			rotUA = (int)Mathf.LerpUnclamped(rotUA, isFirearm ? -90 : 90, ease01);
			rotUB = (int)Mathf.LerpUnclamped(rotUB, -90, ease01);
			UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
			UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
			if (isFirearm) {
				UpperArmL.Height += FacingRight ? 2 * A2G : 2 * -A2G;
				UpperArmR.Height += FacingRight ? 2 * -A2G : 2 * A2G;
			}

			int rotLA = -90 - rotUA;
			int rotLB = (int)Mathf.LerpUnclamped(0, 0, ease01);
			LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
			LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
			if (isFirearm) {
				LowerArmL.Height += FacingRight ? A2G : -A2G;
				LowerArmR.Height += FacingRight ? -A2G : A2G;
			}

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			Attack_LegShake(ease01);

			// Z
			UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
			LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
			HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR = FacingRight ? 0 : 180;
			Target.HandGrabScaleL = Target.HandGrabScaleR = 1000;

		}


		// Scratch
		public static void Attack_Scratch () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			Attack_HeadDown(ease01);
			ResetShoulderAndUpperArm();

			if (Target.AttackCombo % 2 == 0) {
				Attack_Scratch_A(ease01);
			} else {
				Attack_Scratch_B(ease01);
			}

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			Attack_LegShake(ease01);

		}


		private static void Attack_Scratch_A (float ease01) {

			// Arm
			int rotUA = (int)Mathf.LerpUnclamped(6, 137, ease01);
			int rotUB = (int)Mathf.LerpUnclamped(-46, 90, ease01);
			UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
			UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
			UpperArmL.Height = UpperArmL.Height * (FacingRight ? 500 : 1200) / 1000;
			UpperArmR.Height = UpperArmR.Height * (FacingRight ? 1200 : 500) / 1000;

			int rotLA = (int)Mathf.LerpUnclamped(-12, -180, ease01);
			int rotLB = (int)Mathf.LerpUnclamped(-24, 27, ease01);
			LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
			LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
			LowerArmL.Height = LowerArmL.Height * (FacingRight ? 500 : 1200) / 1000;
			LowerArmR.Height = LowerArmR.Height * (FacingRight ? 1200 : 500) / 1000;

			// Grab Rotation
			int rotA = (int)Mathf.LerpUnclamped(101, 69, ease01);
			int rotB = (int)Mathf.LerpUnclamped(54, 94, ease01);

			Target.HandGrabRotationL = LowerArmL.Rotation + (FacingRight ? rotA : -rotB);
			Target.HandGrabRotationR = LowerArmR.Rotation + (FacingRight ? rotB : -rotA);
			Target.HandGrabScaleL = FacingRight ? 700 : -1300;
			Target.HandGrabScaleR = FacingRight ? 1300 : -700;

			// Z
			UpperArmL.Z = -FacingSign * FrontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = FacingSign * FrontSign * UpperArmR.Z.Abs();
			LowerArmL.Z = -FacingSign * FrontSign * LowerArmL.Z.Abs();
			LowerArmR.Z = FacingSign * FrontSign * LowerArmR.Z.Abs();
			HandL.Z = -FacingSign * POSE_Z_HAND;
			HandR.Z = FacingSign * POSE_Z_HAND;

		}


		private static void Attack_Scratch_B (float ease01) {

			// Arm
			int rotUA = (int)Mathf.LerpUnclamped(137, -46, ease01);
			int rotUB = (int)Mathf.LerpUnclamped(-44, 35, ease01);
			UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
			UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
			UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1200 : 500) / 1000;
			UpperArmR.Height = UpperArmR.Height * (FacingRight ? 500 : 1200) / 1000;

			int rotLA = (int)Mathf.LerpUnclamped(-180, -45, ease01);
			int rotLB = (int)Mathf.LerpUnclamped(23, 46, ease01);
			LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
			LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
			LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1200 : 500) / 1000;
			LowerArmR.Height = LowerArmR.Height * (FacingRight ? 500 : 1200) / 1000;

			// Grab Rotation
			int rotA = (int)Mathf.LerpUnclamped(93, 72, ease01);
			int rotB = (int)Mathf.LerpUnclamped(94, 54, ease01);
			Target.HandGrabRotationL = LowerArmL.Rotation + (FacingRight ? rotA : -rotB);
			Target.HandGrabRotationR = LowerArmR.Rotation + (FacingRight ? rotB : -rotA);
			Target.HandGrabScaleL = FacingRight ? 1300 : -700;
			Target.HandGrabScaleR = FacingRight ? 700 : -1300;

			// Z
			UpperArmL.Z = FacingSign * FrontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = -FacingSign * FrontSign * UpperArmR.Z.Abs();
			LowerArmL.Z = FacingSign * FrontSign * LowerArmL.Z.Abs();
			LowerArmR.Z = -FacingSign * FrontSign * LowerArmR.Z.Abs();
			HandL.Z = FacingSign * POSE_Z_HAND;
			HandR.Z = -FacingSign * POSE_Z_HAND;

		}


		// Magic
		public static void Attack_Magic () {
			switch (Target.EquippingWeaponHeld) {
				default:
				case WeaponHandHeld.Float:
					Attack_Magic_Float();
					break;
				case WeaponHandHeld.SingleHanded:
					Attack_Magic_SingleHanded();
					break;
				case WeaponHandHeld.Pole:
					Attack_Magic_Pole();
					break;
			}
		}


		private static void Attack_Magic_Float () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			Attack_HeadDown(ease01, 200, 300, 0);
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
			Attack_LegShake(ease01);
		}


		private static void Attack_Magic_SingleHanded () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			Attack_HeadDown(ease01, 200, 300, 0);
			ResetShoulderAndUpperArm(false, true);

			// Upper Arm R
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-165, -80, ease01));
			LowerArmR.LimbRotate(0);
			HandR.LimbRotate(FacingSign);

			// Z
			UpperArmR.Z = LowerArmR.Z = UpperArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(-60, 90, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1000, 1200, ease01);

			// Leg
			Attack_LegShake(ease01);
		}


		private static void Attack_Magic_Pole () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			Attack_HeadDown(ease01, 200, 300, 0);
			ResetShoulderAndUpperArm();

			// Upper Arm
			int uRotA = (int)Mathf.LerpUnclamped(-90, 13, ease01);
			int uRotB = (int)Mathf.LerpUnclamped(-69, 33, ease01);
			UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
			UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);

			// Lower Arm
			int lRotA = (int)Mathf.LerpUnclamped(0, -25, ease01);
			int lRotB = (int)Mathf.LerpUnclamped(-32, 0, ease01);
			LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
			LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);

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
			Attack_LegShake(ease01);

		}


		// UTL
		private static void Attack_HeadDown (float ease01, int headOffsetXAmount = 1000, int headOffsetYAmount = 1000, int bodyOffsetYAmount = 1000) {

			// Head
			int headOffsetX = FacingSign * (int)((0.75f - ease01) * 10 * A2G) - A2G / 3;
			int headOffsetY = (int)((0.75f - ease01) * 10 * A2G) - 5 * A2G;
			headOffsetX = headOffsetX * headOffsetXAmount / 1000;
			headOffsetY = headOffsetY * headOffsetYAmount / 1000;
			if (
				AnimatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				AnimatedPoseType == CharacterPoseAnimationType.SquatMove
			) {
				headOffsetX /= 2;
				headOffsetY /= 2;
			} else if (
				AnimatedPoseType == CharacterPoseAnimationType.JumpDown ||
				AnimatedPoseType == CharacterPoseAnimationType.JumpUp
			) {
				headOffsetX /= 4;
				headOffsetY /= 4;
			}

			Head.X -= headOffsetX.Clamp(-A2G * 2, A2G * 2);
			Head.Y = (Head.Y + headOffsetY).GreaterOrEquel(Body.Y + 1);

			// Body
			int bodyOffsetY = (int)(ease01 * A2G) + A2G * 2;
			bodyOffsetY = bodyOffsetY * bodyOffsetYAmount / 1000;
			Body.Y -= bodyOffsetY;
			Hip.Y -= bodyOffsetY;
			Body.Height = Head.Y - Body.Y;

		}


		private static void Attack_LegShake (float ease01) {
			if (AnimatedPoseType != CharacterPoseAnimationType.Idle) return;
			int deltaX = (int)(2f * ease01 * A2G);
			if (FacingRight) {
				UpperLegL.X -= deltaX / 2;
				LowerLegL.X -= deltaX;
				FootL.X -= deltaX;
				UpperLegR.X += deltaX / 4;
				LowerLegR.X += deltaX / 2;
				FootR.X += deltaX / 2;
			} else {
				UpperLegL.X -= deltaX / 4;
				LowerLegL.X -= deltaX / 2;
				FootL.X -= deltaX / 2;
				UpperLegR.X += deltaX / 2;
				LowerLegR.X += deltaX;
				FootR.X += deltaX;
			}
		}


	}
}
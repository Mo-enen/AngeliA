using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static partial class AnimationLibrary {


		public static void Attack_Punch () {

			int aFrame = (Game.GlobalFrame - Target.LastAttackFrame).UDivide(5);
			if (aFrame >= 4) return;

			int facingSign = FacingRight ? 1 : -1;

			Head.X += facingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G;

			Body.X += facingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G / 2;
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

			uArmRest.LimbRotate(facingSign * -50, 1300);
			lArmRest.LimbRotate(facingSign * -100);
			handRest.LimbRotate(-facingSign);

			if (aFrame == 0) {
				uArmAtt.X -= facingSign * uArmAtt.Height;
				uArmAtt.LimbRotate(facingSign * -90, 0);
				lArmAtt.LimbRotate(0, 0);
				handAtt.Z = 34;
			} else if (aFrame == 1) {
				uArmAtt.X += facingSign * A2G;
				uArmAtt.LimbRotate(facingSign * -90, 500);
				lArmAtt.LimbRotate(0, 500);
				handAtt.Width += handAtt.Width.Sign() * A2G * 3 / 2;
				handAtt.Height += handAtt.Height.Sign() * A2G * 3 / 2;
			} else if (aFrame == 2) {
				uArmAtt.LimbRotate(facingSign * -90, 500);
				lArmAtt.LimbRotate(0, 500);
				handAtt.Width += handAtt.Width.Sign() * A2G * 4 / 3;
				handAtt.Height += handAtt.Height.Sign() * A2G * 4 / 3;
			} else {
				uArmAtt.LimbRotate(facingSign * -35, 800);
				lArmAtt.LimbRotate(-15, 1000);
			}

			handAtt.LimbRotate(-facingSign);

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
			Target.HandGrabRotationL = LowerArmL.Rotation + facingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + facingSign * 90;
		}


		public static void Attack_SmashDown () {

			int aFrame = (Game.GlobalFrame - Target.LastAttackFrame).UDivide(5);
			if (aFrame >= 4) return;

			int facingSign = FacingRight ? 1 : -1;
			int frontSign = FacingFront ? 1 : -1;

			UpperArmL.Z = frontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = frontSign * UpperArmR.Z.Abs();
			LowerArmL.Z = frontSign * LowerArmL.Z.Abs();
			LowerArmR.Z = frontSign * LowerArmR.Z.Abs();
			HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

			Head.X += facingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G;
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

			HandL.LimbRotate(facingSign);
			HandR.LimbRotate(facingSign);
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
			Target.HandGrabRotationL = LowerArmL.Rotation + facingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + facingSign * 90;
		}


		// Wave
		public static void Attack_Wave () {

			float quad01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			// Head
			int headOffsetX = FacingSign * ((0.75f - quad01) * 10 * A2G).RoundToInt() - A2G / 3;
			int headOffsetY = ((0.75f - quad01) * 10 * A2G).RoundToInt() - 5 * A2G;
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
			} else if (AnimatedPoseType != CharacterPoseAnimationType.Idle) {
				headOffsetX = headOffsetX * 2 / 3;
				headOffsetY = headOffsetY * 2 / 3;
			}
			Head.X -= headOffsetX.Clamp(-A2G * 3, A2G * 3);
			Head.Y += headOffsetY;

			// Body
			int bodyOffsetY = (quad01 * A2G).RoundToInt() + A2G * 2;
			Body.Y -= bodyOffsetY;
			Hip.Y -= bodyOffsetY;
			Body.Height = Head.Y - Body.Y;

			// Content
			switch (Target.EquippingWeaponHeld) {
				case WeaponHandHeld.SingleHanded:
					Attack_Wave_SingleHanded(quad01);
					break;
				case WeaponHandHeld.DoubleHanded:
					Attack_Wave_DoubleHanded(quad01);
					break;
				case WeaponHandHeld.OneOnEachHand:
					Attack_Wave_EachHand(quad01);
					break;
				case WeaponHandHeld.Polearm:
					Attack_Wave_Polearm(quad01);
					break;
			}

			// Leg
			if (AnimatedPoseType == CharacterPoseAnimationType.Idle) {
				if (quad01 < 0.25f) {
					UpperLegL.X -= 2 * A2G;
					UpperLegR.X += 2 * A2G;
					LowerLegL.X -= 2 * A2G;
					LowerLegR.X += 2 * A2G;
					FootL.X -= 2 * A2G;
					FootR.X += 2 * A2G;
				} else if (FacingRight) {
					LowerLegL.X -= 2 * A2G;
					FootL.X -= 2 * A2G;
				} else {
					LowerLegR.X += 2 * A2G;
					FootR.X += 2 * A2G;
				}
			}
		}


		private static void Attack_Wave_SingleHanded (float quad01) {

			ResetShoulderAndUpperArm();

			// Left Side
			if (
				AnimatedPoseType == CharacterPoseAnimationType.Idle ||
				AnimatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				AnimatedPoseType == CharacterPoseAnimationType.SquatMove
			) {
				UpperArmL.LimbRotate(-15 - FacingSign * (int)(quad01 * 48), 500);
				LowerArmL.LimbRotate(-100 + FacingSign * (int)(quad01 * 48));
			}

			// Upper Arm R
			UpperArmR.LimbRotate(FacingSign * Mathf.LerpUnclamped(-185, -9, quad01).RoundToInt());
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

			// Grab Rotation
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * Mathf.LerpUnclamped(1100, 1400, quad01).RoundToInt();

		}


		private static void Attack_Wave_DoubleHanded (float quad01) {

			ResetShoulderAndUpperArm();

			int upperRotA = Mathf.LerpUnclamped(180, 42, quad01).RoundToInt();
			int upperRotB = Mathf.LerpUnclamped(180, 29, quad01).RoundToInt();
			int lowerRotA = Mathf.LerpUnclamped(0, 28, quad01).RoundToInt();
			int lowerRotB = Mathf.LerpUnclamped(-98, 14, quad01).RoundToInt();

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
				FacingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * Mathf.LerpUnclamped(1100, 1400, quad01).RoundToInt();

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


		private static void Attack_Wave_EachHand (float quad01) {

			UpperArmR.Z = FrontSign * UpperArmR.Z.Abs();
			LowerArmR.Z = FrontSign * LowerArmR.Z.Abs();
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

			ResetShoulderAndUpperArm();

			UpperArmL.LimbRotate(FacingSign * Mathf.LerpUnclamped(-175, 0, quad01).RoundToInt());
			UpperArmR.LimbRotate(FacingSign * Mathf.LerpUnclamped(-185, -9, quad01).RoundToInt());

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			// Grab Rotation
			Target.HandGrabRotationL = FacingSign * Mathf.LerpUnclamped(-70, 110, quad01).RoundToInt();
			Target.HandGrabScaleL = FacingSign * Mathf.LerpUnclamped(1000, 1300, quad01).RoundToInt();
			Target.HandGrabRotationR = FacingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			Target.HandGrabScaleR = FacingSign * Mathf.LerpUnclamped(1100, 1400, quad01).RoundToInt();

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

		}


		private static void Attack_Wave_Polearm (float quad01) {

			ResetShoulderAndUpperArm();

			// Upper Arm
			int uRotA = Mathf.LerpUnclamped(-130, 63, quad01).RoundToInt();
			int uRotB = Mathf.LerpUnclamped(-79, 43, quad01).RoundToInt();
			UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
			UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);

			// Lower Arm
			int lRotA = Mathf.LerpUnclamped(0, -75, quad01).RoundToInt();
			int lRotB = Mathf.LerpUnclamped(-98, 0, quad01).RoundToInt();
			LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
			LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);

			// Hand
			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * Mathf.LerpUnclamped(-58, 107, quad01).RoundToInt();
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * Mathf.LerpUnclamped(1100, 1400, quad01).RoundToInt();

			// Z
			UpperArmL.Z = LowerArmL.Z = FrontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = FrontSign * UpperArmR.Z.Abs();
			HandL.Z = FrontSign * POSE_Z_HAND;
			HandR.Z = FrontSign * POSE_Z_HAND;

		}


		// Poke
		public static void Attack_Poke () {
			// Double Handed

		}


		// Throw
		public static void Attack_Throw () {



		}


		// Magic
		public static void Attack_Magic () {
			switch (Target.EquippingWeaponHeld) {
				default:
				case WeaponHandHeld.NoHandHeld:

					break;
				case WeaponHandHeld.SingleHanded:

					break;
				case WeaponHandHeld.DoubleHanded:

					break;
				case WeaponHandHeld.OneOnEachHand:

					break;
			}
		}


		// Ranged
		public static void Attack_Bow () {

			float quad01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			ResetShoulderAndUpperArm();

			// Upper Arm
			int rotUA = FacingRight ? UpperArmL.Rotation : -UpperArmR.Rotation;
			int rotUB = FacingRight ? UpperArmR.Rotation : -UpperArmL.Rotation;
			rotUA = Mathf.LerpUnclamped(rotUA, 90, quad01).RoundToInt();
			rotUB = Mathf.LerpUnclamped(rotUB, -90, quad01).RoundToInt();
			UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
			UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);

			int rotLA = -90 - rotUA;
			int rotLB = Mathf.LerpUnclamped(0, 0, quad01).RoundToInt();
			LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
			LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Z
			UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
			LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
			HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR = FacingRight ? 0 : 180;
			Target.HandGrabScaleL = Target.HandGrabScaleR = 1000;

		}


		public static void Attack_Firearm () {

		}


		// Scratch
		public static void Attack_Scratch () {



		}


	}
}
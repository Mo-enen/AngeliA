using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static partial class AnimationLibrary {


		// Data
		public const int POSE_Z_HAND = 36;
		private static Character Target = null;
		private static BodyPart Head = null;
		private static BodyPart Body = null;
		private static BodyPart Hip = null;
		private static BodyPart ShoulderL = null;
		private static BodyPart ShoulderR = null;
		private static BodyPart UpperArmL = null;
		private static BodyPart UpperArmR = null;
		private static BodyPart LowerArmL = null;
		private static BodyPart LowerArmR = null;
		private static BodyPart HandL = null;
		private static BodyPart HandR = null;
		private static BodyPart UpperLegL = null;
		private static BodyPart UpperLegR = null;
		private static BodyPart LowerLegL = null;
		private static BodyPart LowerLegR = null;
		private static BodyPart FootL = null;
		private static BodyPart FootR = null;
		private static bool FacingRight = true;
		private static bool FacingFront = true;
		private static int FacingSign = 0;
		private static int FrontSign = 0;
		private static int CurrentAnimationFrame = 0;
		private static CharacterPoseAnimationType AnimatedPoseType;


		// API
		public static void Begin (Character targetCharacter) {
			Target = targetCharacter;
			CurrentAnimationFrame = targetCharacter.CurrentAnimationFrame;
			Head = targetCharacter.Head;
			Body = targetCharacter.Body;
			Hip = targetCharacter.Hip;
			ShoulderL = targetCharacter.ShoulderL;
			ShoulderR = targetCharacter.ShoulderR;
			UpperArmL = targetCharacter.UpperArmL;
			UpperArmR = targetCharacter.UpperArmR;
			LowerArmL = targetCharacter.LowerArmL;
			LowerArmR = targetCharacter.LowerArmR;
			HandL = targetCharacter.HandL;
			HandR = targetCharacter.HandR;
			UpperLegL = targetCharacter.UpperLegL;
			UpperLegR = targetCharacter.UpperLegR;
			LowerLegL = targetCharacter.LowerLegL;
			LowerLegR = targetCharacter.LowerLegR;
			FootL = targetCharacter.FootL;
			FootR = targetCharacter.FootR;
			FacingRight = targetCharacter.FacingRight;
			FacingFront = targetCharacter.FacingFront;
			AnimatedPoseType = targetCharacter.AnimatedPoseType;
			FacingSign = FacingRight ? 1 : -1;
			FrontSign = FacingFront ? 1 : -1;
		}


		public static void Damage () {

			bool alt = CurrentAnimationFrame.UMod(8) >= 4;
			int halfA2G = A2G / 2;

			if (alt) {
				Body.Height += A2G / 4;
				Head.Y += A2G / 4;
				Head.Width += FacingRight ? halfA2G : -halfA2G;
				Head.Height += halfA2G;
				UpperArmL.Y += A2G / 4;
				UpperArmR.Y += A2G / 4;
			}

			// Arm
			UpperArmL.LimbRotate(alt ? 85 : 65);
			UpperArmR.LimbRotate(alt ? -85 : -65);

			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmL.LimbRotate(alt ? -75 : -55, 500);

			LowerArmR.Z = LowerArmR.Z.Abs();
			LowerArmR.LimbRotate(alt ? 75 : 55, 500);

			HandL.Z = HandL.Z.Abs();
			HandL.LimbRotate(0);

			HandR.Z = HandR.Z.Abs();
			HandR.LimbRotate(1);

			// Leg
			UpperLegL.LimbRotate((FacingRight ? -20 : 70) + (alt ? 5 : 0));
			UpperLegR.LimbRotate((FacingRight ? -70 : 20) + (alt ? -5 : 0));

			LowerLegL.LimbRotate((FacingRight ? 20 : -65) - (alt ? 5 : 0), 500);
			LowerLegL.Height = FacingRight ? LowerLegL.SizeY / 2 : LowerLegL.SizeY * 2 / 3;
			LowerLegL.Z += 2;

			LowerLegR.LimbRotate((FacingRight ? 65 : -20) - (alt ? -5 : 0), 500);
			LowerLegR.Height = !FacingRight ? LowerLegR.SizeY / 2 : LowerLegR.SizeY * 2 / 3;
			LowerLegR.Z += 2;

			FootL.LimbRotate(FacingRight ? 0 : 1);
			FootL.Z += 2;

			FootR.LimbRotate(FacingRight ? 0 : 1);
			FootR.Z += 2;

		}


		public static void Sleep () {

			bool alt = CurrentAnimationFrame.UMod(120) >= 60;

			Target.PoseRootY = 0;

			Body.Height = Body.SizeY / 4;

			Head.X = FacingRight ? A2G * 4 : A2G * -4;
			Head.Y = 0;
			Head.Height -= alt ? A2G : 0;

			// Arm
			ShoulderL.X = UpperArmL.X = Head.X - Head.SizeX / 2 + UpperArmL.SizeX;
			ShoulderL.Y = ShoulderL.Height;
			UpperArmL.Y = 0;

			ShoulderR.X = UpperArmR.X = Head.X + Head.SizeX / 2 - UpperArmR.SizeX;
			ShoulderR.Y = ShoulderR.Height;
			UpperArmR.Y = 0;

			if (FacingRight) {
				UpperArmR.X -= A2G;
			} else {
				UpperArmL.X += A2G;
			}

			UpperArmL.LimbRotate(FacingRight ? 0 : -90, 0);
			UpperArmR.LimbRotate(FacingRight ? 90 : 0, 0);

			LowerArmL.LimbRotate(0);
			LowerArmL.Height = alt ? LowerArmL.SizeY + A2G : LowerArmL.SizeY;
			LowerArmR.LimbRotate(0);
			LowerArmR.Height = alt ? LowerArmR.SizeY + A2G : LowerArmR.SizeY;

			HandL.LimbRotate(0);
			HandR.LimbRotate(1);

			// Leg
			int legX = FacingRight ?
				Body.X - Body.SizeX / 2 + Body.Border.left - A2G * 2 :
				Body.X + Body.SizeX / 2 - Body.Border.right + A2G * 2;
			UpperLegL.X = legX - A2G;
			UpperLegL.Y = 0;
			UpperLegL.LimbRotate(FacingRight ? 90 : -90);
			UpperLegL.Height = UpperLegL.SizeY / 2;

			UpperLegR.X = legX + A2G;
			UpperLegR.Y = 0;
			UpperLegR.LimbRotate(FacingRight ? 90 : -90);
			UpperLegR.Height = UpperLegR.SizeY / 2;

			LowerLegL.LimbRotate(0);
			LowerLegR.LimbRotate(0);

			FootL.LimbRotate(FacingRight ? 0 : 1);
			FootR.LimbRotate(FacingRight ? 0 : 1);

		}


		public static void PassOut () {

			Target.PoseRootY = 0;

			Body.Height = Body.SizeY / 4;

			Head.X = FacingRight ? A2G * 4 : A2G * -4;
			Head.Y = 0;

			// Arm
			ShoulderL.X = UpperArmL.X = FacingRight ? Head.X - Head.Width / 2 : Head.X + Head.Width / 2;
			ShoulderL.Y = ShoulderL.Height;
			UpperArmL.Y = 0;
			UpperArmL.LimbRotate(FacingRight ? -90 : 90, 0);

			ShoulderR.X = UpperArmR.X = FacingRight ? Head.X + Head.Width / 2 : Head.X - Head.Width / 2;
			ShoulderR.Y = ShoulderR.Height;
			UpperArmR.Y = 0;
			UpperArmR.LimbRotate(FacingRight ? -90 : 90, 0);

			LowerArmL.LimbRotate(0, 0);
			LowerArmR.LimbRotate(0, 0);

			HandL.LimbRotate(FacingRight ? 0 : 1);
			HandR.LimbRotate(FacingRight ? 0 : 1);

			// Leg
			int legX = FacingRight ?
				Body.X - Body.SizeX / 2 + Body.Border.left - A2G * 2 :
				Body.X + Body.SizeX / 2 - Body.Border.right + A2G * 2;
			UpperLegL.X = legX - A2G;
			UpperLegL.Y = 0;
			UpperLegL.LimbRotate(FacingRight ? 90 : -90);
			UpperLegL.Height = UpperLegL.SizeY / 2;

			UpperLegR.X = legX + A2G;
			UpperLegR.Y = 0;
			UpperLegR.LimbRotate(FacingRight ? 90 : -90);
			UpperLegR.Height = UpperLegR.SizeY / 2;

			LowerLegL.LimbRotate(0);
			LowerLegR.LimbRotate(0);

			FootL.LimbRotate(FacingRight ? 0 : 1);
			FootR.LimbRotate(FacingRight ? 0 : 1);

		}


		// Override
		public static void HandHeld_Double () {
			if (!Target.IsChargingAttack) {
				HandHeld_DoubleBowFirearm();
			} else {
				Attack_WaveDoubleHanded_SmashDown();
			}
		}


		public static void HandHeld_Bow () {
			if (!Target.IsChargingAttack) {
				HandHeld_DoubleBowFirearm();
			} else {
				Attack_Bow();
			}
		}


		public static void HandHeld_Firearm () {
			if (!Target.IsChargingAttack) {
				HandHeld_DoubleBowFirearm();
			} else {
				Attack_Firearm();
			}
		}


		private static void HandHeld_DoubleBowFirearm () {

			ResetShoulderAndUpperArm();

			int twistShift = Target.PoseTwist / 50;
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

		}


		public static void HandHeld_Pole () {

			if (Target.IsChargingAttack) {
				// Charging
				Attack_WavePolearm_SmashDown();
			} else {
				// Normal
				bool dashing = AnimatedPoseType == CharacterPoseAnimationType.Dash;

				ResetShoulderAndUpperArm();

				// Upper Arm
				int twistDelta = Target.PoseTwist / 26;
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

			}

		}


		public static void HandHeld_Magic_Pole () {
			if (Target.IsChargingAttack) {
				// Charge
				Attack_Magic_Pole();
			} else {
				// Normal

				ResetShoulderAndUpperArm();

				int twistShift = Target.PoseTwist / 50;
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

			}
		}


		public static void HandHeld_Magic_Float_Charging () => Attack_Magic_Float();


		public static void HandHeld_Charging_EachHand () => Attack_WaveEachHand_SmashDown();


		public static void HandHeld_Charging () => Attack_WaveSingleHanded_SmashDown();


		// UTL
		private static void ResetShoulderAndUpperArm (bool resetLeft = true, bool resetRight = true) => ResetShoulderAndUpperArm(Target, resetLeft, resetRight);
		public static void ResetShoulderAndUpperArm (Character character, bool resetLeft = true, bool resetRight = true) {
			if (resetLeft) {
				int bodyBorderL = character.FacingRight ? character.Body.Border.left : character.Body.Border.right;
				character.ShoulderL.X = character.Body.X - character.Body.Width.Abs() / 2 + bodyBorderL;
				character.ShoulderL.Y = character.Body.Y + character.Body.Height - character.Body.Border.up;
				character.ShoulderL.Height = Mathf.Min(character.ShoulderL.Height, character.Body.Height);
				character.ShoulderL.PivotX = 1000;
				character.UpperArmL.X = character.ShoulderL.X;
				character.UpperArmL.Y = character.ShoulderL.Y - character.ShoulderL.Height + character.ShoulderL.Border.down;
				character.UpperArmL.PivotX = 1000;
				character.UpperArmL.Height = character.UpperArmL.SizeY;
			}
			if (resetRight) {
				int bodyBorderR = character.FacingRight ? character.Body.Border.right : character.Body.Border.left;
				character.ShoulderR.X = character.Body.X + character.Body.Width.Abs() / 2 - bodyBorderR;
				character.ShoulderR.Y = character.Body.Y + character.Body.Height - character.Body.Border.up;
				character.ShoulderR.Height = Mathf.Min(character.ShoulderR.Height, character.Body.Height);
				character.ShoulderR.PivotX = 1000;
				character.UpperArmR.X = character.ShoulderR.X;
				character.UpperArmR.Y = character.ShoulderR.Y - character.ShoulderR.Height + character.ShoulderR.Border.down;
				character.UpperArmR.PivotX = 0;
				character.UpperArmR.Height = character.UpperArmR.SizeY;
			}
		}

	}
}
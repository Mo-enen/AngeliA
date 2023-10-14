using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static partial class AnimationLibrary {


		// Data
		private const int POSE_Z_HAND = 36;
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
		public static void HandHeld_Double_Bow () {

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
			UpperArmL.Z = LowerArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = UpperArmR.Z.Abs();
			HandL.Z = HandL.Z.Abs();
			HandR.Z = HandR.Z.Abs();

			// Grab Rotation
			Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * 1000;
			Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * (
				30 - CurrentAnimationFrame.PingPong(120) / 30
				+ Target.DeltaPositionY.Clamp(-24, 24) / 5
			) - Target.DeltaPositionX.Clamp(-24, 24) / 4;

		}


		public static void HandHeld_Polearm () {

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
			Target.HandGrabScaleL = Target.HandGrabScaleR = 1000;

		}


		// UTL
		private static void ResetShoulderAndUpperArm () {

			int bodyBorderL = Target.FacingRight ? Body.Border.left : Body.Border.right;
			int bodyBorderR = Target.FacingRight ? Body.Border.right : Body.Border.left;

			// Shoulder L
			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderL.Height = Mathf.Min(ShoulderL.Height, Body.Height);
			ShoulderL.PivotX = 1000;

			// Shoulder R
			ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderR.Height = Mathf.Min(ShoulderR.Height, Body.Height);
			ShoulderR.PivotX = 1000;

			// Upper Arm
			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmL.PivotX = 1000;
			UpperArmL.Height = UpperArmL.SizeY;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.PivotX = 0;
			UpperArmR.Height = UpperArmR.SizeY;

		}


	}
}
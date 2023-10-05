using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static partial class AnimationLibrary {


		public static void PoseAnimationOverride_Attack_Punch (Character character) {

			bool FacingRight = character.FacingRight;
			var Head = character.Head;
			var Body = character.Body;
			var ShoulderL = character.ShoulderL;
			var ShoulderR = character.ShoulderR;
			var UpperArmL = character.UpperArmL;
			var UpperArmR = character.UpperArmR;
			var LowerArmL = character.LowerArmL;
			var LowerArmR = character.LowerArmR;
			var HandL = character.HandL;
			var HandR = character.HandR;
			var UpperLegL = character.UpperLegL;
			var UpperLegR = character.UpperLegR;
			var LowerLegL = character.LowerLegL;
			var LowerLegR = character.LowerLegR;
			var FootL = character.FootL;
			var FootR = character.FootR;

			int aFrame = (Game.GlobalFrame - character.LastAttackFrame).UDivide(5);
			if (aFrame >= 4) return;

			int facingSign = FacingRight ? 1 : -1;

			Head.X += facingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G;

			Body.X += facingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G / 2;
			character.Hip.Y -= A2G / 2;
			Body.Y -= A2G / 2;
			Body.Height = Head.Y - Body.Y;

			// Arm
			UpperArmL.PivotX = 0;
			UpperArmR.PivotX = 1000;


			ShoulderL.X = Body.X - Body.SizeX / 2 + Body.Border.Left;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.Up;
			ShoulderR.X = Body.X + Body.SizeX / 2 - Body.Border.Right;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.Up;
			ShoulderL.Height = Mathf.Min(ShoulderL.Height, Body.Height);
			ShoulderR.Height = Mathf.Min(ShoulderR.Height, Body.Height);
			ShoulderL.PivotX = 1000;
			ShoulderR.PivotX = 1000;

			UpperArmL.PivotX = 0;
			UpperArmR.PivotX = 1000;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.Down;
			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.Down;
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
			if (character.AnimatedPoseType == CharacterPoseAnimationType.Idle) {
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
		}


		public static void PoseAnimationOverride_Attack_SmashDown (Character character) {

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			var Head = character.Head;
			var Body = character.Body;
			var ShoulderL = character.ShoulderL;
			var ShoulderR = character.ShoulderR;
			var UpperArmL = character.UpperArmL;
			var UpperArmR = character.UpperArmR;
			var LowerArmL = character.LowerArmL;
			var LowerArmR = character.LowerArmR;
			var HandL = character.HandL;
			var HandR = character.HandR;
			var UpperLegL = character.UpperLegL;
			var UpperLegR = character.UpperLegR;
			var LowerLegL = character.LowerLegL;
			var LowerLegR = character.LowerLegR;
			var FootL = character.FootL;
			var FootR = character.FootR;

			int aFrame = (Game.GlobalFrame - character.LastAttackFrame).UDivide(5);
			if (aFrame >= 4) return;

			int facingSign = FacingRight ? 1 : -1;
			int frontSign = FacingFront ? 1 : -1;

			UpperArmL.Z = frontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = frontSign * UpperArmR.Z.Abs();
			LowerArmL.Z = frontSign * LowerArmL.Z.Abs();
			LowerArmR.Z = frontSign * LowerArmR.Z.Abs();
			HandL.Z = (FacingFront ? 34 : -34);
			HandR.Z = (FacingFront ? 34 : -34);

			Head.X += facingSign * (aFrame == 0 ? -2 : (3 - aFrame) * 2) * A2G;
			Head.Y += A2G * 2 * (aFrame == 0 ? 0 : (3 - aFrame) * -2);

			Body.Y -= aFrame * A2G / 4;
			character.Hip.Y -= aFrame * A2G / 4;
			Body.Height = Head.Y - Body.Y;

			// Arm
			ShoulderL.X = Body.X - Body.SizeX / 2 + Body.Border.Left;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.Up;
			ShoulderR.X = Body.X + Body.SizeX / 2 - Body.Border.Right;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.Up;
			ShoulderL.Height = Mathf.Min(ShoulderL.Height, Body.Height);
			ShoulderR.Height = Mathf.Min(ShoulderR.Height, Body.Height);
			ShoulderL.PivotX = 1000;
			ShoulderR.PivotX = 1000;

			UpperArmL.PivotX = 0;
			UpperArmR.PivotX = 1000;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.Down;
			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.Down;
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
			if (character.AnimatedPoseType == CharacterPoseAnimationType.Idle) {
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

		}


	}
}
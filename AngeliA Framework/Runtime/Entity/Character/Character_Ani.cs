using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract partial class Character {


		private void PoseAnimation_Damage () {

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


		private void PoseAnimation_Sleep () {

			bool alt = CurrentAnimationFrame.UMod(120) >= 60;

			PoseRootY = 0;

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
				Body.X - Body.SizeX / 2 + Body.Border.Left - A2G * 2 :
				Body.X + Body.SizeX / 2 - Body.Border.Right + A2G * 2;
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


		private void PoseAnimation_PassOut () {

			PoseRootY = 0;

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
				Body.X - Body.SizeX / 2 + Body.Border.Left - A2G * 2 :
				Body.X + Body.SizeX / 2 - Body.Border.Right + A2G * 2;
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


		// Attack
		public void PoseAnimationOverride_Attack_Punch () {

			int aFrame = (Game.GlobalFrame - LastAttackFrame).UDivide(5);
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
		}


		public void PoseAnimationOverride_Attack_SmashDown () {

			int aFrame = (Game.GlobalFrame - LastAttackFrame).UDivide(5);
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
			Hip.Y -= aFrame * A2G / 4;
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

		}


		// Weapon Holding




	}
}
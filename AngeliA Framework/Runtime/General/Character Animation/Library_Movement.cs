using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	// General
	public class PoseAnimation_Animation_TakingDamage : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

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
	}

	public class PoseAnimation_Sleep : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
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
	}

	public class PoseAnimation_PassOut : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			Target.PoseRootY = 0;

			Body.Height = Body.SizeY / 4;

			Head.X = FacingRight ? A2G * 4 : A2G * -4;
			Head.Y = 0;

			// Arm
			ShoulderL.X = UpperArmL.X = FacingRight ? Head.X - Head.Width / 2 : Head.X + Head.Width / 2;
			ShoulderL.Y = ShoulderL.Height;
			UpperArmL.Y = 0;
			UpperArmL.LimbRotate(FacingRight ? -90 : 90, 0);
			UpperArmL.Height /= 2;

			ShoulderR.X = UpperArmR.X = FacingRight ? Head.X + Head.Width / 2 : Head.X - Head.Width / 2;
			ShoulderR.Y = ShoulderR.Height;
			UpperArmR.Y = 0;
			UpperArmR.LimbRotate(FacingRight ? -90 : 90, 0);
			UpperArmR.Height /= 2;

			LowerArmL.LimbRotate(0, 0);
			LowerArmR.LimbRotate(0, 0);
			LowerArmL.Height /= 2;
			LowerArmR.Height /= 2;

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
	}


	// Motion
	public class PoseAnimation_Idle : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			const int LOOP = 128;
			int currentFrame = (CurrentAnimationFrame.UMod(128) / 16) * (LOOP / 8);

			float ease = Ease.InOutCirc((currentFrame % (LOOP / 2f)) / (LOOP / 2f));
			if (currentFrame >= LOOP / 2) ease = 1f - ease;
			int oneEase = (int)(ease * A2G);
			int halfEase = (int)(ease * A2G / 2);
			int bodyBorderL = FacingRight ? Body.Border.left : Body.Border.right;
			int bodyBorderR = FacingRight ? Body.Border.right : Body.Border.left;

			Body.Width += FacingSign * halfEase;
			Body.Height -= oneEase;
			Head.Y -= oneEase;

			int ARM_SHIFT_XL = FacingRight ? A2G * 2 / 3 : 0;
			int ARM_SHIFT_XR = FacingRight ? 0 : A2G * 2 / 3;

			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y -= oneEase;
			ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
			ShoulderR.Y -= oneEase;

			// Arm
			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y -= oneEase;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y -= oneEase;

			LowerArmL.X = UpperArmL.X - ARM_SHIFT_XL;
			LowerArmL.Y -= oneEase;

			LowerArmR.X = UpperArmR.X + ARM_SHIFT_XR;
			LowerArmR.Y -= oneEase;

			// Hand
			HandL.X -= halfEase / 2 + ARM_SHIFT_XL;
			HandL.Y -= oneEase;

			HandR.X += halfEase / 2 + ARM_SHIFT_XR;
			HandR.Y -= oneEase;

			// Leg
			LowerLegL.Y += 1;
			LowerLegR.Y += 1;
			LowerLegL.Height += 1;
			LowerLegR.Height += 1;

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;

		}
	}

	public class PoseAnimation_Walk : PoseAnimation {
		private static readonly float[] EASE = { 0f, 0.03125f, 0.125f, 0.28125f, 0.5f, 0.71875f, 0.875f, 0.96875f, 1f, 0.96875f, 0.875f, 0.71875f, 0.5f, 0.28125f, 0.125f, 0.03125f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f, };
		private static readonly int[,] ROTS = { { -20, 20, 25, -25, 0, 0, }, { -17, 17, 21, -25, 0, 0, }, { -15, 15, 17, -27, 30, 20, }, { -7, 7, 17, -15, 45, 10, }, { 0, 0, -5, -5, 60, 0, }, { 7, -7, -5, 7, 75, 0, }, { 15, -15, -27, 17, 90, 0, }, { 17, -17, -26, 21, 45, 0, }, { 20, -20, -25, 25, 0, 0, }, { 17, -17, -26, 21, 10, 15, }, { 15, -15, -27, 17, 20, 30, }, { 7, -7, -15, 7, 10, 45, }, { 0, 0, -5, -5, 0, 60, }, { -7, 7, 5, -10, 0, 75, }, { -15, 15, 17, -27, 0, 90, }, { -17, 17, 21, -26, 0, 45, }, };
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			const int FRAME_LENGTH = 16;

			int loop = Mathf.Max(FRAME_LENGTH * 50 / Target.WalkSpeed.FinalValue.Clamp(1, 100) / FRAME_LENGTH * FRAME_LENGTH, 1);
			int frameRate = (loop / FRAME_LENGTH).GreaterOrEquel(1);
			int fixedAnimationFrame = (CurrentAnimationFrame + frameRate * 2).UMod(loop);
			int currentFrame = fixedAnimationFrame / frameRate * frameRate;
			int arrFrame = (currentFrame / frameRate) % FRAME_LENGTH;

			if (fixedAnimationFrame == 0) RollRandomFactor(2);

			float ease = EASE[arrFrame];
			float easeDouble = EASE[arrFrame + FRAME_LENGTH];
			int legOffsetX = (int)Mathf.LerpUnclamped(
				0f, (Body.SizeX - Body.Border.horizontal - UpperLegL.SizeX) * 0.7f,
				FacingRight ? ease : 1f - ease
			);

			Target.PoseRootY += (int)(easeDouble * A2G);

			// Arm
			UpperArmL.LimbRotate(ROTS[arrFrame, 0] * FacingSign);
			UpperArmR.LimbRotate(ROTS[arrFrame, 1] * FacingSign);

			LowerArmL.LimbRotate((RandomFactor0 - 500) / 30);
			LowerArmR.LimbRotate((RandomFactor1 - 500) / 30);

			HandL.LimbRotate(-FacingSign);
			HandR.LimbRotate(-FacingSign);

			// Leg
			UpperLegL.X += legOffsetX;
			UpperLegL.LimbRotate(ROTS[arrFrame, 2] * FacingSign);

			UpperLegR.X -= legOffsetX;
			UpperLegR.LimbRotate(ROTS[arrFrame, 3] * FacingSign);

			LowerLegL.LimbRotate(ROTS[arrFrame, 4] * FacingSign);
			LowerLegR.LimbRotate(ROTS[arrFrame, 5] * FacingSign);

			FootL.LimbRotate(FacingRight ? 0 : 1);
			FootR.LimbRotate(FacingRight ? 0 : 1);

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}
	}

	public class PoseAnimation_Run : PoseAnimation {
		private static readonly float[] EASE = { 0f, 0.03125f, 0.125f, 0.28125f, 0.5f, 0.71875f, 0.875f, 0.96875f, 1f, 0.96875f, 0.875f, 0.71875f, 0.5f, 0.28125f, 0.125f, 0.03125f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f, };
		private static readonly int[,] ROTS = { { -10, 80, -65, -90, 45, -55, 80, 60, }, { -10, 80, -65, -90, 45, -55, 80, 60, }, { 1, 68, -68, -86, 32, -42, 90, 29, }, { 1, 68, -68, -86, 32, -42, 90, 29, }, { 35, 35, -77, -77, -5, -5, 90, 0, }, { 35, 35, -77, -77, -5, -5, 90, 0, }, { 68, 1, -86, -68, -42, 32, 90, 0, }, { 68, 1, -86, -68, -42, 32, 90, 0, }, { 80, -10, -90, -65, -55, 45, 60, 80, }, { 80, -10, -90, -65, -55, 45, 60, 80, }, { 68, 1, -86, -68, -42, 32, 29, 90, }, { 68, 1, -86, -68, -42, 32, 29, 90, }, { 35, 35, -77, -77, -5, -5, 0, 90, }, { 35, 35, -77, -77, -5, -5, 0, 90, }, { 1, 68, -68, -86, 32, -42, 0, 90, }, { 1, 68, -68, -86, 32, -42, 0, 90, }, };
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			const int FRAME_LENGTH = 16;

			int loop = Mathf.Max(FRAME_LENGTH * 75 / Target.RunSpeed.FinalValue.Clamp(1, 1024) / FRAME_LENGTH * FRAME_LENGTH, 1);
			int frameRate = (loop / FRAME_LENGTH).GreaterOrEquel(1);
			int fixedAnimationFrame = (CurrentAnimationFrame + frameRate * 2).UMod(loop);
			int currentFrame = fixedAnimationFrame / frameRate * frameRate;
			int arrFrame = (currentFrame / frameRate) % FRAME_LENGTH;

			if (fixedAnimationFrame == 0) RollRandomFactor();

			float ease = EASE[arrFrame];
			float easeDouble = EASE[arrFrame + FRAME_LENGTH];
			int legOffsetX = (int)Mathf.LerpUnclamped(
				0f, (Body.SizeX - Body.Border.horizontal - UpperLegL.SizeX) * 0.9f,
				FacingRight ? ease : 1f - ease
			);
			float frame01 = (float)arrFrame / FRAME_LENGTH;

			Target.PoseRootY += (int)((1f - easeDouble) * A2G * 2);
			Target.BodyTwist = (int)Mathf.LerpUnclamped(1000f, -1000f, frame01 < 0.5f ? frame01 * 2f : 2f - 2f * frame01);

			// Arm
			UpperArmL.LimbRotate(ROTS[arrFrame, 0] * FacingSign);
			LowerArmL.Height = LowerArmL.SizeY * 4 / 10;

			UpperArmR.LimbRotate(ROTS[arrFrame, 1] * FacingSign);
			LowerArmR.Height = LowerArmR.SizeY * 4 / 10;

			LowerArmL.LimbRotate(ROTS[arrFrame, 2] * FacingSign + (RandomFactor0 - 500) / 15);
			LowerArmR.LimbRotate(ROTS[arrFrame, 3] * FacingSign + (RandomFactor1 - 500) / 15);

			HandL.LimbRotate(FacingRight ? 0 : 1);
			HandR.LimbRotate(FacingRight ? 0 : 1);

			// Leg
			UpperLegL.X += legOffsetX;
			UpperLegL.Z = 1;
			UpperLegL.LimbRotate(ROTS[arrFrame, 4] * FacingSign);

			UpperLegR.X -= legOffsetX;
			UpperLegR.Z = 1;
			UpperLegR.LimbRotate(ROTS[arrFrame, 5] * FacingSign);

			LowerLegL.LimbRotate(ROTS[arrFrame, 6] * FacingSign + (RandomFactor2 - 500) / 20);
			LowerLegL.Z = 2;

			LowerLegR.LimbRotate(ROTS[arrFrame, 7] * FacingSign + (RandomFactor3 - 500) / 20);
			LowerLegR.Z = 2;

			FootL.LimbRotate(-FacingSign);
			FootL.Z = 3;

			FootR.LimbRotate(-FacingSign);
			FootR.Z = 3;

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}
	}

	public class PoseAnimation_JumpUp : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			bool alt = CurrentAnimationFrame.UMod(8) >= 4;

			Target.PoseRootY += A2G;
			Target.BodyTwist = FacingRight ? -400 : 400;

			if (alt) {
				Body.Height += A2G / 4;
				Head.Y += A2G / 4;
				UpperArmL.Y += A2G / 4;
				UpperArmR.Y += A2G / 4;
			}
			Head.Height += A2G;

			// Arm
			int motionDelta = -Target.DeltaPositionX.Clamp(-8, 8);
			UpperArmL.LimbRotate((alt ? 65 : 55) + motionDelta);
			UpperArmR.LimbRotate((alt ? -65 : -55) + motionDelta);

			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmL.LimbRotate(alt ? -55 : -45, 500);

			LowerArmR.Z = LowerArmR.Z.Abs();
			LowerArmR.LimbRotate(alt ? 55 : 45, 500);

			HandL.Z = HandL.Z.Abs();
			HandL.LimbRotate(0);

			HandR.Z = HandR.Z.Abs();
			HandR.LimbRotate(1);

			// Leg
			UpperLegL.X += FacingRight ? -A2G / 2 : A2G / 2;
			UpperLegL.LimbRotate(FacingRight ? 0 : 20);

			UpperLegR.X += FacingRight ? -A2G / 2 : A2G / 2;
			UpperLegR.LimbRotate(FacingRight ? -20 : 0);

			LowerLegL.LimbRotate(FacingRight ? 0 : -45);
			LowerLegL.Height = FacingRight ? LowerLegL.SizeY * 3 / 4 : LowerLegL.SizeY;
			LowerLegL.Z += 2;

			LowerLegR.LimbRotate(FacingRight ? 45 : 0);
			LowerLegR.Height = FacingRight ? LowerLegR.SizeY : LowerLegR.SizeY * 3 / 4;
			LowerLegR.Z += 2;

			FootL.LimbRotate(FacingRight ? 0 : 1);
			FootL.Z += 2;

			FootR.LimbRotate(FacingRight ? 0 : 1);
			FootR.Z += 2;

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}
	}

	public class PoseAnimation_JumpDown : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			bool alt = CurrentAnimationFrame.UMod(8) >= 4;

			Target.PoseRootY -= A2G;
			Target.BodyTwist = FacingRight ? -400 : 400;

			if (alt) {
				Body.Height += A2G / 4;
				Head.Y += A2G / 4;
				UpperArmL.Y += A2G / 4;
				UpperArmR.Y += A2G / 4;
			}
			Head.Height -= A2G;

			// Arm
			int motionDelta = Target.DeltaPositionX.Clamp(-8, 8);
			UpperArmL.LimbRotate((alt ? 135 : 125) + motionDelta);
			UpperArmR.LimbRotate((alt ? -125 : -135) + motionDelta);

			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmL.LimbRotate(alt ? 35 : 45);

			LowerArmR.Z = LowerArmR.Z.Abs();
			LowerArmR.LimbRotate(alt ? -45 : -35);

			HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandL.LimbRotate(0);

			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandR.LimbRotate(1);

			// Leg
			UpperLegL.X += FacingRight ? A2G / 2 : -A2G / 2;
			UpperLegL.LimbRotate(FacingRight ? 0 : 25);

			UpperLegR.X += FacingRight ? A2G / 2 : -A2G / 2;
			UpperLegR.LimbRotate(FacingRight ? -25 : 0);

			LowerLegL.LimbRotate(FacingRight ? 3 : -15);
			LowerLegL.Height = LowerLegL.SizeY * 3 / 4;
			LowerLegL.Z += 2;

			LowerLegR.LimbRotate(FacingRight ? 15 : -3);
			LowerLegR.Height = LowerLegR.SizeY * 3 / 4;
			LowerLegR.Z += 2;

			FootL.LimbRotate(FacingRight ? 0 : 1);
			FootL.Z += 2;

			FootR.LimbRotate(FacingRight ? 0 : 1);
			FootR.Z += 2;

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation - FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation - FacingSign * 90;
		}
	}

	public class PoseAnimation_SwimIdle : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int frame0121 = CurrentAnimationFrame.UMod(64) / 16;

			// Arm
			if (frame0121 == 3) frame0121 = 1;
			int easeArm = frame0121 * A2G / 2;

			UpperArmL.LimbRotate(FacingSign * -90);
			UpperArmL.Height = FacingRight ?
				UpperArmL.SizeY + 2 * A2G - easeArm :
				UpperArmL.SizeY / 2 + easeArm;

			UpperArmR.LimbRotate(FacingSign * -90);
			UpperArmR.Height = FacingRight ?
				UpperArmR.SizeY / 2 + easeArm :
				UpperArmR.SizeY + 2 * A2G - easeArm;

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			HandL.LimbRotate(-FacingSign);
			HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandL.Height = HandL.SizeY - frame0121 * A2G / 3;

			HandR.LimbRotate(-FacingSign);
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandR.Height = HandR.SizeY - frame0121 * A2G / 3;

			// Leg
			UpperLegL.LimbRotate((frame0121 - 1) * 10);
			UpperLegR.LimbRotate((frame0121 - 1) * -10);

			LowerLegL.LimbRotate((frame0121 - 1) * -10);
			LowerLegR.LimbRotate((frame0121 - 1) * 10);
			FootL.LimbRotate(-FacingSign);
			FootR.LimbRotate(-FacingSign);

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}
	}

	public class PoseAnimation_SwimMove : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int loop = Mathf.Max((1200 / Target.SwimSpeed.FinalValue).Clamp(1, 128) / 4 * 4, 1);
			int frame = CurrentAnimationFrame.UMod(loop) / (loop / 4);

			int frame0121 = frame == 3 ? 1 : frame;
			int easeArm = frame * A2G;

			Target.BodyTwist = FacingRight ? 0 : 1000;

			Head.X = FacingSign * A2G;

			int bodyOffsetX = FacingSign * 2 * A2G;
			Body.X += bodyOffsetX;
			ShoulderL.X += bodyOffsetX;
			ShoulderR.X += bodyOffsetX;
			UpperArmL.X += bodyOffsetX / 2;
			UpperArmR.X += bodyOffsetX / 2;
			UpperLegL.X += bodyOffsetX;
			UpperLegR.X += bodyOffsetX;

			// Arm
			if (FacingRight) {
				UpperArmL.LimbRotate(FacingSign * -90);
				UpperArmL.Height += 2 * A2G - easeArm;

				LowerArmL.LimbRotate(0);

				HandL.LimbRotate(-FacingSign);
				HandL.Height -= frame0121 * A2G / 3;

				UpperArmR.Imitate(UpperArmL);
				LowerArmR.Imitate(LowerArmL);
				HandR.Imitate(HandL);

			} else {
				UpperArmR.LimbRotate(FacingSign * -90);
				UpperArmR.Height += 2 * A2G - easeArm;

				LowerArmR.LimbRotate(0);

				HandR.LimbRotate(-FacingSign);
				HandR.Height -= frame0121 * A2G / 3;

				UpperArmL.Imitate(UpperArmR);
				LowerArmL.Imitate(LowerArmR);
				HandL.Imitate(HandR);
			}

			// Leg
			UpperLegL.Z += FacingSign * 2;
			UpperLegL.LimbRotate(FacingSign * (25 + FacingSign * frame0121 * 15));
			UpperLegL.Height = UpperLegL.SizeY + A2G - FacingSign * frame0121 * A2G / 2;

			UpperLegR.Z -= FacingSign * 2;
			UpperLegR.LimbRotate(FacingSign * (25 - FacingSign * frame0121 * 15));
			UpperLegR.Height = UpperLegR.SizeY + A2G + FacingSign * frame0121 * A2G / 2;

			LowerLegL.Z += FacingSign * 2;
			LowerLegL.LimbRotate(FacingSign * 85 - UpperLegL.Rotation);

			LowerLegR.Z -= FacingSign * 2;
			LowerLegR.LimbRotate(FacingSign * 85 - UpperLegR.Rotation);

			FootL.Z += FacingSign * 2;
			FootL.LimbRotate(-FacingSign);

			FootR.Z -= FacingSign * 2;
			FootR.LimbRotate(-FacingSign);

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}
	}

	public class PoseAnimation_SquatIdle : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int arrFrame = CurrentAnimationFrame.UMod(64) / 16;

			float ease = Ease.InOutCirc(arrFrame / 4f);
			if (arrFrame >= 2) ease = 1f - ease;
			int oneEase = (int)(ease * A2G);
			int halfEase = (int)(ease * A2G / 2);
			int bodyBorderL = FacingRight ? Body.Border.left : Body.Border.right;
			int bodyBorderR = FacingRight ? Body.Border.right : Body.Border.left;

			int above = Target.PoseRootY /= 2;

			Body.Width += FacingSign * halfEase;
			Body.Height = Body.SizeY / 2 - oneEase;

			Head.Y = Body.Y + Body.Height;
			Head.Height -= A2G;

			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
			UpperArmL.X = ShoulderL.X;
			UpperArmR.X = ShoulderR.X;
			LowerArmL.X = UpperArmL.X;
			LowerArmR.X = UpperArmR.X;

			// Arm
			ShoulderL.Y = Body.Y + Body.Height;
			ShoulderR.Y = Body.Y + Body.Height;
			ShoulderL.Height /= 2;
			ShoulderR.Height /= 2;

			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmL.LimbRotate(25);

			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.Z = UpperArmR.Z.Abs();
			UpperArmR.LimbRotate(-25);

			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmL.LimbRotate(-90);
			LowerArmL.Height = LowerArmL.SizeY * 3 / 4;

			LowerArmR.Z = LowerArmR.Z.Abs();
			LowerArmR.LimbRotate(90);
			LowerArmR.Height = LowerArmR.SizeY * 3 / 4;

			HandL.Z = (FacingFront ? 1 : -1) * HandL.Z.Abs();
			HandL.LimbRotate(0);

			HandR.Z = (FacingFront ? 1 : -1) * HandR.Z.Abs();
			HandR.LimbRotate(1);

			// Leg
			LowerLegL.Height -= A2G;
			LowerLegL.X = UpperLegL.X + (FacingRight ? -A2G : 0);
			LowerLegL.Y = Mathf.Max(UpperLegL.Y - UpperLegL.Height, Body.Y - above + LowerLegL.Height);

			LowerLegR.Height -= A2G;
			LowerLegR.X = UpperLegR.X + (FacingRight ? 0 : A2G);
			LowerLegR.Y = Mathf.Max(UpperLegR.Y - UpperLegR.Height, Body.Y - above + LowerLegR.Height);

			FootL.X = FacingRight ? LowerLegL.X : LowerLegL.X + LowerLegL.SizeX;
			FootR.X = FacingRight ? LowerLegR.X - FootR.SizeX : LowerLegR.X;
			FootL.Y = -above + FootL.Height;
			FootR.Y = -above + FootR.Height;

			// Arm
			UpperArmL.X -= halfEase;
			UpperArmL.Width += halfEase;
			UpperArmR.X += halfEase;
			UpperArmR.Width += halfEase;
			LowerArmL.X -= halfEase;
			LowerArmL.Width += halfEase;
			LowerArmR.X += halfEase;
			LowerArmR.Width += halfEase;
			HandL.X -= halfEase;
			HandR.X += halfEase;

			// Final
			Target.HandGrabRotationL =
				Target.EquippingWeaponHeld == WeaponHandheld.OneOnEachHand ? FacingRight ? 80 : -100 :
				FacingSign * 100;
			Target.HandGrabRotationR =
				Target.EquippingWeaponHeld == WeaponHandheld.OneOnEachHand ? FacingRight ? 100 : -80 :
				FacingSign * 100;

		}
	}

	public class PoseAnimation_SquatMove : PoseAnimation {
		private static readonly float[] EASE = { 0f, 0.03125f, 0.125f, 0.28125f, 0.5f, 0.71875f, 0.875f, 0.96875f, 1f, 0.96875f, 0.875f, 0.71875f, 0.5f, 0.28125f, 0.125f, 0.03125f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f, 0f, 0.04081633f, 0.1632653f, 0.3673469f, 0.6326531f, 0.8367347f, 0.9591837f, 1f, };
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			const int FRAME_LENGTH = 16;

			int loop = Mathf.Max(600 / Target.SquatSpeed.FinalValue.Clamp(1, 256) / FRAME_LENGTH * FRAME_LENGTH, 1);
			int frameRate = (loop / FRAME_LENGTH).GreaterOrEquel(1);
			int arrFrame = (CurrentAnimationFrame.UMod(loop) / frameRate) % FRAME_LENGTH;
			arrFrame = (arrFrame + 4).UMod(FRAME_LENGTH);

			float ease = EASE[arrFrame];
			int easeA2G = (int)(ease * A2G);
			int easeA2G2 = (int)(ease * 2 * A2G);
			int above = Target.PoseRootY = Target.PoseRootY / 2 + easeA2G;

			Body.Height = Body.SizeY / 2 + easeA2G;
			Head.Y = Body.Y + Body.Height;
			Head.Height -= A2G;

			// Arm
			ShoulderL.Y = Body.Y + Body.Height;
			ShoulderR.Y = Body.Y + Body.Height;
			ShoulderL.Height /= 2;
			ShoulderR.Height /= 2;

			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmL.LimbRotate(arrFrame >= 6 && arrFrame <= 12 ? 45 : 25);

			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.Z = UpperArmR.Z.Abs();
			UpperArmR.LimbRotate(arrFrame >= 6 && arrFrame <= 12 ? -45 : -25);

			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmL.LimbRotate(-90);
			LowerArmL.Height = LowerArmL.SizeY * 3 / 4;

			LowerArmR.Z = LowerArmR.Z.Abs();
			LowerArmR.LimbRotate(90);
			LowerArmR.Height = LowerArmR.SizeY * 3 / 4;

			HandL.Z = (FacingFront ? 1 : -1) * HandL.Z.Abs();
			HandL.LimbRotate(0);

			HandR.Z = (FacingFront ? 1 : -1) * HandR.Z.Abs();
			HandR.LimbRotate(1);

			// Leg
			UpperLegL.X -= FacingRight ? easeA2G2 : easeA2G;
			UpperLegR.X += FacingRight ? easeA2G : easeA2G2;

			LowerLegL.Height -= A2G;
			LowerLegL.X = UpperLegL.X + (FacingRight ? -A2G : 0);
			LowerLegL.Y = Mathf.Max(UpperLegL.Y - UpperLegL.Height, Body.Y - above + LowerLegL.Height);

			LowerLegR.Height -= A2G;
			LowerLegR.X = UpperLegR.X + (FacingRight ? 0 : A2G);
			LowerLegR.Y = Mathf.Max(UpperLegR.Y - UpperLegR.Height, Body.Y - above + LowerLegR.Height);

			FootL.X = FacingRight ? LowerLegL.X : LowerLegL.X + LowerLegL.SizeX;
			FootL.Y = FootL.Height - above;

			FootR.X = FacingRight ? LowerLegR.X - FootR.SizeX : LowerLegR.X;
			FootR.Y = FootR.Height - above;

			// Final
			Target.HandGrabRotationL =
				Target.EquippingWeaponHeld == WeaponHandheld.OneOnEachHand ? FacingRight ? 80 : -100 :
				FacingSign * 100;
			Target.HandGrabRotationR =
				Target.EquippingWeaponHeld == WeaponHandheld.OneOnEachHand ? FacingRight ? 100 : -80 :
				FacingSign * 100;

		}
	}

	public class PoseAnimation_Dash : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int aFrame = CurrentAnimationFrame.UMod(8) / 4;
			bool alt = aFrame == 1;

			HandL.Z = HandR.Z = POSE_Z_HAND;

			Target.PoseRootY = 0;
			Target.BodyTwist = -1000;

			Head.X += FacingSign * A2G;
			Body.X += FacingSign * 2 * A2G;
			Hip.X += FacingSign * 2 * A2G;
			UpperLegL.X += FacingSign * 2 * A2G + Body.Width / 4;
			UpperLegR.X += FacingSign * 2 * A2G + Body.Width / 4;
			ShoulderL.X += FacingSign * 2 * A2G;
			ShoulderR.X += FacingSign * 2 * A2G;
			UpperArmL.X += FacingSign * 2 * A2G;
			UpperArmR.X += FacingSign * 2 * A2G;

			Hip.Y = UpperLegL.SizeX;
			Body.Y = Hip.Y + Hip.Height;
			Body.Height = Body.SizeY - A2G;
			Head.Y = Body.Y + Body.Height;
			Head.Height -= A2G;

			if (FacingRight) {
				LowerLegR.Z = 31;
				FootR.Z = 32;
			} else {
				LowerLegL.Z = 31;
				FootL.Z = 32;
			}

			// Arm
			ShoulderL.Y = Body.Y + Body.Height;
			ShoulderR.Y = Body.Y + Body.Height;

			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;

			if (FacingRight) {
				UpperArmR.Y += A2G;
			} else {
				UpperArmL.Y += A2G;
			}
			UpperArmL.LimbRotate((FacingRight ? 60 : 135) + (alt ? -2 : 2));
			UpperArmR.LimbRotate((FacingRight ? -135 : -60) + (alt ? 2 : -2));
			if (FacingRight) {
				UpperArmR.Height = UpperArmR.SizeY + A2G;
			} else {
				UpperArmL.Height = UpperArmL.SizeY + A2G;
			}

			LowerArmL.LimbRotate((FacingRight ? -60 : -45) + (alt ? -2 : 2));
			LowerArmR.LimbRotate((FacingRight ? 45 : 60) + (alt ? 2 : -2));
			if (FacingRight) {
				LowerArmL.Height = LowerArmL.SizeY + A2G;
			} else {
				LowerArmR.Height = LowerArmR.SizeY + A2G;
			}

			HandL.LimbRotate(0);
			HandR.LimbRotate(1);

			// Leg
			UpperLegL.X += FacingRight ? 0 : Body.Width.Abs() / 3;
			UpperLegR.X += FacingRight ? -Body.Width.Abs() / 3 : 0;
			UpperLegL.Y += FacingRight ? 0 : 2 * A2G;
			UpperLegR.Y += FacingRight ? 2 * A2G : 0;
			UpperLegL.LimbRotate((FacingRight ? -90 : 135));
			UpperLegR.LimbRotate((FacingRight ? -135 : 90));

			LowerLegL.LimbRotate((FacingRight ? 0 : -90) + (alt ? 2 : 0));
			LowerLegR.LimbRotate((FacingRight ? 90 : 0) + (alt ? -2 : 0));
			if (FacingRight) {
				LowerLegL.Height = LowerLegL.SizeY + A2G;
			} else {
				LowerLegR.Height = LowerLegR.SizeY + A2G;
			}

			FootL.LimbRotate(-FacingSign);
			FootR.LimbRotate(-FacingSign);

			// Final
			Target.HandGrabRotationL = FacingRight ? 90 : 0;
			Target.HandGrabRotationR = FacingRight ? 0 : -90;
		}
	}

	public class PoseAnimation_Rolling : PoseAnimation {
		private static readonly int[,] ROLLING = { { 1450, +100, -000, 0900, 0500, -020, -025, -015, -040, 70, 80, }, { 1200, +450, -000, 0800, 0250, +025, +030, -025, -030, 75, 85, }, { 0850, +800, -000, -800, -100, -160, -150, -145, -125, 80, 90, }, { 0300, +450, -000, -750, -200, -170, -160, -155, -115, 80, 90, }, { 0650, -100, +000, -750, -200, -170, -160, -155, -115, 80, 90, }, { 0850, -450, +000, -800, -100, -160, -150, -145, -125, 80, 90, }, { 0950, -800, +000, 0800, 0250, -065, -065, -025, -030, 75, 85, }, { 1200, -450, +000, 0900, 0750, -040, -045, -015, -040, 70, 80, }, };
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int arrFrame = CurrentAnimationFrame.UMod(24) / 3;
			int upsideSign = arrFrame < 2 || arrFrame > 5 ? 1 : -1;

			Target.PoseRootY = Target.PoseRootY * ROLLING[arrFrame, 0] / 1500;

			Head.FrontSide = Body.FrontSide = arrFrame < 2 || arrFrame > 5;

			Body.Width = Hip.Width = Body.Width * 2 / 3;
			Head.Height = Head.SizeY * ROLLING[arrFrame, 3] / 1000;
			Body.Height = Body.SizeY * ROLLING[arrFrame, 4] / 1500;

			Head.X = FacingSign * A2G * ROLLING[arrFrame, 1] / 250;
			Body.X = FacingSign * A2G * ROLLING[arrFrame, 2] / 300;
			Head.Y = upsideSign > 0 ? 0 : Head.SizeY;
			Hip.Y = Head.Y - Body.Height;
			Body.Y = Hip.Y + Hip.Height;

			// Arm
			ShoulderL.X = Body.X - Body.Width / 2 + Body.Border.left;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderL.Height /= 2;

			ShoulderR.X = Body.X + Body.Width / 2 - Body.Border.right;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderR.Height /= 2;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmL.Z = upsideSign * UpperArmL.Z.Abs();
			UpperArmL.LimbRotate(FacingSign * ROLLING[arrFrame, 5]);
			UpperArmL.Height /= 2;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.Z = upsideSign * UpperArmR.Z.Abs();
			UpperArmR.LimbRotate(FacingSign * ROLLING[arrFrame, 6]);
			UpperArmR.Height /= 2;

			LowerArmL.LimbRotate(0);
			LowerArmL.Z = upsideSign * LowerArmL.Z.Abs();
			LowerArmL.Height /= 2;

			LowerArmR.LimbRotate(0);
			LowerArmR.Z = upsideSign * LowerArmR.Z.Abs();
			LowerArmR.Height /= 2;

			HandL.LimbRotate(FacingSign * upsideSign * 1);
			HandL.Z = upsideSign * POSE_Z_HAND;

			HandR.LimbRotate(FacingSign * upsideSign * 1);
			HandR.Z = upsideSign * POSE_Z_HAND;

			// Leg
			UpperLegL.X = Body.X - Body.Width.Abs() / 2 + Body.Border.left - FacingSign * A2G * 2;
			UpperLegL.Y = Body.Y;
			UpperLegL.LimbRotate(FacingSign * ROLLING[arrFrame, 7], 0);

			UpperLegR.X = Body.X + Body.Width.Abs() / 2 - Body.Border.right - FacingSign * A2G * 2;
			UpperLegR.Y = Body.Y;
			UpperLegR.LimbRotate(FacingSign * ROLLING[arrFrame, 8], 0);

			LowerLegL.LimbRotate(FacingSign * ROLLING[arrFrame, 9], 0);
			LowerLegR.LimbRotate(FacingSign * ROLLING[arrFrame, 10], 0);

			FootL.LimbRotate(FacingSign * upsideSign * -1);
			FootR.LimbRotate(FacingSign * upsideSign * -1);

			if (upsideSign < 0) {
				UpperLegL.PivotX = 1000 - UpperLegL.PivotX;
				UpperLegR.PivotX = 1000 - UpperLegR.PivotX;
			}

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + Body.Height.Sign() * FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + Body.Height.Sign() * FacingSign * 90;
		}
	}

	public class PoseAnimation_Rush : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int aFrame = CurrentAnimationFrame.Abs() / 3;
			bool alt = aFrame % 2 == 0;

			Target.PoseRootY -= aFrame.Clamp(0, 2) * A2G - A2G * 2;
			Target.BodyTwist = -FacingSign * 1000;
			Head.Height -= A2G;
			Body.X -= FacingSign * A2G / 2;

			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = (FacingFront ? 1 : -1) * HandL.Z.Abs();
			HandR.Z = (FacingFront ? 1 : -1) * HandR.Z.Abs();
			UpperLegL.Z += FacingSign;
			UpperLegR.Z -= FacingSign;
			LowerLegL.Z += FacingSign;
			LowerLegR.Z -= FacingSign;
			FootL.Z += FacingSign;
			FootR.Z -= FacingSign;
			UpperLegL.X -= FacingSign * A2G + (alt ? 3 : 0);
			UpperLegR.X -= FacingSign * A2G + (alt ? 3 : 0);

			if (aFrame == 0) {
				// 0
				UpperArmL.LimbRotate((FacingRight ? 40 : 0));
				UpperArmR.LimbRotate((FacingRight ? 0 : -40));

				LowerArmL.LimbRotate(FacingRight ? 0 : -40);
				LowerArmR.LimbRotate(FacingRight ? 40 : 0);

				UpperLegL.LimbRotate(FacingRight ? 5 : 9);
				UpperLegR.LimbRotate(FacingRight ? -9 : -5);

				LowerLegL.LimbRotate(FacingSign * (FacingRight ? 25 : 35) - UpperLegL.Rotation);
				LowerLegR.LimbRotate(FacingSign * (FacingRight ? 35 : 25) - UpperLegR.Rotation);

			} else if (aFrame == 1) {
				// 1
				UpperArmL.LimbRotate((FacingRight ? 70 : 0));
				UpperArmR.LimbRotate((FacingRight ? 0 : -70));

				LowerArmL.LimbRotate(FacingRight ? 0 : -70);
				LowerArmR.LimbRotate(FacingRight ? 70 : 0);

				UpperLegL.LimbRotate(FacingRight ? 15 : 25);
				UpperLegR.LimbRotate(FacingRight ? -25 : -15);

				LowerLegL.LimbRotate(FacingSign * (FacingRight ? 45 : 65) - UpperLegL.Rotation);
				LowerLegR.LimbRotate(FacingSign * (FacingRight ? 65 : 45) - UpperLegR.Rotation);

			} else {
				// 2..
				UpperArmL.LimbRotate((FacingRight ? 90 : 0) + (alt ? 3 : 0));
				UpperArmR.LimbRotate((FacingRight ? 0 : -90) + (alt ? -3 : 0));

				LowerArmL.LimbRotate((FacingRight ? 0 : -90) + (alt ? 2 : 0));
				LowerArmR.LimbRotate((FacingRight ? 90 : 0) + (alt ? -2 : 0));

				UpperLegL.LimbRotate((FacingRight ? 20 : 30) + (alt ? 3 : 0));
				UpperLegR.LimbRotate((FacingRight ? -30 : -20) + (alt ? -2 : 0));

				LowerLegL.LimbRotate((FacingSign * (FacingRight ? 65 : 85) - UpperLegL.Rotation) + (alt ? 2 : 0));
				LowerLegR.LimbRotate((FacingSign * (FacingRight ? 85 : 65) - UpperLegR.Rotation) + (alt ? -3 : 0));
			}

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);
			FootL.LimbRotate(-FacingSign);
			FootR.LimbRotate(-FacingSign);

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}
	}

	public class PoseAnimation_Crash : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			Target.PoseRootY = 0;

			Body.Height = Body.SizeY / 4;

			Head.X = FacingRight ? A2G * 4 : A2G * -4;
			Head.Y = 0;

			// Arm
			ShoulderL.X = UpperArmL.X = FacingRight ? Head.X - Head.Width / 2 : Head.X + Head.Width / 2;
			ShoulderL.Y = ShoulderL.Height;
			UpperArmL.Y = 0;
			UpperArmL.LimbRotate(FacingRight ? -90 : 90, 0);
			UpperArmL.Height /= 2;

			ShoulderR.X = UpperArmR.X = FacingRight ? Head.X + Head.Width / 2 : Head.X - Head.Width / 2;
			ShoulderR.Y = ShoulderR.Height;
			UpperArmR.Y = 0;
			UpperArmR.LimbRotate(FacingRight ? -90 : 90, 0);
			UpperArmR.Height /= 2;

			LowerArmL.LimbRotate(0, 0);
			LowerArmR.LimbRotate(0, 0);
			LowerArmL.Height /= 2;
			LowerArmR.Height /= 2;

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
	}

	public class PoseAnimation_Pound : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			bool alt = CurrentAnimationFrame % 8 < 4;

			HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			UpperLegL.Z = -3;
			UpperLegR.Z = -3;
			LowerLegL.Z = FacingFront ? 4 : -4;
			LowerLegR.Z = FacingFront ? 4 : -4;
			FootL.Z = FacingFront ? 5 : -5;
			FootR.Z = FacingFront ? 5 : -5;

			Target.PoseRootY = A2G;

			Head.X += FacingSign * A2G;
			Head.Y -= A2G;

			// Arm
			UpperArmL.LimbRotate(135 + (alt ? 15 : 0), 500);
			UpperArmR.LimbRotate(-135 + (alt ? 15 : 0), 500);

			LowerArmL.LimbRotate(-60 + (alt ? -15 : 0), 500);
			LowerArmR.LimbRotate(60 + (alt ? -15 : 0), 500);

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			UpperLegL.LimbRotate((FacingRight ? -125 : 155) + (alt ? 10 : 0), FacingRight ? 1000 : 500);
			UpperLegR.LimbRotate((FacingRight ? -155 : 125) + (alt ? 10 : 0), FacingRight ? 500 : 1000);

			LowerLegL.LimbRotate((FacingRight ? 100 : -120) + (alt ? -5 : 0));
			LowerLegR.LimbRotate((FacingRight ? 120 : -100) + (alt ? -5 : 0));

			FootL.LimbRotate(-FacingSign);
			FootR.LimbRotate(-FacingSign);

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation - 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + 90;
		}
	}

	public class PoseAnimation_Climb : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int frameRate = Mathf.Max(560 / Target.ClimbSpeedY.FinalValue.Clamp(1, 1024) / 8, 1);
			int aFrame = CurrentAnimationFrame.UMod(frameRate * 10 - 1) / frameRate;

			int delayFrame = (aFrame + 1) % 10;
			if (aFrame >= 5) aFrame = 8 - aFrame;
			if (delayFrame >= 5) delayFrame = 8 - delayFrame;

			Target.PoseRootY -= (aFrame - 2).Abs() * A2G;

			// Arm
			UpperArmL.LimbRotate(((3 - delayFrame) * -35 + 135).Clamp(45, 135), 1000);
			UpperArmR.LimbRotate((delayFrame * 35 - 135).Clamp(-135, -45), 1000);

			LowerArmL.LimbRotate(180 - UpperArmL.Rotation, 1000);
			LowerArmR.LimbRotate(-180 - UpperArmR.Rotation, 1000);

			HandL.LimbRotate(1);
			HandR.LimbRotate(0);
			HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

			// Leg
			UpperLegL.LimbRotate((aFrame * 35).Clamp(0, 60), 500);
			UpperLegR.LimbRotate(((3 - aFrame) * -35).Clamp(-60, 0), 500);

			LowerLegL.LimbRotate(-UpperLegL.Rotation - 5, 1000);
			LowerLegR.LimbRotate(-UpperLegR.Rotation + 5, 1000);

			FootL.LimbRotate(-1);
			FootR.LimbRotate(1);

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation - 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + 90;
			Target.HandGrabScaleL = 1000;
			Target.HandGrabScaleR = 1000;
		}
	}

	public class PoseAnimation_Fly : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int frame = CurrentAnimationFrame.UMod(16) / 2;
			int pingpong = frame < 4 ? frame : 8 - frame;
			int motionDelta = (Target.DeltaPositionX * 2).Clamp(-30, 30);

			Target.PoseRootY = (frame < 6 ? frame / 2 : 8 - frame) * -A2G + 2 * A2G;

			Head.Y = 0;

			Body.Height = A2G * 2;
			Hip.Y = Head.Y - Body.Height;
			Body.Y = Hip.Y + Hip.Height;

			// Arm
			ShoulderL.Y = Body.Y + Body.Height;
			ShoulderR.Y = Body.Y + Body.Height;

			UpperArmL.X -= pingpong * A2G / 4;
			UpperArmL.Y = Head.Y;
			UpperArmL.LimbRotate(motionDelta);
			UpperArmL.Height -= pingpong * A2G / 6;

			UpperArmR.X += pingpong * A2G / 4;
			UpperArmR.Y = Head.Y;
			UpperArmR.LimbRotate(motionDelta);
			UpperArmR.Height -= pingpong * A2G / 6;

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			HandL.LimbRotate(0);
			HandR.LimbRotate(1);

			// Leg
			UpperLegL.X -= Body.Width / 2 + FacingSign * A2G;
			UpperLegR.X -= Body.Width / 2 + FacingSign * A2G;
			UpperLegL.Y = Head.Y + A2G * 5;
			UpperLegR.Y = Head.Y + A2G * 5;
			UpperLegL.Z = -34;
			UpperLegR.Z = -34;
			UpperLegL.LimbRotate(20 + motionDelta / 2);
			UpperLegR.LimbRotate(-20 + motionDelta / 2);

			LowerLegL.LimbRotate(pingpong * 4 - 20);
			LowerLegR.LimbRotate(-pingpong * 4 + 20);
			LowerLegL.Z = -33;
			LowerLegR.Z = -33;

			FootL.LimbRotate(-FacingSign);
			FootR.LimbRotate(-FacingSign);
			FootL.Z = -32;
			FootR.Z = -32;

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 60;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 60;
		}
	}

	public class PoseAnimation_Slide : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			bool alt = (CurrentAnimationFrame / 4) % 2 == 0;

			Head.X -= FacingSign * 2 * A2G;
			Head.Width *= -1;

			Body.X -= FacingSign * A2G;

			if (FacingRight) {
				UpperLegL.X += A2G;
			} else {
				UpperLegR.X -= A2G;
			}

			// Arm
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

			UpperArmL.LimbRotate(FacingRight ? 70 : 175);
			UpperArmR.LimbRotate(FacingRight ? -175 : -70);

			if (FacingRight) {
				UpperArmR.Height = UpperArmR.Height * 3 / 2 + (alt ? A2G / 5 : 0);
			} else {
				UpperArmL.Height = UpperArmL.Height * 3 / 2 + (alt ? A2G / 5 : 0);
			}

			LowerArmL.LimbRotate(FacingRight ? -65 : 0);
			LowerArmR.LimbRotate(FacingRight ? 0 : 65);

			if (FacingRight) {
				LowerArmR.Height = LowerArmR.Height * 3 / 2 + (alt ? A2G / 5 : 0);
			} else {
				LowerArmL.Height = LowerArmL.Height * 3 / 2 + (alt ? A2G / 5 : 0);
			}

			HandL.LimbRotate(-FacingSign);
			HandR.LimbRotate(-FacingSign);

			// Leg
			UpperLegL.LimbRotate((FacingRight ? -15 : 30) + (alt ? 1 : 0), FacingRight ? 1000 : 500);
			UpperLegR.LimbRotate((FacingRight ? -30 : 15) + (alt ? 0 : 1), FacingRight ? 500 : 1000);

			LowerLegL.LimbRotate(FacingRight ? -30 : -20, FacingRight ? 1000 : 300);
			LowerLegR.LimbRotate(FacingRight ? 20 : 30, FacingRight ? 300 : 1000);

			FootL.LimbRotate(-FacingSign);
			FootR.LimbRotate(-FacingSign);

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}
	}

	public class PoseAnimation_GrabTop : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			int loop = Mathf.Max((700 / Target.GrabMoveSpeedX.FinalValue.Clamp(1, 1024)) / 4 * 4, 1);
			int arrFrame = (CurrentAnimationFrame.UMod(loop) / (loop / 4)) % 4;// 0123
			int pingpong = arrFrame == 3 ? 1 : arrFrame; // 0121
			int pingpongAlt = arrFrame == 2 ? 1 : arrFrame == 3 ? 0 : arrFrame + 1; // 1210

			UpperArmL.Z = UpperArmR.Z = (FacingFront ? 34 : -34);
			LowerArmL.Z = LowerArmR.Z = (FacingFront ? 35 : -35);
			HandL.Z = HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

			Target.PoseRootY += pingpongAlt * A2G;

			// Arm
			UpperArmL.LimbRotate(165 - pingpong * 6, 1500);
			UpperArmR.LimbRotate(-165 + pingpong * 6, 1500);

			UpperArmL.Height += A2G;
			UpperArmR.Height += A2G;

			LowerArmL.LimbRotate(180 - pingpong * 6 - UpperArmL.Rotation);
			LowerArmR.LimbRotate(-180 + pingpong * 6 - UpperArmR.Rotation);

			LowerArmL.Height += 2 * A2G;
			LowerArmR.Height += 2 * A2G;

			HandL.LimbRotate(1);
			HandR.LimbRotate(0);

			// Leg
			UpperLegL.X -= pingpongAlt * A2G / 4;
			UpperLegR.X += pingpongAlt * A2G / 4;

			LowerLegL.X -= pingpongAlt * A2G / 3;
			LowerLegR.X += pingpongAlt * A2G / 3;

			FootL.X -= pingpongAlt * A2G / 2;
			FootR.X += pingpongAlt * A2G / 2;

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}
	}

	public class PoseAnimation_GrabSide : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int loop = Mathf.Max((700 / Target.GrabMoveSpeedY.FinalValue.Clamp(1, 1024)) / 4 * 4, 1);
			int arrFrame = (CurrentAnimationFrame.UMod(loop) / (loop / 4)) % 4;// 0123
			int pingpong = arrFrame == 3 ? 1 : arrFrame; // 0121
			int bodyShift = FacingSign * (Body.Width.Abs() / 2 - A2G * 2);

			Target.PoseRootY -= pingpong * A2G;

			Body.X += bodyShift;

			HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

			// Arm
			ShoulderL.X += bodyShift;
			ShoulderR.X += bodyShift;
			UpperArmL.X += bodyShift;
			UpperArmR.X += bodyShift;
			if (FacingRight) {
				UpperArmR.X -= bodyShift.Abs();
			} else {
				UpperArmL.X += bodyShift.Abs();
			}
			UpperArmL.LimbRotate(FacingSign * (-77 + (pingpong - 1) * -12), 700);
			UpperArmR.LimbRotate(FacingSign * (-77 + (pingpong - 1) * 12), 700);

			LowerArmL.LimbRotate(FacingSign * (pingpong * -28 - 70) - UpperArmL.Rotation, 700);
			LowerArmR.LimbRotate(FacingSign * ((2 - pingpong) * -28 - 70) - UpperArmR.Rotation, 700);

			HandL.LimbRotate(-FacingSign);
			HandR.LimbRotate(-FacingSign);

			// Leg
			UpperLegL.X = Body.X - A2G - Body.Width / 6;
			UpperLegR.X = Body.X + A2G - Body.Width / 6;

			UpperLegL.LimbRotate(FacingSign * (-71 + (pingpong - 1) * 18));
			UpperLegR.LimbRotate(FacingSign * (-71 + (pingpong - 1) * -18));

			LowerLegL.LimbRotate(-UpperLegL.Rotation - (pingpong * 5));
			LowerLegR.LimbRotate(-UpperLegR.Rotation - (pingpong * 5));

			FootL.LimbRotate(-FacingSign);
			FootR.LimbRotate(-FacingSign);

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + FacingSign * 90;
		}
	}

	public class PoseAnimation_Spin : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);

			int aFrame = CurrentAnimationFrame.UMod(8);
			int pingpong = aFrame < 4 ? aFrame : 8 - aFrame; // 01234321
			int pingpong2 = pingpong < 2 ? pingpong : 4 - pingpong; // 01210121
			bool facingFront = aFrame < 4;

			if (!facingFront) {
				FootL.Z -= 2;
				FootR.Z -= 2;
			}

			Target.BodyTwist = (pingpong - 2) * 1000;
			Target.HeadTwist = (pingpong - 2) * -300;

			Head.FrontSide = facingFront;
			Body.FrontSide = facingFront;

			Body.Width = FacingSign * (facingFront ? 1 : -1) * (Body.SizeX - pingpong2 * A2G);
			Body.Height += A2G;

			Head.Width = FacingSign * (facingFront ? 1 : -1) * (Head.SizeX - pingpong2 * A2G);
			Head.Height += A2G;
			Head.X += (pingpong - 2) * A2G / 2;
			Head.Y = Body.Y + Body.Height;

			UpperArmL.Z = LowerArmL.Z;
			UpperArmR.Z = LowerArmR.Z;

			// Arm
			ShoulderL.X += pingpong * (Body.SizeX / 5);
			ShoulderR.X -= pingpong * (Body.SizeX / 5);
			UpperArmL.X += pingpong * (Body.SizeX / 5);
			UpperArmR.X -= pingpong * (Body.SizeX / 5);
			UpperArmL.LimbRotate(-180 + aFrame / 2 * 12);
			UpperArmR.LimbRotate(180 + aFrame / 2 * -12);

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			UpperLegL.X += pingpong * UpperLegL.SizeX / 4;
			UpperLegR.X -= pingpong * UpperLegR.SizeX / 4;
			UpperLegL.LimbRotate(0, 0);
			UpperLegR.LimbRotate(0, 0);

			LowerLegL.LimbRotate((2 - pingpong) * -10, 0);
			LowerLegR.LimbRotate((2 - pingpong) * 10, 0);

			FootL.LimbRotate(facingFront ? -FacingSign : FacingSign);
			FootR.LimbRotate(facingFront ? -FacingSign : FacingSign);

			// Final
			Target.HandGrabRotationL = LowerArmL.Rotation - 90;
			Target.HandGrabRotationR = LowerArmR.Rotation + 90;
		}
	}

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static partial class AnimationLibrary {


		private const int A2G = Const.CEL / Const.ART_CEL;
		private static readonly float[] WALK_RUN_EASE = { 0f, 0.125f, 0.5f, 0.875f, 1f, 0.875f, 0.5f, 0.125f, 0f, 0.222f, 0.777f, 1f, 0f, 0.222f, 0.777f, 1f, };
		private static readonly int[,] WALK_ROTS = { { -20, 20, 25, -25, 0, 0, }, { -15, 15, 17, -27, 30, 20, }, { 0, 0, -5, -5, 60, 0, }, { 15, -15, -27, 17, 90, 0, }, { 20, -20, -25, 25, 0, 0, }, { 15, -15, -27, 17, 20, 30, }, { 0, 0, -5, -5, 0, 60, }, { -15, 15, 17, -27, 0, 90, }, };
		private static readonly int[,] RUN_ROTS = { { -10, 80, -65, -90, 45, -55, 80, 60, }, { 1, 68, -68, -86, 32, -42, 90, 29, }, { 35, 35, -77, -77, -5, -5, 90, 0, }, { 68, 1, -86, -68, -42, 32, 90, 0, }, { 80, -10, -90, -65, -55, 45, 60, 80, }, { 68, 1, -86, -68, -42, 32, 29, 90, }, { 35, 35, -77, -77, -5, -5, 0, 90, }, { 1, 68, -68, -86, 32, -42, 0, 90, }, };
		private static readonly int[,] ROLLING = { { 1450, +100, -000, 0900, 0500, -020, -025, -015, -040, 70, 80, }, { 1200, +450, -000, 0800, 0250, +025, +030, -025, -030, 75, 85, }, { 0850, +800, -000, -800, -100, -160, -150, -145, -125, 80, 90, }, { 0300, +450, -000, -750, -200, -170, -160, -155, -115, 80, 90, }, { 0650, -100, +000, -750, -200, -170, -160, -155, -115, 80, 90, }, { 0850, -450, +000, -800, -100, -160, -150, -145, -125, 80, 90, }, { 0950, -800, +000, 0800, 0250, -065, -065, -025, -030, 75, 85, }, { 1200, -450, +000, 0900, 0750, -040, -045, -015, -040, 70, 80, }, };



		public static void Idle (Character character) {

			const int LOOP = 128;
			int currentFrame = (character.CurrentAnimationFrame.UMod(128) / 16) * (LOOP / 8);

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
			var LowerLegL = character.LowerLegL;
			var LowerLegR = character.LowerLegR;

			float ease = Ease.InOutCirc((currentFrame % (LOOP / 2f)) / (LOOP / 2f));
			if (currentFrame >= LOOP / 2) ease = 1f - ease;
			int oneEase = (int)(ease * A2G);
			int halfEase = (int)(ease * A2G / 2);
			int facingSign = FacingRight ? 1 : -1;
			int bodyBorderL = FacingRight ? Body.Border.Left : Body.Border.Right;
			int bodyBorderR = FacingRight ? Body.Border.Right : Body.Border.Left;

			Body.Width += facingSign * halfEase;
			Body.Height -= oneEase;
			Head.Y -= oneEase;

			int ARM_SHIFT_XL = FacingRight ? A2G * 2 / 3 : 0;
			int ARM_SHIFT_XR = FacingRight ? 0 : A2G * 2 / 3;

			// Arm
			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y -= oneEase;
			ShoulderR.X = Body.X + Body.Width.Abs() / 2 - bodyBorderR;
			ShoulderR.Y -= oneEase;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y -= oneEase;
			//UpperArmL.Width += halfEase;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y -= oneEase;
			//UpperArmR.Width += halfEase;

			LowerArmL.X = UpperArmL.X - ARM_SHIFT_XL;
			LowerArmL.Y -= oneEase;
			//LowerArmL.Width += halfEase;

			LowerArmR.X = UpperArmR.X + ARM_SHIFT_XR;
			LowerArmR.Y -= oneEase;
			//LowerArmR.Width += halfEase;

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
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;

		}


		public static void Walk (Character character) {

			bool FacingRight = character.FacingRight;
			var Body = character.Body;
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

			int loop = Mathf.Max(800 / character.WalkSpeed.Value.Clamp(1, 100) / 8 * 8, 1);
			int frameRate = loop / 8;
			int currentFrame = (character.CurrentAnimationFrame + frameRate * 2).UMod(loop) / frameRate * frameRate;
			int arrFrame = (currentFrame / frameRate) % 8;

			float ease = WALK_RUN_EASE[arrFrame];
			float easeDouble = WALK_RUN_EASE[arrFrame + 8];
			int legOffsetX = (int)Mathf.LerpUnclamped(
				0f, (Body.SizeX - Body.Border.Horizontal - UpperLegL.SizeX) * 0.7f,
				FacingRight ? ease : 1f - ease
			);
			int facingSign = FacingRight ? 1 : -1;

			character.PoseRootY += (int)(easeDouble * A2G);

			// Arm
			UpperArmL.LimbRotate(WALK_ROTS[arrFrame, 0] * facingSign);
			UpperArmR.LimbRotate(WALK_ROTS[arrFrame, 1] * facingSign);

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			HandL.LimbRotate(-facingSign);
			HandR.LimbRotate(-facingSign);

			// Leg
			UpperLegL.X += legOffsetX;
			UpperLegL.LimbRotate(WALK_ROTS[arrFrame, 2] * facingSign);

			UpperLegR.X -= legOffsetX;
			UpperLegR.LimbRotate(WALK_ROTS[arrFrame, 3] * facingSign);

			LowerLegL.LimbRotate(WALK_ROTS[arrFrame, 4] * facingSign);
			LowerLegR.LimbRotate(WALK_ROTS[arrFrame, 5] * facingSign);

			FootL.LimbRotate(FacingRight ? 0 : 1);
			FootR.LimbRotate(FacingRight ? 0 : 1);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void Run (Character character) {

			bool FacingRight = character.FacingRight;
			var Body = character.Body;
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

			int loop = Mathf.Max(1200 / character.RunSpeed.Value.Clamp(1, 1024) / 8 * 8, 1);
			int frameRate = loop / 8;
			int currentFrame = (character.CurrentAnimationFrame + frameRate * 2).UMod(loop) / frameRate * frameRate;
			int arrFrame = (currentFrame / frameRate) % 8;

			float ease = WALK_RUN_EASE[arrFrame];
			float easeDouble = WALK_RUN_EASE[arrFrame + 8];
			int legOffsetX = (int)Mathf.LerpUnclamped(
				0f, (Body.SizeX - Body.Border.Horizontal - UpperLegL.SizeX) * 0.9f,
				FacingRight ? ease : 1f - ease
			);
			int facingSign = FacingRight ? 1 : -1;
			float frame01 = arrFrame / 8f;

			character.PoseRootY += (int)((1f - easeDouble) * A2G * 2);
			character.PoseTwist = (int)Mathf.LerpUnclamped(1000f, -1000f, frame01 < 0.5f ? frame01 * 2f : 2f - 2f * frame01);

			// Arm
			UpperArmL.LimbRotate(RUN_ROTS[arrFrame, 0] * facingSign);
			LowerArmL.Height = LowerArmL.SizeY * 4 / 10;

			UpperArmR.LimbRotate(RUN_ROTS[arrFrame, 1] * facingSign);
			LowerArmR.Height = LowerArmR.SizeY * 4 / 10;

			LowerArmL.LimbRotate(RUN_ROTS[arrFrame, 2] * facingSign);
			LowerArmR.LimbRotate(RUN_ROTS[arrFrame, 3] * facingSign);

			HandL.LimbRotate(FacingRight ? 0 : 1);
			HandR.LimbRotate(FacingRight ? 0 : 1);

			// Leg
			UpperLegL.X += legOffsetX;
			UpperLegL.Z = 1;
			UpperLegL.LimbRotate(RUN_ROTS[arrFrame, 4] * facingSign);

			UpperLegR.X -= legOffsetX;
			UpperLegR.Z = 1;
			UpperLegR.LimbRotate(RUN_ROTS[arrFrame, 5] * facingSign);

			LowerLegL.LimbRotate(RUN_ROTS[arrFrame, 6] * facingSign);
			LowerLegL.Z = 2;

			LowerLegR.LimbRotate(RUN_ROTS[arrFrame, 7] * facingSign);
			LowerLegR.Z = 2;

			FootL.LimbRotate(-facingSign);
			FootL.Z = 3;

			FootR.LimbRotate(-facingSign);
			FootR.Z = 3;

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void JumpUp (Character character) {

			bool FacingRight = character.FacingRight;
			int facingSign = FacingRight ? 1 : -1;
			var Head = character.Head;
			var Body = character.Body;
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

			bool alt = character.CurrentAnimationFrame.UMod(8) >= 4;

			character.PoseRootY += A2G;
			character.PoseTwist = FacingRight ? -400 : 400;

			if (alt) {
				Body.Height += A2G / 4;
				Head.Y += A2G / 4;
				UpperArmL.Y += A2G / 4;
				UpperArmR.Y += A2G / 4;
			}
			Head.Height += A2G;

			// Arm
			UpperArmL.LimbRotate(alt ? 65 : 55);
			UpperArmR.LimbRotate(alt ? -65 : -55);

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
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void JumpDown (Character character) {

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			int facingSign = FacingRight ? 1 : -1;
			var Head = character.Head;
			var Body = character.Body;
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

			bool alt = character.CurrentAnimationFrame.UMod(8) >= 4;

			character.PoseRootY -= A2G;
			character.PoseTwist = FacingRight ? -400 : 400;

			if (alt) {
				Body.Height += A2G / 4;
				Head.Y += A2G / 4;
				UpperArmL.Y += A2G / 4;
				UpperArmR.Y += A2G / 4;
			}
			Head.Height -= A2G;

			// Arm
			UpperArmL.LimbRotate(alt ? 135 : 125);
			UpperArmR.LimbRotate(alt ? -125 : -135);

			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmL.LimbRotate(alt ? 35 : 45);

			LowerArmR.Z = LowerArmR.Z.Abs();
			LowerArmR.LimbRotate(alt ? -45 : -35);

			HandL.Z = (FacingFront ? 34 : -34);
			HandL.LimbRotate(0);

			HandR.Z = (FacingFront ? 34 : -34);
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
			character.HandGrabRotationL = character.LowerArmL.Rotation - facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation - facingSign * 90;
		}


		public static void SwimIdle (Character character) {

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
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

			int frame0121 = character.CurrentAnimationFrame.UMod(64) / 16;
			int facingSign = FacingRight ? 1 : -1;

			// Arm
			if (frame0121 == 3) frame0121 = 1;
			int easeArm = frame0121 * A2G / 2;

			UpperArmL.LimbRotate(facingSign * -90);
			UpperArmL.Height = FacingRight ?
				UpperArmL.SizeY + 2 * A2G - easeArm :
				UpperArmL.SizeY / 2 + easeArm;

			UpperArmR.LimbRotate(facingSign * -90);
			UpperArmR.Height = FacingRight ?
				UpperArmR.SizeY / 2 + easeArm :
				UpperArmR.SizeY + 2 * A2G - easeArm;

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			HandL.LimbRotate(-facingSign);
			HandL.Z = (FacingFront ? 33 : -33);
			HandL.Height = HandL.SizeY - frame0121 * A2G / 3;

			HandR.LimbRotate(-facingSign);
			HandR.Z = (FacingFront ? 33 : -33);
			HandR.Height = HandR.SizeY - frame0121 * A2G / 3;

			// Leg
			UpperLegL.LimbRotate((frame0121 - 1) * 10);
			UpperLegR.LimbRotate((frame0121 - 1) * -10);

			LowerLegL.LimbRotate((frame0121 - 1) * -10);
			LowerLegR.LimbRotate((frame0121 - 1) * 10);
			FootL.LimbRotate(-facingSign);
			FootR.LimbRotate(-facingSign);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void SwimMove (Character character) {

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

			int loop = Mathf.Max((1200 / character.SwimSpeed.Value).Clamp(1, 128) / 4 * 4, 1);
			int frame = character.CurrentAnimationFrame.UMod(loop) / (loop / 4);

			int frame0121 = frame == 3 ? 1 : frame;
			int facingSign = FacingRight ? 1 : -1;
			int easeArm = frame * A2G;

			character.PoseTwist = FacingRight ? 0 : 1000;

			Head.X = facingSign * A2G;

			int bodyOffsetX = facingSign * 2 * A2G;
			Body.X += bodyOffsetX;
			ShoulderL.X += bodyOffsetX;
			ShoulderR.X += bodyOffsetX;
			UpperArmL.X += bodyOffsetX / 2;
			UpperArmR.X += bodyOffsetX / 2;
			UpperLegL.X += bodyOffsetX;
			UpperLegR.X += bodyOffsetX;

			// Arm
			if (FacingRight) {
				UpperArmL.LimbRotate(facingSign * -90);
				UpperArmL.Height += 2 * A2G - easeArm;

				LowerArmL.LimbRotate(0);

				HandL.LimbRotate(-facingSign);
				HandL.Height -= frame0121 * A2G / 3;

				UpperArmR.Imitate(UpperArmL);
				LowerArmR.Imitate(LowerArmL);
				HandR.Imitate(HandL);

			} else {
				UpperArmR.LimbRotate(facingSign * -90);
				UpperArmR.Height += 2 * A2G - easeArm;

				LowerArmR.LimbRotate(0);

				HandR.LimbRotate(-facingSign);
				HandR.Height -= frame0121 * A2G / 3;

				UpperArmL.Imitate(UpperArmR);
				LowerArmL.Imitate(LowerArmR);
				HandL.Imitate(HandR);
			}

			// Leg
			UpperLegL.Z += facingSign * 2;
			UpperLegL.LimbRotate(facingSign * (25 + facingSign * frame0121 * 15));
			UpperLegL.Height = UpperLegL.SizeY + A2G - facingSign * frame0121 * A2G / 2;

			UpperLegR.Z -= facingSign * 2;
			UpperLegR.LimbRotate(facingSign * (25 - facingSign * frame0121 * 15));
			UpperLegR.Height = UpperLegR.SizeY + A2G + facingSign * frame0121 * A2G / 2;

			LowerLegL.Z += facingSign * 2;
			LowerLegL.LimbRotate(facingSign * 85 - UpperLegL.Rotation);

			LowerLegR.Z -= facingSign * 2;
			LowerLegR.LimbRotate(facingSign * 85 - UpperLegR.Rotation);

			FootL.Z += facingSign * 2;
			FootL.LimbRotate(-facingSign);

			FootR.Z -= facingSign * 2;
			FootR.LimbRotate(-facingSign);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void SquatIdle (Character character) {

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

			int arrFrame = character.CurrentAnimationFrame.UMod(64) / 16;

			float ease = Ease.InOutCirc(arrFrame / 4f);
			if (arrFrame >= 2) ease = 1f - ease;
			int oneEase = (int)(ease * A2G);
			int halfEase = (int)(ease * A2G / 2);
			int facingSign = FacingRight ? 1 : -1;
			int bodyBorderL = FacingRight ? Body.Border.Left : Body.Border.Right;
			int bodyBorderR = FacingRight ? Body.Border.Right : Body.Border.Left;

			int above = character.PoseRootY /= 2;

			Body.Width += facingSign * halfEase;
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

			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.Down;
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmL.LimbRotate(25);

			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.Down;
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
			character.HandGrabRotationL = character.LowerArmL.Rotation + 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation - 90;
		}


		public static void SquatMove (Character character) {

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			int facingSign = FacingRight ? 1 : -1;
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

			int loop = Mathf.Max((600 / character.SquatSpeed.Value.Clamp(1, 256)) / 8 * 8, 1);
			int frameRate = loop / 8;
			int arrFrame = (character.CurrentAnimationFrame.UMod(loop) / frameRate) % 8;

			arrFrame = (arrFrame + 2).UMod(8);
			float ease = WALK_RUN_EASE[arrFrame];
			int easeA2G = (int)(ease * A2G);
			int easeA2G2 = (int)(ease * 2 * A2G);
			int above = character.PoseRootY = character.PoseRootY / 2 + easeA2G;

			Body.Height = Body.SizeY / 2 + easeA2G;
			Head.Y = Body.Y + Body.Height;
			Head.Height -= A2G;

			// Arm
			ShoulderL.Y = Body.Y + Body.Height;
			ShoulderR.Y = Body.Y + Body.Height;
			ShoulderL.Height /= 2;
			ShoulderR.Height /= 2;

			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.Down;
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmL.LimbRotate(arrFrame >= 3 && arrFrame <= 6 ? 45 : 25);

			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.Down;
			UpperArmR.Z = UpperArmR.Z.Abs();
			UpperArmR.LimbRotate(arrFrame >= 3 && arrFrame <= 6 ? -45 : -25);

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
			character.HandGrabRotationL = character.LowerArmL.Rotation + 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation - 90;
		}


		public static void Dash (Character character) {

			bool FacingRight = character.FacingRight;
			var Head = character.Head;
			var Body = character.Body;
			var Hip = character.Hip;
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

			int aFrame = character.CurrentAnimationFrame.UMod(8) / 4;
			bool alt = aFrame == 1;
			int facingSign = FacingRight ? 1 : -1;

			HandL.Z = 34;
			HandR.Z = 34;

			character.PoseRootY = 0;
			character.PoseTwist = -1000;

			Head.X += facingSign * A2G;
			Body.X += facingSign * 2 * A2G;
			Hip.X += facingSign * 2 * A2G;
			UpperLegL.X += facingSign * 2 * A2G + Body.Width / 4;
			UpperLegR.X += facingSign * 2 * A2G + Body.Width / 4;
			ShoulderL.X += facingSign * 2 * A2G;
			ShoulderR.X += facingSign * 2 * A2G;
			UpperArmL.X += facingSign * 2 * A2G;
			UpperArmR.X += facingSign * 2 * A2G;

			Hip.Y = UpperLegL.SizeX;
			Body.Y = Hip.Y + Hip.Height;
			Body.Height = Body.SizeY - A2G;
			Head.Y = Body.Y + Body.Height;
			Head.Height -= A2G;

			if (FacingRight) {
				LowerLegR.Z = 33;
				FootR.Z = 34;
			} else {
				LowerLegL.Z = 33;
				FootL.Z = 34;
			}

			// Arm
			ShoulderL.Y = Body.Y + Body.Height;
			ShoulderR.Y = Body.Y + Body.Height;

			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.Down;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.Down;

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

			FootL.LimbRotate(-facingSign);
			FootR.LimbRotate(-facingSign);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void Rolling (Character character) {

			bool FacingRight = character.FacingRight;
			var Head = character.Head;
			var Body = character.Body;
			var Hip = character.Hip;
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

			int arrFrame = character.CurrentAnimationFrame.UMod(24) / 3;
			int facingSign = FacingRight ? 1 : -1;
			int upsideSign = arrFrame < 2 || arrFrame > 5 ? 1 : -1;

			character.PoseRootY = character.PoseRootY * ROLLING[arrFrame, 0] / 1500;

			Head.FrontSide = Body.FrontSide = arrFrame < 2 || arrFrame > 5;

			Body.Width = Hip.Width = Body.Width * 2 / 3;
			Head.Height = Head.SizeY * ROLLING[arrFrame, 3] / 1000;
			Body.Height = Body.SizeY * ROLLING[arrFrame, 4] / 1500;

			Head.X = facingSign * A2G * ROLLING[arrFrame, 1] / 250;
			Body.X = facingSign * A2G * ROLLING[arrFrame, 2] / 300;
			Head.Y = upsideSign > 0 ? 0 : Head.SizeY;
			Hip.Y = Head.Y - Body.Height;
			Body.Y = Hip.Y + Hip.Height;

			// Arm
			ShoulderL.X = Body.X - Body.Width / 2 + Body.Border.Left;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.Up;
			ShoulderL.Height /= 2;

			ShoulderR.X = Body.X + Body.Width / 2 - Body.Border.Right;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.Up;
			ShoulderR.Height /= 2;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.Down;
			UpperArmL.Z = upsideSign * UpperArmL.Z.Abs();
			UpperArmL.LimbRotate(facingSign * ROLLING[arrFrame, 5]);
			UpperArmL.Height /= 2;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.Down;
			UpperArmR.Z = upsideSign * UpperArmR.Z.Abs();
			UpperArmR.LimbRotate(facingSign * ROLLING[arrFrame, 6]);
			UpperArmR.Height /= 2;

			LowerArmL.LimbRotate(0);
			LowerArmL.Z = upsideSign * LowerArmL.Z.Abs();
			LowerArmL.Height /= 2;

			LowerArmR.LimbRotate(0);
			LowerArmR.Z = upsideSign * LowerArmR.Z.Abs();
			LowerArmR.Height /= 2;

			HandL.LimbRotate(facingSign * upsideSign * 1);
			HandL.Z = upsideSign * 33;

			HandR.LimbRotate(facingSign * upsideSign * 1);
			HandR.Z = upsideSign * 33;

			// Leg
			UpperLegL.X = Body.X - Body.Width.Abs() / 2 + Body.Border.Left - facingSign * A2G * 2;
			UpperLegL.Y = Body.Y;
			UpperLegL.LimbRotate(facingSign * ROLLING[arrFrame, 7], 0);

			UpperLegR.X = Body.X + Body.Width.Abs() / 2 - Body.Border.Right - facingSign * A2G * 2;
			UpperLegR.Y = Body.Y;
			UpperLegR.LimbRotate(facingSign * ROLLING[arrFrame, 8], 0);

			LowerLegL.LimbRotate(facingSign * ROLLING[arrFrame, 9], 0);
			LowerLegR.LimbRotate(facingSign * ROLLING[arrFrame, 10], 0);

			FootL.LimbRotate(facingSign * upsideSign * -1);
			FootR.LimbRotate(facingSign * upsideSign * -1);

			if (upsideSign < 0) {
				UpperLegL.PivotX = 1000 - UpperLegL.PivotX;
				UpperLegR.PivotX = 1000 - UpperLegR.PivotX;
			}

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + Body.Height.Sign() * facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + Body.Height.Sign() * facingSign * 90;
		}


		public static void Rush (Character character) {

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			var Head = character.Head;
			var Body = character.Body;
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

			int aFrame = character.CurrentAnimationFrame.Abs() / 3;
			bool alt = aFrame % 2 == 0;
			int facingSign = FacingRight ? 1 : -1;

			character.PoseRootY -= aFrame.Clamp(0, 2) * A2G - A2G * 2;
			character.PoseTwist = 1000;
			Head.Height -= A2G;
			Body.X -= facingSign * A2G / 2;

			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = (FacingFront ? 1 : -1) * HandL.Z.Abs();
			HandR.Z = (FacingFront ? 1 : -1) * HandR.Z.Abs();
			UpperLegL.Z += facingSign;
			UpperLegR.Z -= facingSign;
			LowerLegL.Z += facingSign;
			LowerLegR.Z -= facingSign;
			FootL.Z += facingSign;
			FootR.Z -= facingSign;
			UpperLegL.X -= facingSign * A2G + (alt ? 3 : 0);
			UpperLegR.X -= facingSign * A2G + (alt ? 3 : 0);

			if (aFrame == 0) {
				// 0
				UpperArmL.LimbRotate((FacingRight ? 40 : 0));
				UpperArmR.LimbRotate((FacingRight ? 0 : -40));

				LowerArmL.LimbRotate(FacingRight ? 0 : -40);
				LowerArmR.LimbRotate(FacingRight ? 40 : 0);

				UpperLegL.LimbRotate(FacingRight ? 5 : 9);
				UpperLegR.LimbRotate(FacingRight ? -9 : -5);

				LowerLegL.LimbRotate(facingSign * (FacingRight ? 25 : 35) - UpperLegL.Rotation);
				LowerLegR.LimbRotate(facingSign * (FacingRight ? 35 : 25) - UpperLegR.Rotation);

			} else if (aFrame == 1) {
				// 1
				UpperArmL.LimbRotate((FacingRight ? 70 : 0));
				UpperArmR.LimbRotate((FacingRight ? 0 : -70));

				LowerArmL.LimbRotate(FacingRight ? 0 : -70);
				LowerArmR.LimbRotate(FacingRight ? 70 : 0);

				UpperLegL.LimbRotate(FacingRight ? 15 : 25);
				UpperLegR.LimbRotate(FacingRight ? -25 : -15);

				LowerLegL.LimbRotate(facingSign * (FacingRight ? 45 : 65) - UpperLegL.Rotation);
				LowerLegR.LimbRotate(facingSign * (FacingRight ? 65 : 45) - UpperLegR.Rotation);

			} else {
				// 2..
				UpperArmL.LimbRotate((FacingRight ? 90 : 0) + (alt ? 3 : 0));
				UpperArmR.LimbRotate((FacingRight ? 0 : -90) + (alt ? -3 : 0));

				LowerArmL.LimbRotate((FacingRight ? 0 : -90) + (alt ? 2 : 0));
				LowerArmR.LimbRotate((FacingRight ? 90 : 0) + (alt ? -2 : 0));

				UpperLegL.LimbRotate((FacingRight ? 20 : 30) + (alt ? 3 : 0));
				UpperLegR.LimbRotate((FacingRight ? -30 : -20) + (alt ? -2 : 0));

				LowerLegL.LimbRotate((facingSign * (FacingRight ? 65 : 85) - UpperLegL.Rotation) + (alt ? 2 : 0));
				LowerLegR.LimbRotate((facingSign * (FacingRight ? 85 : 65) - UpperLegR.Rotation) + (alt ? -3 : 0));
			}

			HandL.LimbRotate(facingSign);
			HandR.LimbRotate(facingSign);
			FootL.LimbRotate(-facingSign);
			FootR.LimbRotate(-facingSign);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void Pound (Character character) {

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			var Head = character.Head;
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

			bool alt = character.CurrentAnimationFrame % 8 < 4;
			int facingSign = FacingRight ? 1 : -1;

			HandL.Z = (FacingFront ? 34 : -34);
			HandR.Z = (FacingFront ? 34 : -34);
			UpperLegL.Z = -3;
			UpperLegR.Z = -3;
			LowerLegL.Z = FacingFront ? 4 : -4;
			LowerLegR.Z = FacingFront ? 4 : -4;
			FootL.Z = FacingFront ? 5 : -5;
			FootR.Z = FacingFront ? 5 : -5;

			character.PoseRootY = A2G;

			Head.X += facingSign * A2G;
			Head.Y -= A2G;

			// Arm
			UpperArmL.LimbRotate(135 + (alt ? 15 : 0), 500);
			UpperArmR.LimbRotate(-135 + (alt ? 15 : 0), 500);

			LowerArmL.LimbRotate(-60 + (alt ? -15 : 0), 500);
			LowerArmR.LimbRotate(60 + (alt ? -15 : 0), 500);

			HandL.LimbRotate(facingSign);
			HandR.LimbRotate(facingSign);

			// Leg
			UpperLegL.LimbRotate((FacingRight ? -125 : 155) + (alt ? 10 : 0), FacingRight ? 1000 : 500);
			UpperLegR.LimbRotate((FacingRight ? -155 : 125) + (alt ? 10 : 0), FacingRight ? 500 : 1000);

			LowerLegL.LimbRotate((FacingRight ? 100 : -120) + (alt ? -5 : 0));
			LowerLegR.LimbRotate((FacingRight ? 120 : -100) + (alt ? -5 : 0));

			FootL.LimbRotate(-facingSign);
			FootR.LimbRotate(-facingSign);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation - 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + 90;
		}


		public static void Spin (Character character) {

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

			int aFrame = character.CurrentAnimationFrame.UMod(8);
			int pingpong = aFrame < 4 ? aFrame : 8 - aFrame; // 01234321
			int pingpong2 = pingpong < 2 ? pingpong : 4 - pingpong; // 01210121
			int facingSign = FacingRight ? 1 : -1;
			bool facingFront = aFrame < 4;

			if (!facingFront) {
				FootL.Z -= 2;
				FootR.Z -= 2;
			}

			character.PoseTwist = (pingpong - 2) * 1000;

			Head.FrontSide = facingFront;
			Body.FrontSide = facingFront;

			Body.Width = facingSign * (facingFront ? 1 : -1) * (Body.SizeX - pingpong2 * A2G);
			Body.Height += A2G;

			Head.Width = facingSign * (facingFront ? 1 : -1) * (Head.SizeX - pingpong2 * A2G);
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

			HandL.LimbRotate(facingSign);
			HandR.LimbRotate(facingSign);
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

			FootL.LimbRotate(facingFront ? -facingSign : facingSign);
			FootR.LimbRotate(facingFront ? -facingSign : facingSign);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation - 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + 90;
		}


		public static void Climb (Character character) {

			bool FacingFront = character.FacingFront;
			int facingSign = character.FacingRight ? 1 : -1;
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

			int frameRate = Mathf.Max(560 / character.ClimbSpeedY.Value.Clamp(1, 1024) / 8, 1);
			int aFrame = character.CurrentAnimationFrame.UMod(frameRate * 10 - 1) / frameRate;

			int delayFrame = (aFrame + 1) % 10;
			if (aFrame >= 5) aFrame = 8 - aFrame;
			if (delayFrame >= 5) delayFrame = 8 - delayFrame;

			character.PoseRootY -= (aFrame - 2).Abs() * A2G;

			// Arm
			UpperArmL.LimbRotate(((3 - delayFrame) * -35 + 135).Clamp(45, 135), 1000);
			UpperArmR.LimbRotate((delayFrame * 35 - 135).Clamp(-135, -45), 1000);

			LowerArmL.LimbRotate(180 - UpperArmL.Rotation, 1000);
			LowerArmR.LimbRotate(-180 - UpperArmR.Rotation, 1000);

			HandL.LimbRotate(1);
			HandR.LimbRotate(0);
			HandL.Z = (FacingFront ? 34 : -34);
			HandR.Z = (FacingFront ? 34 : -34);

			// Leg
			UpperLegL.LimbRotate((aFrame * 35).Clamp(0, 60), 500);
			UpperLegR.LimbRotate(((3 - aFrame) * -35).Clamp(-60, 0), 500);

			LowerLegL.LimbRotate(-UpperLegL.Rotation - 5, 1000);
			LowerLegR.LimbRotate(-UpperLegR.Rotation + 5, 1000);

			FootL.LimbRotate(-1);
			FootR.LimbRotate(1);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation - 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + 90;
		}


		public static void Fly (Character character) {

			bool FacingRight = character.FacingRight;
			var Head = character.Head;
			var Body = character.Body;
			var Hip = character.Hip;
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

			int frame = character.CurrentAnimationFrame.UMod(16) / 2;
			int pingpong = frame < 4 ? frame : 8 - frame;
			int facingSign = FacingRight ? 1 : -1;

			character.PoseRootY = (frame < 6 ? frame / 2 : 8 - frame) * -A2G + 2 * A2G;

			Head.Y = 0;

			Body.Height = A2G * 2;
			Hip.Y = Head.Y - Body.Height;
			Body.Y = Hip.Y + Hip.Height;

			// Arm
			ShoulderL.Y = Body.Y + Body.Height;
			ShoulderR.Y = Body.Y + Body.Height;

			UpperArmL.X -= pingpong * A2G / 4;
			UpperArmL.Y = Head.Y;
			UpperArmL.LimbRotate(0);
			UpperArmL.Height -= pingpong * A2G / 6;

			UpperArmR.X += pingpong * A2G / 4;
			UpperArmR.Y = Head.Y;
			UpperArmR.LimbRotate(0);
			UpperArmR.Height -= pingpong * A2G / 6;

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			HandL.LimbRotate(0);
			HandR.LimbRotate(1);

			// Leg
			UpperLegL.X -= Body.Width / 2 + facingSign * A2G;
			UpperLegR.X -= Body.Width / 2 + facingSign * A2G;
			UpperLegL.Y = Head.Y + A2G * 5;
			UpperLegR.Y = Head.Y + A2G * 5;
			UpperLegL.Z = -34;
			UpperLegR.Z = -34;
			UpperLegL.LimbRotate(20);
			UpperLegR.LimbRotate(-20);

			LowerLegL.LimbRotate(pingpong * 4 - 20);
			LowerLegR.LimbRotate(-pingpong * 4 + 20);
			LowerLegL.Z = -33;
			LowerLegR.Z = -33;

			FootL.LimbRotate(-facingSign);
			FootR.LimbRotate(-facingSign);
			FootL.Z = -32;
			FootR.Z = -32;

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void Slide (Character character) {

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			var Head = character.Head;
			var Body = character.Body;
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

			bool alt = (character.CurrentAnimationFrame / 4) % 2 == 0;
			int facingSign = FacingRight ? 1 : -1;

			Head.X -= facingSign * 2 * A2G;
			Head.Width *= -1;

			Body.X -= facingSign * A2G;



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
			HandL.Z = (FacingFront ? 34 : -34);
			HandR.Z = (FacingFront ? 34 : -34);

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

			HandL.LimbRotate(-facingSign);
			HandR.LimbRotate(-facingSign);

			// Leg
			UpperLegL.LimbRotate((FacingRight ? -15 : 30) + (alt ? 1 : 0), FacingRight ? 1000 : 500);
			UpperLegR.LimbRotate((FacingRight ? -30 : 15) + (alt ? 0 : 1), FacingRight ? 500 : 1000);

			LowerLegL.LimbRotate(FacingRight ? -30 : -20, FacingRight ? 1000 : 300);
			LowerLegR.LimbRotate(FacingRight ? 20 : 30, FacingRight ? 300 : 1000);

			FootL.LimbRotate(-facingSign);
			FootR.LimbRotate(-facingSign);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;

		}


		public static void GrabTop (Character character) {

			bool FacingFront = character.FacingFront;
			int facingSign = character.FacingRight ? 1 : -1;
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

			int loop = Mathf.Max((700 / character.GrabMoveSpeedX.Value.Clamp(1, 1024)) / 4 * 4, 1);
			int arrFrame = ((character.CurrentAnimationFrame).UMod(loop) / (loop / 4)) % 4;// 0123
			int pingpong = arrFrame == 3 ? 1 : arrFrame; // 0121
			int pingpongAlt = arrFrame == 2 ? 1 : arrFrame == 3 ? 0 : arrFrame + 1; // 1210

			UpperArmL.Z = UpperArmR.Z = (FacingFront ? 34 : -34);
			LowerArmL.Z = LowerArmR.Z = (FacingFront ? 35 : -35);
			HandL.Z = HandR.Z = (FacingFront ? 36 : -36);


			character.PoseRootY += pingpongAlt * A2G;

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
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void GrabSide (Character character) {

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
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

			int loop = Mathf.Max((700 / character.GrabMoveSpeedY.Value.Clamp(1, 1024)) / 4 * 4, 1);
			int arrFrame = ((character.CurrentAnimationFrame).UMod(loop) / (loop / 4)) % 4;// 0123
			int pingpong = arrFrame == 3 ? 1 : arrFrame; // 0121
			int facingSign = FacingRight ? 1 : -1;
			int bodyShift = facingSign * (Body.Width.Abs() / 2 - A2G * 2);

			character.PoseRootY -= pingpong * A2G;

			Body.X += bodyShift;

			HandL.Z = (FacingFront ? 34 : -34);
			HandR.Z = (FacingFront ? 34 : -34);

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
			UpperArmL.LimbRotate(facingSign * (-77 + (pingpong - 1) * -12), 700);
			UpperArmR.LimbRotate(facingSign * (-77 + (pingpong - 1) * 12), 700);

			LowerArmL.LimbRotate(facingSign * (pingpong * -28 - 70) - UpperArmL.Rotation, 700);
			LowerArmR.LimbRotate(facingSign * ((2 - pingpong) * -28 - 70) - UpperArmR.Rotation, 700);

			HandL.LimbRotate(-facingSign);
			HandR.LimbRotate(-facingSign);

			// Leg
			UpperLegL.X = Body.X - A2G - Body.Width / 6;
			UpperLegR.X = Body.X + A2G - Body.Width / 6;

			UpperLegL.LimbRotate(facingSign * (-71 + (pingpong - 1) * 18));
			UpperLegR.LimbRotate(facingSign * (-71 + (pingpong - 1) * -18));

			LowerLegL.LimbRotate(-UpperLegL.Rotation - (pingpong * 5));
			LowerLegR.LimbRotate(-UpperLegR.Rotation - (pingpong * 5));

			FootL.LimbRotate(-facingSign);
			FootR.LimbRotate(-facingSign);

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


	}
}
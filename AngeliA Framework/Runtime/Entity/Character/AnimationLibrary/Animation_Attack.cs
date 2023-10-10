using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static partial class AnimationLibrary {


		public static void Attack_Punch (Character character) {

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

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		public static void Attack_SmashDown (Character character) {

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

			// Final
			character.HandGrabRotationL = character.LowerArmL.Rotation + facingSign * 90;
			character.HandGrabRotationR = character.LowerArmR.Rotation + facingSign * 90;
		}


		// Wave
		public static void Attack_Wave (Character character) {

			int localFrame = Game.GlobalFrame - character.LastAttackFrame;

			// Content
			if (localFrame < character.AttackDuration) {
				switch (character.EquippingWeaponHeld) {
					case WeaponHandHeld.SingleHanded:
						Attack_Wave_SingleHanded(character, localFrame);
						break;
					case WeaponHandHeld.DoubleHanded:
						Attack_Wave_DoubleHanded(character, localFrame);
						break;
					case WeaponHandHeld.OneOnEachHand:
						Attack_Wave_EachHand(character, localFrame);
						break;
					case WeaponHandHeld.Polearm:
						Attack_Wave_Polearm(character, localFrame);
						break;
				}
			}

			// Leg
			if (character.AnimatedPoseType == CharacterPoseAnimationType.Idle) {
				if (localFrame < character.AttackDuration / 4) {
					character.UpperLegL.X -= 2 * A2G;
					character.UpperLegR.X += 2 * A2G;
					character.LowerLegL.X -= 2 * A2G;
					character.LowerLegR.X += 2 * A2G;
					character.FootL.X -= 2 * A2G;
					character.FootR.X += 2 * A2G;
				} else if (character.FacingRight) {
					character.LowerLegL.X -= 2 * A2G;
					character.FootL.X -= 2 * A2G;
				} else {
					character.LowerLegR.X += 2 * A2G;
					character.FootR.X += 2 * A2G;
				}
			}
		}


		private static void Attack_Wave_SingleHanded (Character character, int localFrame) {

			float quad01 = Ease.OutBack((float)localFrame / character.AttackDuration);

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			var Head = character.Head;
			var Body = character.Body;
			var ShoulderL = character.ShoulderL;
			var UpperArmL = character.UpperArmL;
			var LowerArmL = character.LowerArmL;
			var HandL = character.HandL;
			var ShoulderR = character.ShoulderR;
			var UpperArmR = character.UpperArmR;
			var LowerArmR = character.LowerArmR;
			var HandR = character.HandR;

			int bodyBorderL = FacingRight ? Body.Border.left : Body.Border.right;
			int bodyBorderR = FacingRight ? Body.Border.right : Body.Border.left;

			bool idle = character.AnimatedPoseType == CharacterPoseAnimationType.Idle;
			bool squat =
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatMove;
			bool inAir =
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpDown ||
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpUp;

			int facingSign = FacingRight ? 1 : -1;
			int frontSign = FacingFront ? 1 : -1;

			UpperArmR.Z = frontSign * UpperArmR.Z.Abs();
			LowerArmR.Z = frontSign * LowerArmR.Z.Abs();
			HandR.Z = (FacingFront ? 34 : -34);

			// Head
			int headOffsetX = facingSign * ((9 - localFrame) / 3 * 2 * A2G - A2G * 5 / 2);
			int headOffsetY = A2G * 4 * ((9 - localFrame) / 3) - 5 * A2G;
			if (squat) {
				headOffsetX /= 2;
				headOffsetY /= 2;
			} else if (inAir) {
				headOffsetX /= 4;
				headOffsetY /= 4;
			} else if (!idle) {
				headOffsetX = headOffsetX * 2 / 3;
				headOffsetY = headOffsetY * 2 / 3;
			}
			Head.X -= headOffsetX;
			Head.Y += headOffsetY;

			// Body
			Body.Y -= localFrame * A2G / 18;
			character.Hip.Y -= localFrame * A2G / 18;
			Body.Height = Head.Y - Body.Y;

			// Left Side
			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;

			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;

			// Arm
			ShoulderR.X = Body.X + Body.SizeX / 2 - bodyBorderR;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderR.Height = Mathf.Min(ShoulderR.Height, Body.Height);
			ShoulderR.PivotX = 1000;

			UpperArmR.PivotX = 1000;

			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.PivotX = 0;

			if (idle) {
				UpperArmL.LimbRotate(localFrame * 3, 500);
				LowerArmL.LimbRotate(-localFrame * 2, 500);
			}

			// Upper Arm R
			UpperArmR.LimbRotate(facingSign * Mathf.LerpUnclamped(-185, -9, quad01).RoundToInt());

			UpperArmR.Height += A2G;

			LowerArmR.LimbRotate(0);
			LowerArmR.Height += A2G;

			HandL.LimbRotate(facingSign);
			HandR.LimbRotate(facingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Grab Rotation
			character.HandGrabRotationR = facingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			character.HandGrabScaleR = Mathf.LerpUnclamped(1100, 1600, quad01).RoundToInt();

		}


		private static void Attack_Wave_DoubleHanded (Character character, int localFrame) {

			float quad01 = Ease.OutBack((float)localFrame / character.AttackDuration);

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			var Head = character.Head;
			var Body = character.Body;
			var ShoulderL = character.ShoulderL;
			var UpperArmL = character.UpperArmL;
			var LowerArmL = character.LowerArmL;
			var HandL = character.HandL;
			var ShoulderR = character.ShoulderR;
			var UpperArmR = character.UpperArmR;
			var LowerArmR = character.LowerArmR;
			var HandR = character.HandR;

			int bodyBorderL = FacingRight ? Body.Border.left : Body.Border.right;
			int bodyBorderR = FacingRight ? Body.Border.right : Body.Border.left;

			bool idle = character.AnimatedPoseType == CharacterPoseAnimationType.Idle;
			bool squat =
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatMove;
			bool inAir =
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpDown ||
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpUp;

			int facingSign = FacingRight ? 1 : -1;
			int frontSign = FacingFront ? 1 : -1;

			UpperArmR.Z = frontSign * UpperArmR.Z.Abs();
			LowerArmR.Z = frontSign * LowerArmR.Z.Abs();
			HandR.Z = (FacingFront ? 34 : -34);

			// Head
			int headOffsetX = facingSign * ((9 - localFrame) / 3 * 2 * A2G - A2G * 5 / 2);
			int headOffsetY = A2G * 4 * ((9 - localFrame) / 3) - 5 * A2G;
			if (squat) {
				headOffsetX /= 2;
				headOffsetY /= 2;
			} else if (inAir) {
				headOffsetX /= 4;
				headOffsetY /= 4;
			} else if (!idle) {
				headOffsetX = headOffsetX * 2 / 3;
				headOffsetY = headOffsetY * 2 / 3;
			}
			Head.X -= headOffsetX;
			Head.Y += headOffsetY;

			// Body
			Body.Y -= localFrame * A2G / 18;
			character.Hip.Y -= localFrame * A2G / 18;
			Body.Height = Head.Y - Body.Y;

			// Arm L
			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderL.Height = Mathf.Min(ShoulderL.Height, Body.Height);

			// Arm R
			ShoulderR.X = Body.X + Body.SizeX / 2 - bodyBorderR;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderR.Height = Mathf.Min(ShoulderR.Height, Body.Height);
			ShoulderR.PivotX = 1000;

			// Upper Arm
			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.PivotX = 0;

			UpperArmL.LimbRotate(facingSign * Mathf.LerpUnclamped(-175, 0, quad01).RoundToInt());
			UpperArmR.LimbRotate(facingSign * Mathf.LerpUnclamped(-185, -9, quad01).RoundToInt());

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			// Grab Rotation
			character.HandGrabRotationL = facingSign * Mathf.LerpUnclamped(-70, 110, quad01).RoundToInt();
			character.HandGrabScaleL = Mathf.LerpUnclamped(1000, 1700, quad01).RoundToInt();
			character.HandGrabRotationR = facingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			character.HandGrabScaleR = Mathf.LerpUnclamped(1100, 1600, quad01).RoundToInt();

			// Upper Arm
			UpperArmL.Height += A2G;
			UpperArmR.Height += A2G;
			LowerArmL.Height += A2G;
			LowerArmR.Height += A2G;

			HandL.LimbRotate(facingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(facingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

		}


		private static void Attack_Wave_EachHand (Character character, int localFrame) {

			float quad01 = Ease.OutBack((float)localFrame / character.AttackDuration);

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			var Head = character.Head;
			var Body = character.Body;
			var ShoulderL = character.ShoulderL;
			var UpperArmL = character.UpperArmL;
			var LowerArmL = character.LowerArmL;
			var HandL = character.HandL;
			var ShoulderR = character.ShoulderR;
			var UpperArmR = character.UpperArmR;
			var LowerArmR = character.LowerArmR;
			var HandR = character.HandR;

			int bodyBorderL = FacingRight ? Body.Border.left : Body.Border.right;
			int bodyBorderR = FacingRight ? Body.Border.right : Body.Border.left;

			bool idle = character.AnimatedPoseType == CharacterPoseAnimationType.Idle;
			bool squat =
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatMove;
			bool inAir =
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpDown ||
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpUp;

			int facingSign = FacingRight ? 1 : -1;
			int frontSign = FacingFront ? 1 : -1;

			UpperArmR.Z = frontSign * UpperArmR.Z.Abs();
			LowerArmR.Z = frontSign * LowerArmR.Z.Abs();
			HandR.Z = (FacingFront ? 34 : -34);

			// Head
			int headOffsetX = facingSign * ((9 - localFrame) / 3 * 2 * A2G - A2G * 5 / 2);
			int headOffsetY = A2G * 4 * ((9 - localFrame) / 3) - 5 * A2G;
			if (squat) {
				headOffsetX /= 2;
				headOffsetY /= 2;
			} else if (inAir) {
				headOffsetX /= 4;
				headOffsetY /= 4;
			} else if (!idle) {
				headOffsetX = headOffsetX * 2 / 3;
				headOffsetY = headOffsetY * 2 / 3;
			}
			Head.X -= headOffsetX;
			Head.Y += headOffsetY;

			// Body
			Body.Y -= localFrame * A2G / 18;
			character.Hip.Y -= localFrame * A2G / 18;
			Body.Height = Head.Y - Body.Y;

			// Arm L
			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderL.Height = Mathf.Min(ShoulderL.Height, Body.Height);

			// Arm R
			ShoulderR.X = Body.X + Body.SizeX / 2 - bodyBorderR;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderR.Height = Mathf.Min(ShoulderR.Height, Body.Height);
			ShoulderR.PivotX = 1000;

			// Upper Arm
			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.PivotX = 0;

			UpperArmL.LimbRotate(facingSign * Mathf.LerpUnclamped(-175, 0, quad01).RoundToInt());
			UpperArmR.LimbRotate(facingSign * Mathf.LerpUnclamped(-185, -9, quad01).RoundToInt());

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			// Grab Rotation
			character.HandGrabRotationL = facingSign * Mathf.LerpUnclamped(-70, 110, quad01).RoundToInt();
			character.HandGrabScaleL = Mathf.LerpUnclamped(1000, 1700, quad01).RoundToInt();
			character.HandGrabRotationR = facingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			character.HandGrabScaleR = Mathf.LerpUnclamped(1100, 1600, quad01).RoundToInt();

			// Upper Arm
			UpperArmL.Height += A2G;
			UpperArmR.Height += A2G;
			LowerArmL.Height += A2G;
			LowerArmR.Height += A2G;

			HandL.LimbRotate(facingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(facingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

		}


		private static void Attack_Wave_Polearm (Character character, int localFrame) {

			float quad01 = Ease.OutBack((float)localFrame / character.AttackDuration);

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			var Head = character.Head;
			var Body = character.Body;
			var ShoulderL = character.ShoulderL;
			var UpperArmL = character.UpperArmL;
			var LowerArmL = character.LowerArmL;
			var HandL = character.HandL;
			var ShoulderR = character.ShoulderR;
			var UpperArmR = character.UpperArmR;
			var LowerArmR = character.LowerArmR;
			var HandR = character.HandR;

			int bodyBorderL = FacingRight ? Body.Border.left : Body.Border.right;
			int bodyBorderR = FacingRight ? Body.Border.right : Body.Border.left;

			bool idle = character.AnimatedPoseType == CharacterPoseAnimationType.Idle;
			bool squat =
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatMove;
			bool inAir =
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpDown ||
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpUp;

			int facingSign = FacingRight ? 1 : -1;
			int frontSign = FacingFront ? 1 : -1;

			UpperArmR.Z = frontSign * UpperArmR.Z.Abs();
			LowerArmR.Z = frontSign * LowerArmR.Z.Abs();
			HandR.Z = (FacingFront ? 34 : -34);

			// Head
			int headOffsetX = facingSign * ((9 - localFrame) / 3 * 2 * A2G - A2G * 5 / 2);
			int headOffsetY = A2G * 4 * ((9 - localFrame) / 3) - 5 * A2G;
			if (squat) {
				headOffsetX /= 2;
				headOffsetY /= 2;
			} else if (inAir) {
				headOffsetX /= 4;
				headOffsetY /= 4;
			} else if (!idle) {
				headOffsetX = headOffsetX * 2 / 3;
				headOffsetY = headOffsetY * 2 / 3;
			}
			Head.X -= headOffsetX;
			Head.Y += headOffsetY;

			// Body
			Body.Y -= localFrame * A2G / 18;
			character.Hip.Y -= localFrame * A2G / 18;
			Body.Height = Head.Y - Body.Y;

			// Arm L
			ShoulderL.X = Body.X - Body.Width.Abs() / 2 + bodyBorderL;
			ShoulderL.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderL.Height = Mathf.Min(ShoulderL.Height, Body.Height);

			// Arm R
			ShoulderR.X = Body.X + Body.SizeX / 2 - bodyBorderR;
			ShoulderR.Y = Body.Y + Body.Height - Body.Border.up;
			ShoulderR.Height = Mathf.Min(ShoulderR.Height, Body.Height);
			ShoulderR.PivotX = 1000;

			// Upper Arm
			UpperArmL.X = ShoulderL.X;
			UpperArmL.Y = ShoulderL.Y - ShoulderL.Height + ShoulderL.Border.down;
			UpperArmR.X = ShoulderR.X;
			UpperArmR.Y = ShoulderR.Y - ShoulderR.Height + ShoulderR.Border.down;
			UpperArmR.PivotX = 0;

			UpperArmL.LimbRotate(facingSign * Mathf.LerpUnclamped(-175, 0, quad01).RoundToInt());
			UpperArmR.LimbRotate(facingSign * Mathf.LerpUnclamped(-185, -9, quad01).RoundToInt());

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			// Grab Rotation
			character.HandGrabRotationL = facingSign * Mathf.LerpUnclamped(-70, 110, quad01).RoundToInt();
			character.HandGrabScaleL = Mathf.LerpUnclamped(1000, 1700, quad01).RoundToInt();
			character.HandGrabRotationR = facingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			character.HandGrabScaleR = Mathf.LerpUnclamped(1100, 1600, quad01).RoundToInt();

			// Upper Arm
			UpperArmL.Height += A2G;
			UpperArmR.Height += A2G;
			LowerArmL.Height += A2G;
			LowerArmR.Height += A2G;

			HandL.LimbRotate(facingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(facingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

		}


		// Poke
		public static void Attack_Poke (Character character) {
			// Double Handed

		}


		// Throw
		public static void Attack_Throw (Character character) {



		}


		// Magic
		public static void Attack_Magic (Character character) {
			switch (character.EquippingWeaponHeld) {
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


		// Bow
		public static void Attack_Bow (Character character) {



		}


		// Scratch
		public static void Attack_Scratch (Character character) {



		}


	}
}
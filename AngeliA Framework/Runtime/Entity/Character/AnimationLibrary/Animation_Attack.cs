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
			HandL.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);
			HandR.Z = (FacingFront ? POSE_Z_HAND : -POSE_Z_HAND);

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
			float easedFrame = quad01 * character.AttackDuration;

			bool FacingRight = character.FacingRight;
			var Head = character.Head;
			var Body = character.Body;
			var UpperArmL = character.UpperArmL;
			var LowerArmL = character.LowerArmL;
			var HandL = character.HandL;
			var UpperArmR = character.UpperArmR;
			var LowerArmR = character.LowerArmR;
			var HandR = character.HandR;

			bool idle = character.AnimatedPoseType == CharacterPoseAnimationType.Idle;
			bool squat =
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatMove;
			bool inAir =
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpDown ||
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpUp;

			int facingSign = FacingRight ? 1 : -1;

			// Head
			int headOffsetX = facingSign * ((9 - easedFrame) * A2G / 2 - A2G / 3).RoundToInt();
			int headOffsetY = ((9 - easedFrame) * A2G - 5 * A2G).RoundToInt();
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
			int bodyOffsetY = localFrame * A2G / 9 + A2G * 2;
			Body.Y -= bodyOffsetY;
			character.Hip.Y -= bodyOffsetY;
			Body.Height = Head.Y - Body.Y;

			ResetShoulderAndUpperArm(character);

			if (idle || squat) {
				UpperArmL.LimbRotate(-15 - facingSign * localFrame * 4, 500);
				LowerArmL.LimbRotate(-100 + facingSign * localFrame * 4);
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

			// Z
			UpperArmL.Z = LowerArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = UpperArmR.Z.Abs();
			HandR.Z = 34;

			// Grab Rotation
			character.HandGrabRotationL = character.HandGrabRotationR =
				facingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			character.HandGrabScaleL = character.HandGrabScaleR =
				facingSign * Mathf.LerpUnclamped(1100, 1400, quad01).RoundToInt();

		}


		private static void Attack_Wave_DoubleHanded (Character character, int localFrame) {

			float quad01 = Ease.OutBack((float)localFrame / character.AttackDuration);
			float easedFrame = quad01 * character.AttackDuration;

			bool FacingRight = character.FacingRight;
			var Head = character.Head;
			var Body = character.Body;
			var UpperArmL = character.UpperArmL;
			var LowerArmL = character.LowerArmL;
			var HandL = character.HandL;
			var UpperArmR = character.UpperArmR;
			var LowerArmR = character.LowerArmR;
			var HandR = character.HandR;

			bool idle = character.AnimatedPoseType == CharacterPoseAnimationType.Idle;
			bool squat =
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatIdle ||
				character.AnimatedPoseType == CharacterPoseAnimationType.SquatMove;
			bool inAir =
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpDown ||
				character.AnimatedPoseType == CharacterPoseAnimationType.JumpUp;

			int facingSign = FacingRight ? 1 : -1;
			int frontSign = character.FacingFront ? 1 : -1;

			// Head
			int headOffsetX = facingSign * ((9 - easedFrame) * A2G / 2 - A2G / 3).RoundToInt();
			int headOffsetY = ((9 - easedFrame) * A2G - 5 * A2G).RoundToInt();
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
			int bodyOffsetY = localFrame * A2G / 9 + A2G * 2;
			Body.Y -= bodyOffsetY;
			character.Hip.Y -= bodyOffsetY;
			Body.Height = Head.Y - Body.Y;

			ResetShoulderAndUpperArm(character);

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
			character.HandGrabRotationL = character.HandGrabRotationR =
				facingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			character.HandGrabScaleL = character.HandGrabScaleR =
				facingSign * Mathf.LerpUnclamped(1100, 1400, quad01).RoundToInt();

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

			// Z
			UpperArmL.Z = LowerArmL.Z = frontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = frontSign * UpperArmR.Z.Abs();
			HandL.Z = frontSign * POSE_Z_HAND;
			HandR.Z = frontSign * POSE_Z_HAND;

		}


		private static void Attack_Wave_EachHand (Character character, int localFrame) {

			float quad01 = Ease.OutBack((float)localFrame / character.AttackDuration);
			float easedFrame = quad01 * character.AttackDuration;

			bool FacingRight = character.FacingRight;
			bool FacingFront = character.FacingFront;
			var Head = character.Head;
			var Body = character.Body;
			var UpperArmL = character.UpperArmL;
			var LowerArmL = character.LowerArmL;
			var HandL = character.HandL;
			var UpperArmR = character.UpperArmR;
			var LowerArmR = character.LowerArmR;
			var HandR = character.HandR;

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
			int headOffsetX = facingSign * ((9 - easedFrame) * A2G / 2 - A2G / 3).RoundToInt();
			int headOffsetY = ((9 - easedFrame) * A2G - 5 * A2G).RoundToInt();
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
			int bodyOffsetY = localFrame * A2G / 9 + A2G * 2;
			Body.Y -= bodyOffsetY;
			character.Hip.Y -= bodyOffsetY;
			Body.Height = Head.Y - Body.Y;

			ResetShoulderAndUpperArm(character);

			UpperArmL.LimbRotate(facingSign * Mathf.LerpUnclamped(-175, 0, quad01).RoundToInt());
			UpperArmR.LimbRotate(facingSign * Mathf.LerpUnclamped(-185, -9, quad01).RoundToInt());

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);

			// Grab Rotation
			character.HandGrabRotationL = facingSign * Mathf.LerpUnclamped(-70, 110, quad01).RoundToInt();
			character.HandGrabScaleL = facingSign * Mathf.LerpUnclamped(1000, 1300, quad01).RoundToInt();
			character.HandGrabRotationR = facingSign * Mathf.LerpUnclamped(-80, 100, quad01).RoundToInt();
			character.HandGrabScaleR = facingSign * Mathf.LerpUnclamped(1100, 1400, quad01).RoundToInt();

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
			float easedFrame = quad01 * character.AttackDuration;

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

			int facingSign = FacingRight ? 1 : -1;
			int frontSign = FacingFront ? 1 : -1;

			ResetShoulderAndUpperArm(character);





			// Z
			UpperArmL.Z = LowerArmL.Z = frontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = frontSign * UpperArmR.Z.Abs();
			HandL.Z = frontSign * POSE_Z_HAND;
			HandR.Z = frontSign * POSE_Z_HAND;

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
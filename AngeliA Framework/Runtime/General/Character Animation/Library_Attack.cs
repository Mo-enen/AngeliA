using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {

	public class PoseAttack_Hand : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			character.AttackStyleLoop = 2;
			switch (character.AttackStyleIndex % character.AttackStyleLoop) {
				case 0:
					Punch();
					break;
				case 1:
					Smash();
					break;
			}
		}
		private static void Punch () {

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
			if (AnimationType == CharacterAnimationType.Idle) {
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
		private static void Smash () {

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
			if (AnimationType == CharacterAnimationType.Idle) {
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
	}

	public class PoseAttack_Wave : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			int style;
			var handheld = character.EquippingWeaponHeld;
			var weaponType = character.EquippingWeaponType;
			switch (handheld) {

				// Single Handed
				case WeaponHandheld.SingleHanded:
					character.AttackStyleLoop = 4;
					style =
						character.LastAttackCharged ||
						weaponType == WeaponType.Throwing ||
						weaponType == WeaponType.Flail ?
						0 : character.AttackStyleIndex % character.AttackStyleLoop;
					switch (style) {
						default:
							SingleHanded_SmashDown();
							break;
						case 1:
							SingleHanded_SmashUp();
							break;
						case 2:
							SingleHanded_SlashIn();
							break;
						case 3:
							SingleHanded_SlashOut();
							break;
					}
					break;

				// Double Handed
				case WeaponHandheld.DoubleHanded:
					character.AttackStyleLoop = 4;
					style =
						character.LastAttackCharged ||
						weaponType == WeaponType.Throwing ||
						weaponType == WeaponType.Flail ?
						0 : character.AttackStyleIndex % character.AttackStyleLoop;
					switch (style) {
						default:
							DoubleHanded_SmashDown();
							break;
						case 1:
							DoubleHanded_SmashUp();
							break;
						case 2:
							DoubleHanded_SlashIn();
							break;
						case 3:
							DoubleHanded_SlashOut();
							break;
					}
					break;

				// Each Hand
				case WeaponHandheld.OneOnEachHand:
					character.AttackStyleLoop = 4;
					style = character.LastAttackCharged ? 0 : character.AttackStyleIndex % character.AttackStyleLoop;
					switch (style) {
						default:
							EachHand_SmashDown();
							break;
						case 1:
							EachHand_SmashUp();
							break;
						case 2:
							EachHand_SlashIn();
							break;
						case 3:
							EachHand_SlashOut();
							break;
					}
					break;

				// Pole
				case WeaponHandheld.Pole:
					character.AttackStyleLoop = 4;
					style = character.LastAttackCharged || character.EquippingWeaponType == WeaponType.Flail ? 0 : character.AttackStyleIndex % character.AttackStyleLoop;
					switch (style) {
						default:
							Polearm_SmashDown();
							break;
						case 1:
							Polearm_SmashUp();
							break;
						case 2:
							Polearm_SlashIn();
							break;
						case 3:
							Polearm_SlashOut();
							break;
					}
					break;
			}
		}
		private static void SingleHanded_SmashDown () {

			bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
			bool isThrowing = Target.EquippingWeaponType == WeaponType.Throwing;
			float ease01 = isCharging ?
				1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
				Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			if (isCharging) {
				AttackHeadDown(ease01, 100, 800, 1000, 100);
				ResetShoulderAndUpperArm();
			} else {
				AttackHeadDown(ease01);
				ResetShoulderAndUpperArm();
				// Left Side
				if (
					AnimationType == CharacterAnimationType.Idle ||
					AnimationType == CharacterAnimationType.SquatIdle ||
					AnimationType == CharacterAnimationType.SquatMove
				) {
					UpperArmL.LimbRotate(-15 - (int)(ease01 * 48), 500);
					LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
				}
			}

			// Upper Arm R
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-185, -9, ease01));
			if (!isThrowing) UpperArmR.Height += A2G;
			LowerArmR.LimbRotate(0);
			if (!isThrowing) LowerArmR.Height += A2G;

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);
			if (!isThrowing) {
				HandR.Width += HandR.Width.Sign() * A2G;
				HandR.Height += HandR.Height.Sign() * A2G;
			}

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(-80, 100, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;
		}
		private static void SingleHanded_SmashUp () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			AttackHeadDown(1f - ease01 + 0.5f, -100, 500, 500);
			ResetShoulderAndUpperArm();

			// Left Side
			if (
				AnimationType == CharacterAnimationType.Idle ||
				AnimationType == CharacterAnimationType.SquatIdle ||
				AnimationType == CharacterAnimationType.SquatMove
			) {
				UpperArmL.LimbRotate(-15 - (int)(ease01 * 48), 500);
				LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
			}

			// Upper Arm R
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(60, -160, ease01));
			UpperArmR.Height += A2G;

			LowerArmR.LimbRotate(0);
			LowerArmR.Height += A2G;

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(197, 12, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(-1100, -1400, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;
		}
		private static void SingleHanded_SlashIn () {

			float frame01 = (float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration;
			float ease01 = Ease.OutBack(frame01);
			float ease010 = Mathf.PingPong(ease01 * 2f, 1f);

			AttackHeadDown(ease01, 1400, 500, 500);
			ResetShoulderAndUpperArm();

			Target.BodyTwist = -FacingSign * (int)(frame01 * 2000 - 1000);
			Target.HeadTwist = -FacingSign * (int)(frame01 * 500 - 250);

			// Left Side
			if (
				AnimationType == CharacterAnimationType.Idle ||
				AnimationType == CharacterAnimationType.SquatIdle ||
				AnimationType == CharacterAnimationType.SquatMove
			) {
				UpperArmL.LimbRotate(15 + (int)(ease01 * 48), 500);
				LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
			}

			// Upper Arm R
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-100, 65, ease01));
			UpperArmR.Height = (int)(UpperArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			LowerArmR.LimbRotate(0);
			LowerArmR.Height = (int)(LowerArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(67, 224, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1300, 100, ease010);
			Target.HandGrabAttackTwistL = Target.HandGrabAttackTwistR =
				(int)Mathf.LerpUnclamped(600, 200, frame01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			UpperArmR.Z = (frame01 < 0.5f ? -1 : 1) * UpperArmR.Z.Abs();
			LowerArmR.Z = (frame01 < 0.5f ? -1 : 1) * LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;
		}
		private static void SingleHanded_SlashOut () {

			float frame01 = (float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration;
			float ease01 = Ease.OutBack(frame01);
			float ease010 = Mathf.PingPong(ease01 * 2f, 1f);

			AttackHeadDown(ease01, -500, 500, 500, -1000);
			ResetShoulderAndUpperArm();

			Target.BodyTwist = FacingSign * (int)(frame01 * 2000 - 1000);
			Target.HeadTwist = FacingSign * (int)(frame01 * 500 - 250);

			// Left Side
			if (
				AnimationType == CharacterAnimationType.Idle ||
				AnimationType == CharacterAnimationType.SquatIdle ||
				AnimationType == CharacterAnimationType.SquatMove
			) {
				UpperArmL.LimbRotate(15 + (int)(ease01 * 48), 500);
				LowerArmL.LimbRotate(-100 + (int)(ease01 * 48));
			}

			// Upper Arm R
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(178, -50, ease01));
			UpperArmR.Height = (int)(UpperArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			LowerArmR.LimbRotate(0);
			LowerArmR.Height = (int)(LowerArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(197, 128, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(-1300, -100, ease010);
			Target.HandGrabAttackTwistL = Target.HandGrabAttackTwistR =
				(int)Mathf.LerpUnclamped(600, 200, frame01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			UpperArmR.Z = (frame01 < 0.5f ? -1 : 1) * UpperArmR.Z.Abs();
			LowerArmR.Z = (frame01 < 0.5f ? -1 : 1) * LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;
		}
		private static void DoubleHanded_SmashDown () {

			bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
			float ease01 = isCharging ?
				1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
				Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			if (isCharging) {
				AttackHeadDown(ease01, 100, 800, 1000, 100);
			} else {
				AttackHeadDown(ease01);
			}
			ResetShoulderAndUpperArm();

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

			// Leg
			AttackLegShake(ease01);

			// Grab Rotation
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(-37, 100, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = POSE_Z_HAND;
			HandR.Z = POSE_Z_HAND;
		}
		private static void DoubleHanded_SmashUp () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			AttackHeadDown(1f - ease01 + 0.5f, -200, 500, 500);
			ResetShoulderAndUpperArm();

			int upperRotA = (int)Mathf.LerpUnclamped(42, 180, ease01);
			int upperRotB = (int)Mathf.LerpUnclamped(29, 180, ease01);
			int lowerRotA = (int)Mathf.LerpUnclamped(28, 0, ease01);
			int lowerRotB = (int)Mathf.LerpUnclamped(14, -98, ease01);

			UpperArmL.LimbRotate(FacingRight ? -upperRotA : upperRotB);
			UpperArmR.LimbRotate(FacingRight ? -upperRotB : upperRotA);
			UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1000 : 862) / 1000;
			UpperArmR.Height = UpperArmR.Height * (FacingRight ? 862 : 1000) / 1000;

			LowerArmL.LimbRotate(FacingRight ? -lowerRotA : -lowerRotB);
			LowerArmR.LimbRotate(FacingRight ? lowerRotB : lowerRotA);
			LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1000 : 724) / 1000;
			LowerArmR.Height = LowerArmR.Height * (FacingRight ? 724 : 1000) / 1000;

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

			// Leg
			AttackLegShake(ease01);

			// Grab Rotation
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(100, -20, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = POSE_Z_HAND;
			HandR.Z = POSE_Z_HAND;
		}
		private static void DoubleHanded_SlashIn () {

			float frame01 = (float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration;
			float ease01 = Ease.OutBack(frame01);
			float ease010 = Mathf.PingPong(ease01 * 2f, 1f);

			AttackHeadDown(ease01, 1400, 500, 500);
			ResetShoulderAndUpperArm();

			Target.BodyTwist = -FacingSign * (int)(frame01 * 2000 - 1000);
			Target.HeadTwist = -FacingSign * (int)(frame01 * 500 - 250);

			// Upper Arm
			int upperRotA = (int)Mathf.LerpUnclamped(-100, 65, ease01);
			int upperRotB = (int)Mathf.LerpUnclamped(-47, 77, ease01);

			UpperArmL.LimbRotate(FacingRight ? upperRotA : -upperRotB);
			UpperArmR.LimbRotate(FacingRight ? upperRotB : -upperRotA);
			UpperArmL.Height = (int)(UpperArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			UpperArmR.Height = (int)(UpperArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);
			LowerArmL.Height = (int)(LowerArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			LowerArmR.Height = (int)(LowerArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			HandL.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Grab Rotation
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(71, 248, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1300, 100, ease010);
			Target.HandGrabAttackTwistL = Target.HandGrabAttackTwistR =
				(int)Mathf.LerpUnclamped(600, 200, frame01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = POSE_Z_HAND;
			HandR.Z = POSE_Z_HAND;
		}
		private static void DoubleHanded_SlashOut () {

			float frame01 = (float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration;
			float ease01 = Ease.OutBack(frame01);
			float ease010 = Mathf.PingPong(ease01 * 2f, 1f);

			AttackHeadDown(ease01, -500, 500, 500);
			ResetShoulderAndUpperArm();

			Target.BodyTwist = FacingSign * (int)(frame01 * 2000 - 1000);
			Target.HeadTwist = FacingSign * (int)(frame01 * 500 - 250);

			int upperRotA = (int)Mathf.LerpUnclamped(-171, 49, ease01);
			int upperRotB = (int)Mathf.LerpUnclamped(-100, 39, ease01);

			UpperArmL.LimbRotate(FacingRight ? -upperRotA : upperRotB);
			UpperArmR.LimbRotate(FacingRight ? -upperRotB : upperRotA);
			UpperArmL.Height = (int)(UpperArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			UpperArmR.Height = (int)(UpperArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);
			LowerArmL.Height = (int)(LowerArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			LowerArmR.Height = (int)(LowerArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			HandL.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Grab Rotation
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(297, 128, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1300, 100, ease010);
			Target.HandGrabAttackTwistL = Target.HandGrabAttackTwistR =
				(int)Mathf.LerpUnclamped(600, 200, frame01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = POSE_Z_HAND;
			HandR.Z = POSE_Z_HAND;
		}
		private static void EachHand_SmashDown () {

			bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
			float ease01 = isCharging ?
				1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
				Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			if (isCharging) {
				AttackHeadDown(ease01, 100, 800, 1000, 100);
			} else {
				AttackHeadDown(ease01);
			}
			ResetShoulderAndUpperArm();

			UpperArmL.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-175, 0, ease01));
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-185, -9, ease01));
			UpperArmL.Height += A2G;
			UpperArmR.Height += A2G;

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);
			LowerArmL.Height += A2G;
			LowerArmR.Height += A2G;

			HandL.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			if (isCharging) {
				HandL.Z = FacingSign * POSE_Z_HAND;
				HandR.Z = -FacingSign * POSE_Z_HAND;
			} else {
				HandL.Z = POSE_Z_HAND;
				HandR.Z = POSE_Z_HAND;
			}

			// Grab
			Target.HandGrabRotationL = FacingSign * (int)Mathf.LerpUnclamped(-70, 110, ease01);
			Target.HandGrabScaleL = FacingSign * (int)Mathf.LerpUnclamped(1000, 1300, ease01);
			Target.HandGrabRotationR = FacingSign * (int)Mathf.LerpUnclamped(-80, 100, ease01);
			Target.HandGrabScaleR = FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

		}
		private static void EachHand_SmashUp () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);
			float easeL = FacingRight ? ease01 : ease01 - 0.1f;
			float easeR = FacingRight ? ease01 - 0.1f : ease01;

			AttackHeadDown(1f - ease01 + 0.5f, -100, 500, 500);
			ResetShoulderAndUpperArm();

			// Upper Arm
			UpperArmL.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(60, -160, easeL));
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(60, -160, easeR));
			UpperArmL.Height += A2G;
			UpperArmR.Height += A2G;

			LowerArmL.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-50, 0, easeL));
			LowerArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-50, 0, easeR));
			LowerArmL.Height += A2G;
			LowerArmR.Height += A2G;

			HandL.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = FacingSign * (int)Mathf.LerpUnclamped(197, 12, easeL);
			Target.HandGrabRotationR = FacingSign * (int)Mathf.LerpUnclamped(197, 12, easeR);
			Target.HandGrabScaleL = Target.HandGrabScaleR = FacingSign * (int)Mathf.LerpUnclamped(1000, 1300, ease01);

		}
		private static void EachHand_SlashIn () {

			float frame01 = (float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration;
			float ease01 = Ease.OutBack(frame01);
			float ease010 = Mathf.PingPong(ease01 * 2f, 1f);

			AttackHeadDown(ease01, 1400, 500, 500);
			ResetShoulderAndUpperArm();

			Target.BodyTwist = -FacingSign * (int)(frame01 * 2000 - 1000);
			Target.HeadTwist = -FacingSign * (int)(frame01 * 500 - 250);

			// Upper Arm
			UpperArmL.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-110, 55, ease01));
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(-100, 65, ease01));
			UpperArmL.Height = (int)(UpperArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			UpperArmR.Height = (int)(UpperArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);
			LowerArmL.Height = (int)(LowerArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			LowerArmR.Height = (int)(LowerArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			HandL.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(67, 224, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1300, 100, ease010);
			Target.HandGrabAttackTwistL = Target.HandGrabAttackTwistR =
				(int)Mathf.LerpUnclamped(600, 200, frame01);
		}
		private static void EachHand_SlashOut () {

			float frame01 = (float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration;
			float ease01 = Ease.OutBack(frame01);
			float ease010 = Mathf.PingPong(ease01 * 2f, 1f);

			AttackHeadDown(ease01, -500, 500, 500);
			ResetShoulderAndUpperArm();

			Target.BodyTwist = FacingSign * (int)(frame01 * 2000 - 1000);
			Target.HeadTwist = FacingSign * (int)(frame01 * 500 - 250);

			// Upper Arm
			UpperArmL.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(168, -40, ease01));
			UpperArmR.LimbRotate(FacingSign * (int)Mathf.LerpUnclamped(178, -50, ease01));
			UpperArmL.Height = (int)(UpperArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			UpperArmR.Height = (int)(UpperArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);
			LowerArmL.Height = (int)(LowerArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			LowerArmR.Height = (int)(LowerArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			HandL.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			UpperArmR.Z = (frame01 < 0.5f ? -1 : 1) * UpperArmR.Z.Abs();
			LowerArmR.Z = (frame01 < 0.5f ? -1 : 1) * LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(197, 128, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(-1300, -100, ease010);
			Target.HandGrabAttackTwistL = Target.HandGrabAttackTwistR =
				(int)Mathf.LerpUnclamped(600, 200, frame01);

		}
		private static void Polearm_SmashDown () {

			bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
			float ease01 = isCharging ?
				1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
				Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			if (isCharging) {
				AttackHeadDown(ease01, 100, 800, 1000, 100);
			} else {
				AttackHeadDown(ease01);
			}
			ResetShoulderAndUpperArm();

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

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(-58, 107, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;

		}
		private static void Polearm_SmashUp () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			AttackHeadDown(1f - ease01 + 0.5f, -100, 500, 500);
			ResetShoulderAndUpperArm();

			// Upper Arm
			int uRotA = (int)Mathf.LerpUnclamped(63, -130, ease01);
			int uRotB = (int)Mathf.LerpUnclamped(43, -79, ease01);
			UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
			UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);

			// Lower Arm
			int lRotA = (int)Mathf.LerpUnclamped(-75, 0, ease01);
			int lRotB = (int)Mathf.LerpUnclamped(0, -98, ease01);
			LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
			LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);

			// Hand
			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(130, 10, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1000, 1300, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;

		}
		private static void Polearm_SlashIn () => DoubleHanded_SlashIn();
		private static void Polearm_SlashOut () => DoubleHanded_SlashOut();

	}

	public class PoseAttack_Polearm : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			character.AttackStyleLoop = 8;
			int style = character.LastAttackCharged ? 0 : character.AttackStyleIndex % character.AttackStyleLoop;
			if (character.LastAttackCharged) style = 4;
			switch (style) {
				default:
					Poke();
					break;
				case 4:
					SmashDown();
					break;
				case 5:
					SmashUp();
					break;
				case 6:
					SlashIn();
					break;
				case 7:
					SlashOut();
					break;
			}
		}
		public static void Poke () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			AttackHeadDown(ease01, 300, 300, 1000, 100);
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
			AttackLegShake(ease01);

			// Z
			UpperArmL.Z = LowerArmL.Z = FrontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = LowerArmR.Z = FrontSign * UpperArmR.Z.Abs();
			HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR = FacingSign * 90;
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1000, 1200, ease01);

		}
		private static void SmashDown () {

			bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
			float ease01 = isCharging ?
				1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
				Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			if (isCharging) {
				AttackHeadDown(ease01, 100, 800, 1000, 100);
			} else {
				AttackHeadDown(ease01);
			}
			ResetShoulderAndUpperArm();

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

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(-58, 107, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1100, 1400, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;

		}
		private static void SmashUp () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			AttackHeadDown(1f - ease01 + 0.5f, -100, 500, 500);
			ResetShoulderAndUpperArm();

			// Upper Arm
			int uRotA = (int)Mathf.LerpUnclamped(63, -130, ease01);
			int uRotB = (int)Mathf.LerpUnclamped(43, -79, ease01);
			UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
			UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);

			// Lower Arm
			int lRotA = (int)Mathf.LerpUnclamped(-75, 0, ease01);
			int lRotB = (int)Mathf.LerpUnclamped(0, -98, ease01);
			LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
			LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);

			// Hand
			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(130, 10, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1000, 1300, ease01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandR.Z = POSE_Z_HAND;

		}
		private static void SlashIn () {

			float frame01 = (float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration;
			float ease01 = Ease.OutBack(frame01);
			float ease010 = Mathf.PingPong(ease01 * 2f, 1f);

			AttackHeadDown(ease01, 1400, 500, 500);
			ResetShoulderAndUpperArm();

			Target.BodyTwist = -FacingSign * (int)(frame01 * 2000 - 1000);
			Target.HeadTwist = -FacingSign * (int)(frame01 * 500 - 250);

			// Upper Arm
			int upperRotA = (int)Mathf.LerpUnclamped(-100, 65, ease01);
			int upperRotB = (int)Mathf.LerpUnclamped(-47, 77, ease01);

			UpperArmL.LimbRotate(FacingRight ? upperRotA : -upperRotB);
			UpperArmR.LimbRotate(FacingRight ? upperRotB : -upperRotA);
			UpperArmL.Height = (int)(UpperArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			UpperArmR.Height = (int)(UpperArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);
			LowerArmL.Height = (int)(LowerArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			LowerArmR.Height = (int)(LowerArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			HandL.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Grab Rotation
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(71, 248, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1300, 100, ease010);
			Target.HandGrabAttackTwistL = Target.HandGrabAttackTwistR =
				(int)Mathf.LerpUnclamped(600, 200, frame01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = POSE_Z_HAND;
			HandR.Z = POSE_Z_HAND;
		}
		private static void SlashOut () {

			float frame01 = (float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration;
			float ease01 = Ease.OutBack(frame01);
			float ease010 = Mathf.PingPong(ease01 * 2f, 1f);

			AttackHeadDown(ease01, -500, 500, 500);
			ResetShoulderAndUpperArm();

			Target.BodyTwist = FacingSign * (int)(frame01 * 2000 - 1000);
			Target.HeadTwist = FacingSign * (int)(frame01 * 500 - 250);

			int upperRotA = (int)Mathf.LerpUnclamped(-171, 49, ease01);
			int upperRotB = (int)Mathf.LerpUnclamped(-100, 39, ease01);

			UpperArmL.LimbRotate(FacingRight ? -upperRotA : upperRotB);
			UpperArmR.LimbRotate(FacingRight ? -upperRotB : upperRotA);
			UpperArmL.Height = (int)(UpperArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			UpperArmR.Height = (int)(UpperArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			LowerArmL.LimbRotate(0);
			LowerArmR.LimbRotate(0);
			LowerArmL.Height = (int)(LowerArmL.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));
			LowerArmR.Height = (int)(LowerArmR.Height * Mathf.LerpUnclamped(1.2f, 0.1f, ease010));

			HandL.LimbRotate(FacingSign);
			HandL.Width += HandL.Width.Sign() * A2G;
			HandL.Height += HandL.Height.Sign() * A2G;

			HandR.LimbRotate(FacingSign);
			HandR.Width += HandR.Width.Sign() * A2G;
			HandR.Height += HandR.Height.Sign() * A2G;

			// Leg
			AttackLegShake(ease01);

			// Grab Rotation
			Target.HandGrabRotationL = Target.HandGrabRotationR =
				FacingSign * (int)Mathf.LerpUnclamped(297, 128, ease01);
			Target.HandGrabScaleL = Target.HandGrabScaleR =
				FacingSign * (int)Mathf.LerpUnclamped(1300, 100, ease010);
			Target.HandGrabAttackTwistL = Target.HandGrabAttackTwistR =
				(int)Mathf.LerpUnclamped(600, 200, frame01);

			// Z
			UpperArmL.Z = UpperArmL.Z.Abs();
			UpperArmR.Z = UpperArmR.Z.Abs();
			LowerArmL.Z = LowerArmL.Z.Abs();
			LowerArmR.Z = LowerArmR.Z.Abs();
			HandL.Z = POSE_Z_HAND;
			HandR.Z = POSE_Z_HAND;
		}
	}

	public class PoseAttack_Scratch : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			character.AttackStyleLoop = 3;
			int style = character.LastAttackCharged ? 2 : character.AttackStyleIndex % character.AttackStyleLoop;
			switch (style) {
				default:
					ScratchIn();
					break;
				case 1:
					ScratchOut();
					break;
				case 2:
					ScratchUp();
					break;
			}
		}
		private static void ScratchIn () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);
			AttackHeadDown(ease01, 1000, 500, 500);
			ResetShoulderAndUpperArm();

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
			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			AttackLegShake(ease01);

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
		private static void ScratchOut () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);
			AttackHeadDown(ease01, 1000, 500, 500);
			ResetShoulderAndUpperArm();

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

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			AttackLegShake(ease01);

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
		private static void ScratchUp () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);
			AttackHeadDown(1f - ease01 + 0.5f, -500, 500, 500);
			ResetShoulderAndUpperArm();

			// Arm
			int rotUA = (int)Mathf.LerpUnclamped(-82, 137, ease01);
			int rotUB = (int)Mathf.LerpUnclamped(-69, 142, ease01);
			UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
			UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
			UpperArmL.Height = UpperArmL.Height * (FacingRight ? 1200 : 500) / 1000;
			UpperArmR.Height = UpperArmR.Height * (FacingRight ? 500 : 1200) / 1000;

			int rotLA = (int)Mathf.LerpUnclamped(-36, 0, ease01);
			int rotLB = (int)Mathf.LerpUnclamped(-39, 0, ease01);
			LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
			LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
			LowerArmL.Height = LowerArmL.Height * (FacingRight ? 1200 : 500) / 1000;
			LowerArmR.Height = LowerArmR.Height * (FacingRight ? 500 : 1200) / 1000;

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			AttackLegShake(ease01);

			// Grab
			Target.HandGrabRotationL = UpperArmL.Rotation + FacingSign * 90;
			Target.HandGrabRotationR = UpperArmR.Rotation + FacingSign * 90;
			Target.HandGrabScaleL = FacingSign * 1300;
			Target.HandGrabScaleR = FacingSign * 1300;

			// Z
			UpperArmL.Z = FacingSign * FrontSign * UpperArmL.Z.Abs();
			UpperArmR.Z = -FacingSign * FrontSign * UpperArmR.Z.Abs();
			LowerArmL.Z = FacingSign * FrontSign * LowerArmL.Z.Abs();
			LowerArmR.Z = -FacingSign * FrontSign * LowerArmR.Z.Abs();
			HandL.Z = FacingSign * POSE_Z_HAND;
			HandR.Z = -FacingSign * POSE_Z_HAND;

		}
	}

	public class PoseAttack_Ranged : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			character.AttackStyleLoop = 1;
			if (character.EquippingWeaponHeld == WeaponHandheld.Bow) {
				Bow();
			} else {
				Shooting();
			}
		}
		private static void Bow () {

			bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
			float ease01 = isCharging ?
				Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
				Target.LastAttackCharged ? 1f : Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			AttackHeadDown(ease01, 0, 200, -1000, 0);
			ResetShoulderAndUpperArm();

			// Upper Arm
			int rotUA = FacingRight ? UpperArmL.Rotation : -UpperArmR.Rotation;
			int rotUB = FacingRight ? UpperArmR.Rotation : -UpperArmL.Rotation;
			rotUA = (int)Mathf.LerpUnclamped(rotUA, 90, ease01);
			rotUB = (int)Mathf.LerpUnclamped(rotUB, -90, ease01);
			UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
			UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);

			LowerArmL.LimbRotate(FacingRight ? -90 - rotUA : -0);
			LowerArmR.LimbRotate(FacingRight ? 0 : 90 + rotUA);

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			AttackLegShake(ease01);

			// Z
			UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
			LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
			HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR = FacingRight ? 0 : 180;
			Target.HandGrabScaleL = Target.HandGrabScaleR = 1000;

		}
		private static void Shooting () {

			bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
			float ease01 = isCharging ?
				Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
				Target.LastAttackCharged ? 1f : Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			AttackHeadDown(ease01, 0, 200, -1000, 0);
			ResetShoulderAndUpperArm();

			// Upper Arm
			int rotUA = FacingRight ? UpperArmL.Rotation : -UpperArmR.Rotation;
			int rotUB = FacingRight ? UpperArmR.Rotation : -UpperArmL.Rotation;
			rotUA = (int)Mathf.LerpUnclamped(rotUA, -90, ease01);
			rotUB = (int)Mathf.LerpUnclamped(rotUB, -90, ease01);
			UpperArmL.LimbRotate(FacingRight ? rotUA : -rotUB);
			UpperArmR.LimbRotate(FacingRight ? rotUB : -rotUA);
			UpperArmL.Height += FacingRight ? 2 * A2G : 2 * -A2G;
			UpperArmR.Height += FacingRight ? 2 * -A2G : 2 * A2G;

			int rotLA = -90 - rotUA;
			int rotLB = (int)Mathf.LerpUnclamped(0, 0, ease01);
			LowerArmL.LimbRotate(FacingRight ? rotLA : -rotLB);
			LowerArmR.LimbRotate(FacingRight ? rotLB : -rotLA);
			LowerArmL.Height += FacingRight ? A2G : -A2G;
			LowerArmR.Height += FacingRight ? -A2G : A2G;

			HandL.LimbRotate(FacingSign);
			HandR.LimbRotate(FacingSign);

			// Leg
			AttackLegShake(ease01);

			// Z
			UpperArmL.Z = UpperArmR.Z = FrontSign * (POSE_Z_HAND - 2);
			LowerArmL.Z = LowerArmR.Z = FrontSign * (POSE_Z_HAND - 1);
			HandL.Z = HandR.Z = FrontSign * POSE_Z_HAND;

			// Grab
			Target.HandGrabRotationL = Target.HandGrabRotationR = FacingRight ? 0 : 180;
			Target.HandGrabScaleL = Target.HandGrabScaleR = 1000;

		}
	}

	public class PoseAttack_Magic : PoseAnimation {
		protected override void Animate (PoseCharacter character) {
			base.Animate(character);
			character.AttackStyleLoop = 1;
			switch (character.EquippingWeaponHeld) {
				default:
				case WeaponHandheld.Float:
					Float();
					break;
				case WeaponHandheld.SingleHanded:
					SingleHanded();
					break;
				case WeaponHandheld.Pole:
					Pole();
					break;
			}
		}
		private static void Float () {

			bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
			float ease01 = isCharging ?
				1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
				Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			if (isCharging) {
				AttackHeadDown(ease01, 100, 800, 1000, 100);
			} else {
				AttackHeadDown(ease01, 200, 300, 0, 300);
			}
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
			AttackLegShake(ease01);
		}
		private static void SingleHanded () {

			float ease01 = Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			AttackHeadDown(ease01, 200, 300, 300, 200);
			ResetShoulderAndUpperArm();

			// Arm L
			UpperArmL.LimbRotate((int)(ease01 * 20f));
			LowerArmL.LimbRotate((int)(ease01 * -5f));
			HandL.LimbRotate(FacingSign);

			// Arm R
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
			AttackLegShake(ease01);
		}
		private static void Pole () {

			bool isCharging = Target.IsChargingAttack && Target.AttackChargeStartFrame.HasValue;
			float ease01 = isCharging ?
				1f - Ease.OutBack(((float)(Game.GlobalFrame - Target.AttackChargeStartFrame.Value) / Mathf.Max(Target.MinimalChargeAttackDuration * 2, 1)).Clamp01()) :
				Ease.OutBack((float)(Game.GlobalFrame - Target.LastAttackFrame) / Target.AttackDuration);

			if (isCharging) {
				AttackHeadDown(ease01, 100, 800, 1000, 100);
			} else {
				AttackHeadDown(ease01, 200, 300, 0, 200);
			}
			ResetShoulderAndUpperArm();

			// Upper Arm
			float armGrowAmount = isCharging ? 1f - ease01 : ease01;
			int uRotA = (int)Mathf.LerpUnclamped(-90, 13, ease01);
			int uRotB = (int)Mathf.LerpUnclamped(-69, 33, ease01);
			UpperArmL.LimbRotate(FacingRight ? uRotA : -uRotB);
			UpperArmR.LimbRotate(FacingRight ? uRotB : -uRotA);
			UpperArmL.Height = (int)(UpperArmL.Height * armGrowAmount * (FacingRight ? 1f : 0.2f));
			UpperArmR.Height = (int)(UpperArmR.Height * armGrowAmount * (FacingRight ? 0.2f : 1f));

			// Lower Arm
			int lRotA = (int)Mathf.LerpUnclamped(0, -25, ease01);
			int lRotB = (int)Mathf.LerpUnclamped(-32, 0, ease01);
			LowerArmL.LimbRotate(FacingRight ? lRotA : -lRotB);
			LowerArmR.LimbRotate(FacingRight ? lRotB : -lRotA);
			LowerArmL.Height = (int)(LowerArmL.Height * armGrowAmount * (FacingRight ? 1f : 0.2f));
			LowerArmR.Height = (int)(LowerArmR.Height * armGrowAmount * (FacingRight ? 0.2f : 1f));

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
			AttackLegShake(ease01);

		}
	}
}
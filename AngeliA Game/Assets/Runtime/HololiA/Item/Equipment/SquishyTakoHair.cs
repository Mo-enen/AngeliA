using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	public class SquishyTakoHairBullet : MeleeBullet {


		public static readonly int TYPE_ID = typeof(SquishyTakoHairBullet).AngeHash();
		protected override int Duration => 10;
		protected override int Damage => 1;

		public override void FrameUpdate () {
			base.FrameUpdate();

		}


	}


	public class iSquishyTakoHair : Weapon, IMeleeWeapon {



		// Const
		private static readonly int HAIR_L = "SquishyTakoHair.HairL".AngeHash();
		private static readonly int HAIR_R = "SquishyTakoHair.HairR".AngeHash();
		private static readonly int HAIR_TIP = "SquishyTakoHair.Tip".AngeHash();
		private static readonly int HAIR_SEG = "SquishyTakoHair.Segment".AngeHash();
		private static readonly int[] SQUISHY_HAIR_IDS = {
			typeof(InaDressHair).AngeHash(),
			typeof(InaArtistHair).AngeHash(),
			typeof(InaHair).AngeHash(),
		};

		// Api
		public override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
		public override WeaponType WeaponType => WeaponType.Claw;
		public override int BulletID => SquishyTakoHairBullet.TYPE_ID;
		int IMeleeWeapon.RangeXLeft => 295;
		int IMeleeWeapon.RangeXRight => 295;
		int IMeleeWeapon.RangeY => 477;

		// MSG
		public override void PoseAnimationUpdate_FromEquipment (Entity holder) {

			if (
				holder is not Character character ||
				character.AnimatedPoseType == CharacterPoseAnimationType.Sleep ||
				character.AnimatedPoseType == CharacterPoseAnimationType.PassOut
			) return;

			// Hide Squichy Braid in Hair
			foreach (var id in SQUISHY_HAIR_IDS) {
				if (character.HairID == id) {
					character.HideBraidFrame = Game.GlobalFrame;
					break;
				}
			}

			// Hand Color Darker
			character.HandL.Tint = character.HandR.Tint =
				character.SkinColor.Mult(new Color32(225, 220, 210, 255));

			if (!character.IsAttacking) {
				// Handheld
				Handheld(character);
				// Draw Braids
				if (character.Head.FrontSide) {
					AutoSpriteBraidHair.DrawBraid(
						character, false, HAIR_L, HAIR_R,
						character.HandL.Z - 1, character.HandR.Z - 1,
						positionAmountX: 385,
						positionAmountY: 0,
						facingLeftOffsetX: -10,
						motionAmount: 200,
						offsetX: -character.PoseTwist / 50
					);
				}
			} else {
				// Attack
				int styleIndex = character.AttackStyleIndex % 3;
				bool swingLeft =
					styleIndex == 0 ? !character.FacingRight :
					styleIndex != 1 || character.FacingRight;
				bool swingRight =
					styleIndex == 0 ? character.FacingRight :
					styleIndex != 1 || !character.FacingRight;
				float frame01 = (float)(Game.GlobalFrame - character.LastAttackFrame) / character.AttackDuration;
				float frame010 = frame01 < 0.5f ? frame01 * 2f : 2f - frame01 * 2f;
				float ease01 = Ease.OutBack(frame01);
				AttackHair(ease01, frame010, character, character.HandL, swingLeft, styleIndex == 1);
				AttackHair(ease01, frame010, character, character.HandR, swingRight, styleIndex == 1);
			}

		}

		private static void Handheld (Character character) {

			if (character.AnimatedPoseType == CharacterPoseAnimationType.Climb) return;

			AnimationLibrary.ResetShoulderAndUpperArm(character);

			bool facingRight = character.FacingRight;
			int facingSign = facingRight ? 1 : -1;
			int frontSign = character.Body.FrontSide ? 1 : -1;

			// Arm
			int twistRot = character.PoseTwist / 50;
			character.UpperArmL.LimbRotate((facingRight ? -21 : -22) + twistRot);
			character.UpperArmR.LimbRotate((facingRight ? 22 : 21) + twistRot);

			character.LowerArmL.LimbRotate((facingRight ? -146 : -153) - twistRot);
			character.LowerArmR.LimbRotate((facingRight ? 153 : 146) - twistRot);

			character.HandL.LimbRotate(facingSign);
			character.HandR.LimbRotate(facingSign);

			// Z
			character.UpperArmL.Z = frontSign * (AnimationLibrary.POSE_Z_HAND - 3);
			character.UpperArmR.Z = frontSign * (AnimationLibrary.POSE_Z_HAND - 3);
			character.LowerArmL.Z = frontSign * (AnimationLibrary.POSE_Z_HAND - 2);
			character.LowerArmR.Z = frontSign * (AnimationLibrary.POSE_Z_HAND - 2);
			character.HandL.Z = frontSign * AnimationLibrary.POSE_Z_HAND + 3;
			character.HandR.Z = frontSign * AnimationLibrary.POSE_Z_HAND + 3;

			// Grab
			character.HandGrabRotationL = character.HandGrabRotationR = 0;
			character.HandGrabScaleL = character.HandGrabScaleR = 1000;
			character.HandGrabAttackTwistL = character.HandGrabAttackTwistR = 0;

			character.CalculateBodypartGlobalPosition();

		}

		private static void AttackHair (float ease01, float frame010, Character character, BodyPart hand, bool swing, bool reverseSwing) {

			if (!CellRenderer.TryGetSpriteFromGroup(
				HAIR_TIP, swing ? ((1.1f - ease01) * 3).RoundToInt() : 0, out var sprite, false, true)
			) return;

			int facingSign = character.FacingRight ? 1 : -1;
			int fromX = hand.GlobalX;
			int fromY = hand.GlobalY + hand.Height / 2;
			int toX = fromX;
			int toY = fromY;
			int width = sprite.GlobalWidth;
			int height = sprite.GlobalHeight;
			int rot;
			if (swing) {
				toX += facingSign * (int)Mathf.LerpUnclamped(137, -64, ease01) * (reverseSwing ? -1 : 1);
				toY += (int)Mathf.LerpUnclamped(16, -96, frame010);
				rot = facingSign * (int)Mathf.LerpUnclamped(-87, 105, ease01) * (reverseSwing ? -1 : 1);
				width = (int)(width * Mathf.LerpUnclamped(0.9f, 1.2f, ease01));
				height = (int)(height * Mathf.LerpUnclamped(0.9f, 1.2f, ease01));
			} else {
				width /= 2;
				height /= 2;
				toX += facingSign * (int)Mathf.LerpUnclamped(12, -10, ease01);
				toY += (int)Mathf.LerpUnclamped(-32, -64, ease01);
				rot = facingSign * (int)Mathf.LerpUnclamped(-5, 5, ease01);
			}
			if (swing) {
				if (reverseSwing) {
					width = -width;
				}
				if (!character.FacingRight) {
					width = -width;
				}
			}

			// Seg
			if (swing && CellRenderer.TryGetSprite(HAIR_SEG, out var segSprite)) {
				CellRenderer.Draw(
					segSprite.GlobalID,
					fromX, fromY,
					500, 500, 0,
					segSprite.GlobalWidth * 3 / 10,
					segSprite.GlobalHeight * 3 / 10,
					Const.WHITE, hand.Z
				);
				CellRenderer.Draw(
					segSprite.GlobalID,
					(fromX + toX) / 2, (fromY + toY) / 2,
					500, 500, 0,
					segSprite.GlobalWidth * 6 / 10,
					segSprite.GlobalHeight * 6 / 10,
					Const.WHITE, hand.Z
				);
			}

			// Tip
			CellRenderer.Draw(
				sprite.GlobalID, toX, toY,
				sprite.PivotX, sprite.PivotY, rot + 45,
				width, height, Const.WHITE, hand.Z + 1
			);

		}

	}
}
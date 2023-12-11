using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public enum WeaponType { Hand, Sword, Axe, Hammer, Flail, Ranged, Polearm, Hook, Claw, Magic, Throwing, }


	public enum WeaponHandheld { SingleHanded, DoubleHanded, OneOnEachHand, Pole, MagicPole, Bow, Firearm, Float, }


	public interface IMeleeWeapon {
		public int RangeXLeft { get; }
		public int RangeXRight { get; }
		public int RangeY { get; }
	}


	[EntityAttribute.MapEditorGroup("ItemWeapon")]
	public abstract class Weapon : Equipment {

		// VAR
		protected int SpriteID { get; init; }
		public sealed override EquipmentType EquipmentType => EquipmentType.Weapon;
		public abstract WeaponType WeaponType { get; }
		public abstract WeaponHandheld Handheld { get; }
		public virtual int AttackDuration => 12;
		public virtual int AttackCooldown => 2;
		public virtual int BulletID => DefaultBullet.TYPE_ID;
		public virtual int ChargeAttackDuration => int.MaxValue;
		public virtual int? MovementLoseRateOnAttack => null;
		public virtual bool RepeatAttackWhenHolding => false;
		public virtual bool LockFacingOnAttack => false;
		protected virtual bool IgnoreGrabTwist => false;

		// MSG
		public Weapon () {
			SpriteID = $"{GetType().AngeName()}.Main".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID)) SpriteID = 0;
		}

		public virtual bool AllowingAttack (PoseCharacter character) => true;

		public override void PoseAnimationUpdate_FromEquipment (Entity holder) {

			base.PoseAnimationUpdate_FromEquipment(holder);

			if (
				holder is not PoseCharacter character ||
				character.AnimationType == CharacterAnimationType.Sleep ||
				character.AnimationType == CharacterAnimationType.PassOut ||
				character.AnimationType == CharacterAnimationType.Crash ||
				!CellRenderer.TryGetSprite(SpriteID, out var sprite)
			) return;

			bool attacking = character.IsAttacking;
			int grabScaleL = character.HandGrabScaleL;
			int grabScaleR = character.HandGrabScaleR;
			int twistL = attacking && !IgnoreGrabTwist ? character.HandGrabAttackTwistL : 1000;
			int twistR = attacking && !IgnoreGrabTwist ? character.HandGrabAttackTwistR : 1000;
			int zLeft = character.HandL.Z - 1;
			int zRight = character.HandR.Z - 1;

			if (character.EquippingWeaponType == WeaponType.Claw) {
				grabScaleL = grabScaleL * 700 / 1000;
				grabScaleR = grabScaleR * 700 / 1000;
				if (CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) && meta.IsTrigger) {
					zLeft = character.HandL.Z + 1;
					zRight = character.HandR.Z + 1;
				}
			}

			// Draw
			switch (character.EquippingWeaponHeld) {

				default:
				case WeaponHandheld.Float: {
					// Floating
					const int SHIFT_X = 148;
					int moveDeltaX = -character.DeltaPositionX * 2;
					int moveDeltaY = -character.DeltaPositionY;
					int facingFrame = Game.GlobalFrame - character.LastFacingChangeFrame;
					if (facingFrame < 30) {
						moveDeltaX += (int)Mathf.LerpUnclamped(
							character.FacingRight ? SHIFT_X * 2 : -SHIFT_X * 2, 0,
							Ease.OutBack(facingFrame / 30f)
						);
					}
					DrawWeaponSprite(
						character,
						character.X + (character.FacingRight ? -SHIFT_X : SHIFT_X) + moveDeltaX,
						character.Y + Const.CEL * character.CharacterHeight / 263 + Game.GlobalFrame.PingPong(240) / 4 + moveDeltaY,
						sprite.GlobalWidth,
						sprite.GlobalHeight,
						0,
						attacking ? grabScaleL : 700,
						sprite,
						36
					);
					break;
				}

				case WeaponHandheld.SingleHanded: {
					// Single 
					int grabScale = grabScaleR;
					int grabRotation = character.HandGrabRotationR;
					int z = zRight;
					if (character.EquippingWeaponType == WeaponType.Throwing) {
						if (
							attacking &&
							Game.GlobalFrame - character.LastAttackFrame > character.AttackDuration / 6
						) break;
						grabScale = 700;
						z = character.FacingFront ? character.HandR.Z.Abs() + 1 : -character.HandR.Z.Abs() - 1;
					}
					// Fix Rotation
					if (CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) && meta.IsTrigger) {
						if (!attacking) {
							grabRotation = 0;
						} else {
							grabRotation = Util.RemapUnclamped(
								0, character.AttackDuration,
								character.FacingRight ? 90 : -90, 0,
								Game.GlobalFrame - character.LastAttackFrame
							);
						}
					}
					// Draw
					var center = character.HandR.GlobalLerp(0.5f, 0.5f);
					DrawWeaponSprite(
						character,
						center.x, center.y,
						sprite.GlobalWidth * twistR / 1000,
						sprite.GlobalHeight,
						grabRotation, grabScale,
						sprite, z
					);
					break;
				}

				case WeaponHandheld.DoubleHanded:
				case WeaponHandheld.Firearm: {
					// Double
					var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
					var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
					DrawWeaponSprite(
						character,
						(centerL.x + centerR.x) / 2,
						(centerL.y + centerR.y) / 2,
						sprite.GlobalWidth * twistR / 1000,
						sprite.GlobalHeight,
						character.HandGrabRotationL,
						grabScaleL, sprite,
						zRight
					);
					break;
				}

				case WeaponHandheld.OneOnEachHand: {
					// Each Hand
					var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
					var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
					DrawWeaponSprite(
						character,
						centerL.x, centerL.y,
						sprite.GlobalWidth * twistL / 1000,
						sprite.GlobalHeight,
						character.HandGrabRotationL,
						grabScaleL, sprite,
						zLeft
					);
					DrawWeaponSprite(
						character,
						centerR.x, centerR.y,
						sprite.GlobalWidth * twistR / 1000,
						sprite.GlobalHeight,
						character.HandGrabRotationR,
						grabScaleR, sprite,
						zRight
					);
					break;
				}

				case WeaponHandheld.Pole: {
					// Polearm
					var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
					var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
					DrawWeaponSprite(
						character,
						(centerL.x + centerR.x) / 2,
						(centerL.y + centerR.y) / 2,
						sprite.GlobalWidth * twistR / 1000,
						sprite.GlobalHeight,
						character.HandGrabRotationR,
						grabScaleR,
						sprite,
						zRight
					);
					break;
				}

				case WeaponHandheld.Bow: {
					if (attacking) {
						// Attacking
						var center = (character.FacingRight ? character.HandR : character.HandL).GlobalLerp(0.5f, 0.5f);
						int width = sprite.GlobalWidth;
						int height = sprite.GlobalHeight;
						if (!CellRenderer.TryGetMeta(sprite.GlobalID, out var meta) || !meta.IsTrigger) {
							int localFrame = Game.GlobalFrame - character.LastAttackFrame;
							if (localFrame < character.AttackDuration / 2) {
								// Pulling
								float ease01 = Ease.OutQuad(localFrame / (character.AttackDuration / 2f));
								width += Mathf.LerpUnclamped(0, width * 2 / 3, ease01).RoundToInt();
								height -= Mathf.LerpUnclamped(0, height / 2, ease01).RoundToInt();
							} else {
								// Release
								float ease01 = Ease.OutQuad((localFrame - character.AttackDuration / 2f) / (character.AttackDuration / 2f));
								width += Mathf.LerpUnclamped(width * 2 / 3, 0, ease01).RoundToInt();
								height -= Mathf.LerpUnclamped(height / 2, 0, ease01).RoundToInt();
							}
						}
						DrawWeaponSprite(
							character, center.x, center.y, width, height,
							0, character.FacingRight ? 1000 : -1000,
							sprite, character.FacingRight ? zRight : zLeft
						);
					} else {
						// Holding
						var center = (character.FacingRight ? character.HandR : character.HandL).GlobalLerp(0.5f, 0.5f);
						DrawWeaponSprite(
							character, center.x, center.y,
							sprite.GlobalWidth, sprite.GlobalHeight,
							character.HandGrabRotationL,
							grabScaleL, sprite,
							zRight
						);
					}
					break;
				}

			}

		}

		protected virtual Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) => CellRenderer.Draw(
			sprite.GlobalID,
			x, y,
			sprite.PivotX, sprite.PivotY, grabRotation,
			width * grabScale / 1000,
			height * grabScale.Abs() / 1000,
			z
		);

		public virtual int GetOverrideHandheldAnimationID (Character character) => 0;

		public virtual int GetOverrideAttackAnimationID (Character character) => 0;

	}



}

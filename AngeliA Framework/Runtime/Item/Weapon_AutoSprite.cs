using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	// Throwing
	public abstract class AutoSpriteThrowingWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Throwing;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
	}


	// Sword
	public abstract class AutoSpriteSword : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Sword;
		public override int BulletID => DefaultMeleeBullet.TYPE_ID;
	}


	// Polearm
	public abstract class AutoSpritePolearm : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Polearm;
		public sealed override WeaponHandHeld HandHeld => WeaponHandHeld.Pole;
		public override int BulletID => DefaultMeleeBullet.TYPE_ID;
		protected override bool IgnoreGrabTwist => true;
		public override int AttackDuration => 18;
		public override int AttackCooldown => 2;
	}


	// Magic
	public abstract class AutoSpriteMagicWeapon : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Magic;
	}


	// Hammer
	public abstract class AutoSpriteHammer : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Hammer;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
		public override int BulletID => DefaultMeleeBullet.TYPE_ID;
		protected override bool IgnoreGrabTwist => true;
	}


	// Claw
	public abstract class AutoSpriteClaw : AutoSpriteWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Claw;
		public sealed override WeaponHandHeld HandHeld => WeaponHandHeld.OneOnEachHand;
		public override int BulletID => DefaultMeleeBullet.TYPE_ID;
		public override int AttackDuration => 10;
		public override int AttackCooldown => 0;
		public override int? MovementLoseRateOnAttack => 1000;
	}


	// Axe
	public abstract class AutoSpriteAxe : AutoSpriteWeapon, IMeleeWeapon {
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
		public sealed override WeaponType WeaponType => WeaponType.Axe;
		public override int BulletID => DefaultMeleeBullet.TYPE_ID;
		int IMeleeWeapon.RangeXLeft => 275;
		int IMeleeWeapon.RangeXRight => 384;
		int IMeleeWeapon.RangeY => 512;
		public override int AttackDuration => 12;
		public override int AttackCooldown => 2;
		public override int ChargeAttackDuration => 20;
	}


	// Hook
	public abstract class AutoSpriteHook : AutoSpriteWeapon, IMeleeWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Hook;
		public override int BulletID => DefaultMeleeBullet.TYPE_ID;
		public virtual int RangeXLeft => 275;
		public virtual int RangeXRight => 275;
		public virtual int RangeY => 432;
	}


	// Firearm
	public abstract class AutoSpriteFirearm : AutoSpriteWeapon {

		public sealed override WeaponType WeaponType => WeaponType.Ranged;
		public sealed override WeaponHandHeld HandHeld => WeaponHandHeld.Firearm;
		private int SpriteIdAttack { get; init; }
		private int SpriteFrameCount { get; init; }

		public AutoSpriteFirearm () {
			SpriteIdAttack = $"{GetType().AngeName()}.Attack".AngeHash();
			if (CellRenderer.HasSpriteGroup(SpriteIdAttack, out int length)) {
				SpriteFrameCount = length;
			} else {
				SpriteIdAttack = 0;
				SpriteFrameCount = 0;
			}
		}

		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			// Draw Attack
			if (character.IsAttacking || character.IsChargingAttack) {
				int localFrame = character.IsAttacking ?
					(Game.GlobalFrame - character.LastAttackFrame) * SpriteFrameCount / character.AttackDuration :
					SpriteFrameCount - 1;
				if (CellRenderer.TryGetSpriteFromGroup(SpriteIdAttack, localFrame, out var attackSprite, false, true)) {
					cell.Color = Const.CLEAR;
					CellRenderer.Draw(
						attackSprite.GlobalID,
						cell.X, cell.Y, attackSprite.PivotX, attackSprite.PivotY, cell.Rotation,
						attackSprite.GlobalWidth,
						character.FacingRight ? attackSprite.GlobalHeight : -attackSprite.GlobalHeight,
						cell.Z
					);
				}
			}
			return cell;
		}

	}


	// Bow
	public abstract class AutoSpriteBow : AutoSpriteWeapon {

		public sealed override WeaponType WeaponType => WeaponType.Ranged;
		public sealed override WeaponHandHeld HandHeld => WeaponHandHeld.Bow;
		private int SpriteIdString { get; init; }

		public AutoSpriteBow () {
			SpriteIdString = $"{GetType().AngeName()}.String".AngeHash();
			if (!CellRenderer.HasSprite(SpriteIdString)) SpriteIdString = 0;
		}

		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			DrawString(character, cell, default, default, default);
			return cell;
		}

		protected void DrawString (Character character, Cell mainCell, Vector2Int offsetDown, Vector2Int offsetUp, Vector2Int offsetCenter) {
			int borderL = 0;
			int borderD = 0;
			int borderU = 0;
			if (CellRenderer.TryGetSprite(SpriteID, out var mainSprite)) {
				borderL = mainSprite.GlobalBorder.left * mainCell.Width.Sign();
				borderD = mainSprite.GlobalBorder.down;
				borderU = mainSprite.GlobalBorder.up;
			}
			if (!character.FacingRight) {
				offsetDown.x = -offsetDown.x;
				offsetUp.x = -offsetUp.x;
				offsetCenter.x = -offsetCenter.x;
			}
			if (character.IsAttacking || character.IsChargingAttack) {

				// Attacking
				int duration = character.AttackDuration;
				int localFrame = character.IsAttacking ? Game.GlobalFrame - character.LastAttackFrame : duration / 2 - 1;
				Vector2Int centerPos;
				var cornerU = mainCell.LocalToGlobal(borderL, mainCell.Height - borderU) + offsetUp;
				var cornerD = mainCell.LocalToGlobal(borderL, borderD) + offsetDown;
				var handPos = (character.FacingRight ? character.HandL : character.HandR).GlobalLerp(0.5f, 0.5f);
				if (localFrame < duration / 2) {
					// Pulling
					centerPos = handPos + offsetCenter;
				} else {
					// Release
					centerPos = Vector2.Lerp(
						handPos, mainCell.LocalToGlobal(borderL, mainCell.Height / 2),
						Ease.OutBack((localFrame - duration / 2f) / (duration / 2f))
					).RoundToInt() + offsetCenter;
				}

				// Draw Strings
				int stringWidth = character.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE;
				CellRenderer.Draw(
					SpriteIdString, centerPos.x, centerPos.y, 500, 0,
					-(int)Quaternion.FromToRotation(
						Vector3.up, (Vector3)(Vector2)(cornerU - centerPos)
					).eulerAngles.z,
					stringWidth, Util.DistanceInt(centerPos, cornerU), mainCell.Z - 1
				);
				CellRenderer.Draw(
					SpriteIdString, centerPos.x, centerPos.y, 500, 0,
					-(int)Quaternion.FromToRotation(
						Vector3.up, (Vector3)(Vector2)(cornerD - centerPos)
					).eulerAngles.z,
					stringWidth, Util.DistanceInt(centerPos, cornerD), mainCell.Z - 1
				);

			} else {
				// Holding
				var point = mainCell.LocalToGlobal(borderL + offsetDown.x, borderD + offsetDown.y);
				CellRenderer.Draw(
					SpriteIdString,
					point.x, point.y,
					character.FacingRight ? 0 : 1000, 0, mainCell.Rotation,
					Const.ORIGINAL_SIZE,
					mainCell.Height - borderD - borderU - offsetDown.y + offsetUp.y,
					mainCell.Z - 1
				);
			}
		}

	}


	// Flail
	public abstract class AutoSpriteFlail : AutoSpriteWeapon {

		public sealed override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandHeld HandHeld => WeaponHandHeld.SingleHanded;
		public override int BulletID => DefaultMeleeBullet.TYPE_ID;
		private int SpriteIdHead { get; init; }
		private int SpriteIdChain { get; init; }
		protected virtual int ChainLength => Const.CEL * 7 / 9;
		protected virtual int ChainLengthAttackGrow => 500;
		protected virtual int HeadCount => 1;

		public AutoSpriteFlail () {
			SpriteIdHead = $"{GetType().AngeName()}.Head".AngeHash();
			if (!CellRenderer.HasSprite(SpriteIdHead)) SpriteIdHead = 0;
			SpriteIdChain = $"{GetType().AngeName()}.Chain".AngeHash();
			if (!CellRenderer.HasSpriteGroup(SpriteIdChain)) SpriteIdChain = 0;
		}

		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			// Fix Grab Rotation
			if (character.EquippingWeaponHeld != WeaponHandHeld.Pole) {
				character.HandGrabRotationL += (
					character.HandGrabRotationL.Sign() * -Mathf.Sin(character.HandGrabRotationL.Abs() * Mathf.Deg2Rad) * 30
				).RoundToInt();
				character.HandGrabRotationR += (
					character.HandGrabRotationR.Sign() * -Mathf.Sin(character.HandGrabRotationR.Abs() * Mathf.Deg2Rad) * 30
				).RoundToInt();
			}
			// Draw
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			for (int i = 0; i < HeadCount; i++) {
				DrawFlailHead(character, cell, i);
			}
			return cell;
		}

		private void DrawFlailHead (Character character, Cell handleCell, int headIndex) {

			bool isAttacking = character.IsAttacking;
			bool climbing = character.AnimatedPoseType == CharacterPoseAnimationType.Climb;
			int deltaX = character.DeltaPositionX.Clamp(-20, 20);
			int deltaY = character.DeltaPositionY.Clamp(-30, 30);
			var point = handleCell.LocalToGlobal(handleCell.Width / 2, handleCell.Height);
			int chainLength = isAttacking ? ChainLength * ChainLengthAttackGrow / 1000 : ChainLength;
			Vector2Int headPos;

			if (isAttacking) {
				// Attack
				int localFrame = Game.GlobalFrame - character.LastAttackFrame;
				int duration = character.AttackDuration;
				int swingX = Const.CEL.LerpTo(-Const.CEL, Ease.OutBack((float)localFrame / duration));
				headPos = handleCell.LocalToGlobal(
					handleCell.Width / 2 + (character.FacingRight ? -swingX : swingX) + headIndex * 96,
					handleCell.Height + chainLength - headIndex * 16
				);
			} else {
				// Hover
				headPos = new Vector2Int(
					point.x - deltaX,
					point.y - chainLength - deltaY
				);
				// Shake
				const int SHAKE_DURATION = 60;
				int shakeFrame = Mathf.Min(
					(Game.GlobalFrame - (character.LastEndMoveFrame >= 0 ? character.LastEndMoveFrame : 0)).Clamp(0, SHAKE_DURATION),
					(Game.GlobalFrame - (character.LastAttackFrame >= 0 ? character.LastAttackFrame : 0)).Clamp(0, SHAKE_DURATION)
				);
				if (!climbing && shakeFrame >= 0 && shakeFrame < SHAKE_DURATION) {
					headPos.x += (
						Mathf.Cos(shakeFrame / (float)SHAKE_DURATION * 720f * Mathf.Deg2Rad) *
						(SHAKE_DURATION - shakeFrame) * 0.75f
					).RoundToInt();
				}
				if (headIndex > 0) {
					headPos.x += (headIndex % 2 == 0 ? 1 : -1) * ((headIndex + 1) / 2) * 84;
					headPos.y += (headIndex + 1) / 2 * 32;
				}
			}

			// Draw Head
			if (SpriteIdHead != 0 && CellRenderer.TryGetSprite(SpriteIdHead, out var headSprite)) {
				int scale = character.HandGrabScaleR;
				if (climbing && !isAttacking) scale = -scale.Abs();
				int rot = CellRenderer.TryGetMeta(headSprite.GlobalID, out var meta) && meta.IsTrigger ? -(int)Quaternion.FromToRotation(
					Vector3.up,
					new Vector3(point.x - headPos.x, point.y - headPos.y, 0)
				).eulerAngles.z : 0;
				CellRenderer.Draw(
					headSprite.GlobalID, headPos.x, headPos.y,
					headSprite.PivotX, headSprite.PivotY, rot,
					headSprite.GlobalWidth * scale / 1000,
					headSprite.GlobalHeight * scale.Abs() / 1000,
					(character.FacingFront ? 36 : -36) - headIndex
				);
			}

			// Draw Chain
			if (SpriteIdChain != 0 && CellRenderer.HasSpriteGroup(SpriteIdChain, out int chainCount)) {
				int rot = -(int)Quaternion.FromToRotation(
					Vector3.up,
					new Vector3(point.x - headPos.x, point.y - headPos.y, 0)
				).eulerAngles.z;
				for (int i = 0; i < chainCount; i++) {
					if (CellRenderer.TryGetSpriteFromGroup(SpriteIdChain, i, out var chainSprite, false, true)) {
						CellRenderer.Draw(
							chainSprite.GlobalID,
							Util.RemapUnclamped(-1, chainCount, point.x, headPos.x, i),
							Util.RemapUnclamped(-1, chainCount, point.y, headPos.y, i),
							500, 500, rot,
							chainSprite.GlobalWidth / 2, chainSprite.GlobalHeight / 2,
							character.FacingFront ? 35 : -35
						);
					}
				}
			}

		}

	}


	// Weapon
	public abstract class AutoSpriteWeapon : Weapon {

		protected int SpriteID { get; init; }
		protected virtual bool IgnoreGrabTwist => false;

		public AutoSpriteWeapon () {
			SpriteID = $"{GetType().AngeName()}.Main".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID)) SpriteID = 0;
		}

		public override void PoseAnimationUpdate_FromEquipment (Entity holder) {

			base.PoseAnimationUpdate_FromEquipment(holder);

			if (
				holder is not Character character ||
				character.AnimatedPoseType == CharacterPoseAnimationType.Sleep ||
				character.AnimatedPoseType == CharacterPoseAnimationType.PassOut ||
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
				case WeaponHandHeld.Float: {
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

				case WeaponHandHeld.SingleHanded: {
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

				case WeaponHandHeld.DoubleHanded:
				case WeaponHandHeld.Firearm: {
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

				case WeaponHandHeld.OneOnEachHand: {
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

				case WeaponHandHeld.Pole: {
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

				case WeaponHandHeld.Bow: {
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

		protected virtual Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) => CellRenderer.Draw(
			sprite.GlobalID,
			x, y,
			sprite.PivotX, sprite.PivotY, grabRotation,
			width * grabScale / 1000,
			height * grabScale.Abs() / 1000,
			z
		);

	}


}

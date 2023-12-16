using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	// Sword
	public abstract class Sword<B> : Sword where B : MeleeBullet {
		public Sword () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Sword : MeleeWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Sword;
		public override int RangeXLeft => 275;
		public override int RangeXRight => 275;
		public override int RangeY => 432;
	}


	// Polearm
	public abstract class Polearm<B> : Polearm where B : MeleeBullet {
		public Polearm () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Polearm : MeleeWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Polearm;
		public sealed override WeaponHandheld Handheld => WeaponHandheld.Pole;
		protected override bool IgnoreGrabTwist => true;
		public override int AttackDuration => 18;
		public override int AttackCooldown => 2;
		public override int RangeXLeft => 384;
		public override int RangeXRight => 384;
		public override int RangeY => 432;

	}


	// Hammer
	public abstract class Hammer<B> : Hammer where B : MeleeBullet {
		public Hammer () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Hammer : MeleeWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Hammer;
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
		protected override bool IgnoreGrabTwist => true;
		public override int RangeXLeft => 275;
		public override int RangeXRight => 275;
		public override int RangeY => 432;

	}


	// Claw
	public abstract class Claw<B> : Claw where B : MeleeBullet {
		public Claw () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Claw : MeleeWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Claw;
		public sealed override WeaponHandheld Handheld => WeaponHandheld.OneOnEachHand;
		public override int AttackDuration => 10;
		public override int AttackCooldown => 0;
		public override int? MovementLoseRateOnAttack => 1000;
		public override int RangeXLeft => 275;
		public override int RangeXRight => 275;
		public override int RangeY => 432;

	}


	// Axe
	public abstract class Axe<B> : Axe where B : MeleeBullet {
		public Axe () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Axe : MeleeWeapon {
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
		public sealed override WeaponType WeaponType => WeaponType.Axe;
		public override int AttackDuration => 12;
		public override int AttackCooldown => 2;
		public override int RangeXLeft => 275;
		public override int RangeXRight => 384;
		public override int RangeY => 512;
	}


	// Hook
	public abstract class Hook<B> : Hook where B : MeleeBullet {
		public Hook () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Hook : MeleeWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Hook;
		public override int RangeXLeft => 275;
		public override int RangeXRight => 275;
		public override int RangeY => 432;
	}


	// Flail
	public abstract class Flail<B> : Flail where B : MeleeBullet {
		public Flail () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Flail : MeleeWeapon {

		public sealed override WeaponType WeaponType => WeaponType.Flail;
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
		private int SpriteIdHead { get; init; }
		private int SpriteIdChain { get; init; }
		protected virtual int ChainLength => Const.CEL * 7 / 9;
		protected virtual int ChainLengthAttackGrow => 500;
		protected virtual int HeadCount => 1;
		public override int RangeXLeft => 275;
		public override int RangeXRight => 275;
		public override int RangeY => 432;

		public Flail () {
			SpriteIdHead = $"{GetType().AngeName()}.Head".AngeHash();
			if (!CellRenderer.HasSprite(SpriteIdHead)) SpriteIdHead = 0;
			SpriteIdChain = $"{GetType().AngeName()}.Chain".AngeHash();
			if (!CellRenderer.HasSpriteGroup(SpriteIdChain)) SpriteIdChain = 0;
		}

		protected override Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			// Fix Grab Rotation
			if (character.EquippingWeaponHeld != WeaponHandheld.Pole) {
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

		private void DrawFlailHead (PoseCharacter character, Cell handleCell, int headIndex) {

			bool isAttacking = character.IsAttacking;
			bool climbing = character.AnimationType == CharacterAnimationType.Climb;
			int deltaX = character.DeltaPositionX.Clamp(-20, 20);
			int deltaY = character.DeltaPositionY.Clamp(-30, 30);
			var point = handleCell.LocalToGlobal(handleCell.Width / 2, handleCell.Height);
			int chainLength = isAttacking ? ChainLength * ChainLengthAttackGrow / 1000 : ChainLength;
			Int2 headPos;

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
				headPos = new Int2(
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
				int rot = CellRenderer.TryGetMeta(headSprite.GlobalID, out var meta) && meta.IsTrigger ?
					new Float2(point.x - headPos.x, point.y - headPos.y).GetRotation() : 0;
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
				int rot = new Float2(point.x - headPos.x, point.y - headPos.y).GetRotation();
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


	// Firearm
	public abstract class Firearm<B> : Firearm where B : MovableBullet {
		public Firearm () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Firearm : ProjectileWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Ranged;
		public sealed override WeaponHandheld Handheld => WeaponHandheld.Firearm;
		private int SpriteIdAttack { get; init; }
		private int SpriteFrameCount { get; init; }
		public Firearm () {
			SpriteIdAttack = $"{GetType().AngeName()}.Attack".AngeHash();
			if (CellRenderer.HasSpriteGroup(SpriteIdAttack, out int length)) {
				SpriteFrameCount = length;
			} else {
				SpriteIdAttack = 0;
				SpriteFrameCount = 0;
			}
		}
		protected override Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
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


	// Throwing
	public abstract class ThrowingWeapon<B> : ThrowingWeapon where B : MovableBullet {
		public ThrowingWeapon () => BulletID = typeof(B).AngeHash();
	}
	public abstract class ThrowingWeapon : ProjectileWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Throwing;
		public override WeaponHandheld Handheld => WeaponHandheld.SingleHanded;
	}


	// Magic
	public abstract class MagicWeapon<B> : MagicWeapon where B : MovableBullet {
		public MagicWeapon () => BulletID = typeof(B).AngeHash();
	}
	public abstract class MagicWeapon : ProjectileWeapon {
		public sealed override WeaponType WeaponType => WeaponType.Magic;
	}


	// Bow
	public abstract class Bow<B> : Bow where B : MovableBullet {
		public Bow () => BulletID = typeof(B).AngeHash();
	}
	public abstract class Bow : ProjectileWeapon {

		public sealed override WeaponType WeaponType => WeaponType.Ranged;
		public sealed override WeaponHandheld Handheld => WeaponHandheld.Bow;
		private int SpriteIdString { get; init; }

		public Bow () {
			SpriteIdString = $"{GetType().AngeName()}.String".AngeHash();
			if (!CellRenderer.HasSprite(SpriteIdString)) SpriteIdString = 0;
		}

		protected override Cell DrawWeaponSprite (PoseCharacter character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, width, height, grabRotation, grabScale, sprite, z);
			DrawString(character, cell, default, default, default);
			return cell;
		}

		protected void DrawString (PoseCharacter character, Cell mainCell, Int2 offsetDown, Int2 offsetUp, Int2 offsetCenter) {
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
				Int2 centerPos;
				var cornerU = mainCell.LocalToGlobal(borderL, mainCell.Height - borderU) + offsetUp;
				var cornerD = mainCell.LocalToGlobal(borderL, borderD) + offsetDown;
				var handPos = (character.FacingRight ? character.HandL : character.HandR).GlobalLerp(0.5f, 0.5f);
				if (localFrame < duration / 2) {
					// Pulling
					centerPos = handPos + offsetCenter;
				} else {
					// Release
					centerPos = Float2.Lerp(
						handPos, mainCell.LocalToGlobal(borderL, mainCell.Height / 2),
						Ease.OutBack((localFrame - duration / 2f) / (duration / 2f))
					).RoundToInt() + offsetCenter;
				}

				// Draw Strings
				int stringWidth = character.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE;
				CellRenderer.Draw(
					SpriteIdString, centerPos.x, centerPos.y, 500, 0,
					(cornerU - centerPos).GetRotation(),
					stringWidth, Util.DistanceInt(centerPos, cornerU), mainCell.Z - 1
				);
				CellRenderer.Draw(
					SpriteIdString, centerPos.x, centerPos.y, 500, 0,
					(cornerD - centerPos).GetRotation(),
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


}

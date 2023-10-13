using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public enum WeaponType { Hand, Sword, Axe, Hammer, Flail, Bow, Polearm, Hook, Claw, Wand, Throwing, }

	public enum WeaponHandHeld { NoHandHeld, SingleHanded, DoubleHanded, OneOnEachHand, Polearm, Bow, CrossBow, Throw, }



	public abstract class AutoSpriteFlail : AutoSpriteWeapon {

		private int SpriteIdHead { get; init; }
		private int SpriteIdChain { get; init; }
		protected virtual int ChainLength => Const.CEL * 7 / 9;
		protected virtual int ChainLengthAttackGrow => 1000;
		protected virtual int HeadCount => 1;

		public AutoSpriteFlail () {
			SpriteIdHead = $"{GetType().AngeName()}.Head".AngeHash();
			if (!CellRenderer.HasSprite(SpriteIdHead)) SpriteIdHead = 0;
			SpriteIdChain = $"{GetType().AngeName()}.Chain".AngeHash();
			if (!CellRenderer.HasSpriteGroup(SpriteIdChain)) SpriteIdChain = 0;
		}

		protected override Cell DrawWeaponSprite (Character character, int x, int y, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var cell = base.DrawWeaponSprite(character, x, y, grabRotation, grabScale, sprite, z);
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
			var point = handleCell.CellToGlobal(handleCell.Width / 2, handleCell.Height);
			int chainLength = isAttacking ? ChainLength * ChainLengthAttackGrow / 1000 : ChainLength;
			Vector2Int headPos;

			if (isAttacking) {
				// Attack
				int localFrame = Game.GlobalFrame - character.LastAttackFrame;
				int duration = character.AttackDuration;
				int swingX = Util.RemapUnclamped(0, duration, Const.CEL, -Const.CEL, localFrame);
				headPos = handleCell.CellToGlobal(
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


	public abstract class AutoSpriteWeapon : Weapon {

		private int SpriteID { get; init; }

		public AutoSpriteWeapon () {
			SpriteID = $"{GetType().AngeName()}.Main".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID)) SpriteID = 0;
		}

		public override void PoseAnimationUpdate (Entity holder, ItemLocation location) {

			base.PoseAnimationUpdate(holder, location);

			if (
				location != ItemLocation.Equipment ||
				holder is not Character character ||
				character.AnimatedPoseType == CharacterPoseAnimationType.Sleep ||
				character.AnimatedPoseType == CharacterPoseAnimationType.PassOut ||
				!CellRenderer.TryGetSprite(SpriteID, out var sprite)
			) return;

			// Draw
			switch (character.EquippingWeaponHeld) {

				default:
				case WeaponHandHeld.NoHandHeld: {
					// Floating


					break;
				}

				case WeaponHandHeld.SingleHanded: {
					// Single 
					var center = character.HandR.GlobalLerp(0.5f, 0.5f);
					DrawWeaponSprite(
						character,
						center.x, center.y, character.HandGrabRotationR, character.HandGrabScaleR,
						sprite, character.HandR.Z - 1
					);
					break;
				}

				case WeaponHandHeld.DoubleHanded: {
					// Double
					var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
					var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
					DrawWeaponSprite(
						character,
						(centerL.x + centerR.x) / 2,
						(centerL.y + centerR.y) / 2,
						character.HandGrabRotationL,
						character.HandGrabScaleL, sprite,
						character.HandR.Z - 1
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
						character.HandGrabRotationL,
						character.HandGrabScaleL, sprite,
						character.HandL.Z - 1
					);
					DrawWeaponSprite(
						character,
						centerR.x, centerR.y,
						character.HandGrabRotationR,
						character.HandGrabScaleR, sprite,
						character.HandR.Z - 1
					);
					break;
				}

				case WeaponHandHeld.Polearm: {
					// Polearm
					var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
					var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
					DrawWeaponSprite(
						character,
						(centerL.x + centerR.x) / 2,
						(centerL.y + centerR.y) / 2,
						character.HandGrabRotationR,
						character.HandGrabScaleR,
						sprite,
						character.HandR.Z - 1
					);
					break;
				}

				case WeaponHandHeld.Bow: {
					// Bow


					break;
				}

				case WeaponHandHeld.CrossBow: {
					// CrossBow


					break;
				}

				case WeaponHandHeld.Throw: {
					// Throw


					break;
				}
			}

		}

		protected virtual Cell DrawWeaponSprite (Character character, int x, int y, int grabRotation, int grabScale, AngeSprite sprite, int z) => CellRenderer.Draw(
			sprite.GlobalID,
			x, y,
			sprite.PivotX, sprite.PivotY, grabRotation,
			sprite.GlobalWidth * grabScale / 1000,
			sprite.GlobalHeight * grabScale.Abs() / 1000,
			z
		);

	}



	public abstract class Weapon : Equipment {
		public sealed override EquipmentType EquipmentType => EquipmentType.Weapon;
		public abstract WeaponType WeaponType { get; }
		public abstract WeaponHandHeld HandHeld { get; }
	}



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



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
			if (character.IsAttacking) {
				int localFrame = (Game.GlobalFrame - character.LastAttackFrame) * SpriteFrameCount / character.AttackDuration;
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
			if (character.IsAttacking) {

				// Attacking
				int localFrame = Game.GlobalFrame - character.LastAttackFrame;
				Vector2Int centerPos;
				var cornerU = mainCell.CellToGlobal(borderL, mainCell.Height - borderU) + offsetUp;
				var cornerD = mainCell.CellToGlobal(borderL, borderD) + offsetDown;
				var handPos = (character.FacingRight ? character.HandL : character.HandR).GlobalLerp(0.5f, 0.5f);
				if (localFrame < character.AttackDuration / 2) {
					// Pulling
					centerPos = handPos + offsetCenter;
				} else {
					// Release
					centerPos = Vector2.Lerp(
						handPos, mainCell.CellToGlobal(borderL, mainCell.Height / 2),
						Ease.OutBack((localFrame - character.AttackDuration / 2f) / (character.AttackDuration / 2f))
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
				var point = mainCell.CellToGlobal(borderL + offsetDown.x, borderD + offsetDown.y);
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

		protected override Cell DrawWeaponSprite (Character character, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
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
			var point = handleCell.CellToGlobal(handleCell.Width / 2, handleCell.Height);
			int chainLength = isAttacking ? ChainLength * ChainLengthAttackGrow / 1000 : ChainLength;
			Vector2Int headPos;

			if (isAttacking) {
				// Attack
				int localFrame = Game.GlobalFrame - character.LastAttackFrame;
				int duration = character.AttackDuration;
				int swingX = Const.CEL.LerpTo(
					-Const.CEL,
					Ease.OutBack((float)localFrame / duration)
				);
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


		protected int SpriteID { get; init; }


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

				case WeaponHandHeld.DoubleHanded:
				case WeaponHandHeld.Firearm: {
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

				case WeaponHandHeld.Pole: {
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
					if (character.IsAttacking) {
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
							sprite, (character.FacingRight ? character.HandR : character.HandL).Z - 1
						);
					} else {
						// Holding
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
					}
					break;
				}

			}

		}


		private Cell DrawWeaponSprite (Character character, int x, int y, int grabRotation, int grabScale, AngeSprite sprite, int z) => DrawWeaponSprite(character, x, y, sprite.GlobalWidth, sprite.GlobalHeight, grabRotation, grabScale, sprite, z);
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

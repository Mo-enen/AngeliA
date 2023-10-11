using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public enum WeaponType { Hand, Sword, Axe, Hammer, Flail, Bow, Polearm, Hook, Claw, Wand, Throwing, }

	public enum WeaponHandHeld { NoHandHeld, SingleHanded, DoubleHanded, OneOnEachHand, Polearm, Bow, CrossBow, Throw, }



	public abstract class AutoSpriteWeapon : Weapon {

		private int SpriteID { get; init; }

		public AutoSpriteWeapon () {
			SpriteID = $"{GetType().AngeName()}.Main".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID)) SpriteID = 0;
		}

		protected override AngeSprite GetWeaponSprite () => CellRenderer.TryGetSprite(SpriteID, out var sprite) ? sprite : null;

	}



	public abstract class Weapon : Equipment {

		public sealed override EquipmentType EquipmentType => EquipmentType.Weapon;
		public abstract WeaponType WeaponType { get; }
		public abstract WeaponHandHeld HandHeld { get; }

		public override void PoseAnimationUpdate (Entity holder, ItemLocation location) {

			base.PoseAnimationUpdate(holder, location);

			if (
				location != ItemLocation.Equipment ||
				holder is not Character character ||
				character.AnimatedPoseType == CharacterPoseAnimationType.Sleep ||
				character.AnimatedPoseType == CharacterPoseAnimationType.PassOut ||
				GetWeaponSprite() is not AngeSprite sprite
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
					DrawSprite(
						center.x, center.y, character.HandGrabRotationR, character.HandGrabScaleR, sprite, character.FacingFront ? 36 : -36
					);
					break;
				}

				case WeaponHandHeld.DoubleHanded: {
					var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
					var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
					DrawSprite(
						(centerL.x + centerR.x) / 2,
						(centerL.y + centerR.y) / 2,
						character.HandGrabRotationL,
						character.HandGrabScaleL, sprite,
						character.FacingFront ? 36 : -36
					);
					break;
				}

				case WeaponHandHeld.OneOnEachHand: {
					// Each Hand
					var centerL = character.HandL.GlobalLerp(0.5f, 0.5f);
					var centerR = character.HandR.GlobalLerp(0.5f, 0.5f);
					DrawSprite(
						centerL.x, centerL.y,
						character.HandGrabRotationL,
						character.HandGrabScaleL, sprite,
						character.FacingFront ? 36 : -36
					);
					DrawSprite(
						centerR.x, centerR.y,
						character.HandGrabRotationR,
						character.HandGrabScaleR,
						sprite, character.FacingFront ? 36 : -36
					);
					break;
				}

				case WeaponHandHeld.Polearm: {
					// Polearm



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

		protected abstract AngeSprite GetWeaponSprite ();

		private static void DrawSprite (int x, int y, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			if (sprite.GlobalBorder.IsZero) {
				CellRenderer.Draw(
					sprite.GlobalID,
					x, y,
					sprite.PivotX, sprite.PivotY, grabRotation,
					sprite.GlobalWidth * grabScale / 1000, sprite.GlobalHeight * grabScale / 1000, z
				);
			} else {
				CellRenderer.Draw_9Slice(
					sprite.GlobalID,
					x, y,
					sprite.PivotX, sprite.PivotY, grabRotation,
					sprite.GlobalWidth * grabScale / 1000, sprite.GlobalHeight * grabScale / 1000, z
				);
			}
		}

	}



}

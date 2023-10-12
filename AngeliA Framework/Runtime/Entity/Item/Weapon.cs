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
					DrawWeaponSprite(
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
						centerL.x, centerL.y,
						character.HandGrabRotationL,
						character.HandGrabScaleL, sprite,
						character.HandL.Z - 1
					);
					DrawWeaponSprite(
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


		protected abstract AngeSprite GetWeaponSprite ();


		protected virtual Cell DrawWeaponSprite (int x, int y, int grabRotation, int grabScale, AngeSprite sprite, int z) => CellRenderer.Draw(
			sprite.GlobalID,
			x, y,
			sprite.PivotX, sprite.PivotY, grabRotation,
			sprite.GlobalWidth * grabScale / 1000,
			sprite.GlobalHeight * grabScale.Abs() / 1000,
			z
		);


	}



}

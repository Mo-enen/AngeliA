using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public enum WeaponType { Hand, Sword, Axe, Hammer, Flail, Bow, Polearm, Hook, Claw, Wand, Throwing, }

	public enum WeaponHandHeld { NoHandHeld, SingleHanded, DoubleHanded, OneOnEachHand, Polearm, }



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
			switch (HandHeld) {
				default:
				case WeaponHandHeld.NoHandHeld:
					// Floating


					break;
				case WeaponHandHeld.SingleHanded:
				case WeaponHandHeld.DoubleHanded:
					// Single 
					DrawSprite(
						character.HandR, character.HandGrabRotationR, character.HandGrabScaleR, sprite, character.FacingFront ? 36 : -36
					);
					break;
				case WeaponHandHeld.OneOnEachHand:
					// Each Hand
					DrawSprite(
						character.HandL, character.HandGrabRotationL, character.HandGrabScaleL, sprite, character.FacingFront ? 36 : -36
						);
					DrawSprite(
						character.HandR, character.HandGrabRotationR, character.HandGrabScaleR, sprite, character.FacingFront ? 36 : -36
						);
					break;
				case WeaponHandHeld.Polearm:
					// Polearm



					break;
			}


		}

		protected abstract AngeSprite GetWeaponSprite ();

		private static void DrawSprite (BodyPart hand, int grabRotation, int grabScale, AngeSprite sprite, int z) {
			var handCenter = hand.GlobalLerp(0.5f, 0.5f);
			if (sprite.GlobalBorder.IsZero) {
				CellRenderer.Draw(
					sprite.GlobalID,
					handCenter.x, handCenter.y,
					sprite.PivotX, sprite.PivotY, grabRotation,
					sprite.GlobalWidth * grabScale / 1000, sprite.GlobalHeight * grabScale / 1000, z
				);
			} else {
				CellRenderer.Draw_9Slice(
					sprite.GlobalID,
					handCenter.x, handCenter.y,
					sprite.PivotX, sprite.PivotY, grabRotation,
					sprite.GlobalWidth * grabScale / 1000, sprite.GlobalHeight * grabScale / 1000, z
				);
			}
		}

	}



}

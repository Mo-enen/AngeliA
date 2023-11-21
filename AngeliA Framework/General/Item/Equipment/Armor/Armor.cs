using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Armor : Equipment {


		// VAR
		protected virtual System.Type PrevEquipment => null;
		protected virtual System.Type NextEquipment => null;
		protected virtual System.Type[] RepairMaterials => null;
		protected virtual int Scale => 1000;

		private readonly int[] SpritesID = new int[8];
		private readonly int PrevEquipmentID;
		private readonly int NextEquipmentID;
		private readonly int[] RepairMaterialsID;
		private readonly bool IsSkirt = false;

		// MSG
		public Armor () {
			PrevEquipmentID = AngeUtil.GetAngeHash(PrevEquipment);
			NextEquipmentID = AngeUtil.GetAngeHash(NextEquipment);
			RepairMaterialsID = AngeUtil.GetAngeHashs(RepairMaterials);
			string basicName = GetType().AngeName();
			switch (EquipmentType) {
				case EquipmentType.Body:
					IsSkirt = false;
					SpritesID[0] = SpritesID[7] = $"{basicName}.Body".AngeHash();
					SpritesID[1] = $"{basicName}.Hip".AngeHash();
					SpritesID[2] = $"{basicName}.Shoulder".AngeHash();
					SpritesID[3] = $"{basicName}.UpperArm".AngeHash();
					SpritesID[4] = $"{basicName}.LowerArm".AngeHash();
					SpritesID[5] = $"{basicName}.UpperLeg".AngeHash();
					SpritesID[6] = $"{basicName}.LowerLeg".AngeHash();
					if (!CellRenderer.HasSpriteGroup(SpritesID[0])) {
						SpritesID[0] = $"{basicName}.BodyL".AngeHash();
						SpritesID[7] = $"{basicName}.BodyR".AngeHash();
					}
					if (!CellRenderer.HasSpriteGroup(SpritesID[1]) && !CellRenderer.HasSprite(SpritesID[1])) {
						SpritesID[1] = $"{basicName}.Skirt".AngeHash();
						IsSkirt = true;
					}
					break;
				case EquipmentType.Helmet:
					SpritesID[0] = $"{basicName}.Main".AngeHash();
					SpritesID[1] = $"{basicName}.Back".AngeHash();
					break;
				case EquipmentType.Shoes:
					SpritesID[0] = $"{basicName}.Main".AngeHash();
					break;
				case EquipmentType.Gloves:
					SpritesID[0] = $"{basicName}.Main".AngeHash();
					break;
			}
			for (int i = 0; i < SpritesID.Length; i++) {
				int id = SpritesID[i];
				if (id != 0 && !CellRenderer.HasSprite(id) && !CellRenderer.HasSpriteGroup(id)) SpritesID[i] = 0;
			}
		}


		public override void PoseAnimationUpdate_FromEquipment (Entity holder) {
			base.PoseAnimationUpdate_FromEquipment(holder);

			if (
				holder is not Character character ||
				character.RenderWithSheet ||
				character.AnimatedPoseType == CharacterPoseAnimationType.Sleep ||
				character.AnimatedPoseType == CharacterPoseAnimationType.PassOut
			) return;

			// Draw Armor
			switch (EquipmentType) {
				case EquipmentType.Body:
					DrawBodyArmor(character);
					break;
				case EquipmentType.Helmet:
					DrawHelmet(character);
					break;
				case EquipmentType.Shoes:
					DrawShoes(character);
					break;
				case EquipmentType.Gloves:
					DrawGloves(character);
					break;
			}

		}


		private void DrawBodyArmor (Character character) {

			int bodyId = SpritesID[0];
			int bodyIdAlt = SpritesID[7];
			int hipID = SpritesID[1];
			int shoulderID = SpritesID[2];
			int upperArmID = SpritesID[3];
			int lowerArmID = SpritesID[4];
			int upperLegID = SpritesID[5];
			int lowerLegID = SpritesID[6];

			// Body
			if (bodyId != 0 || bodyIdAlt != 0) {
				if (CellRenderer.TryGetMeta(bodyId, out var meta) && meta.IsTrigger) {
					if (CellRenderer.TryGetSprite(bodyId, out var bodySprite)) {
						Cloth.AttachClothOn(
							character.Body, bodySprite, 500, 1000, 8,
							widthAmount: character.Body.Width > 0 ? Scale : -Scale,
							heightAmount: Scale,
							defaultHideLimb: false
						);
					}
				} else {
					Cloth.DrawClothForBody(character, bodyId, bodyIdAlt, 8);
				}
			}

			// Hip
			if (hipID != 0) {
				if (IsSkirt) {
					Cloth.DrawClothForSkirt(character, hipID, 7);
				} else {
					Cloth.DrawClothForHip(character, hipID, 2);
				}
			}

			// Cape
			if (bodyId != 0 && CellRenderer.TryGetSpriteFromGroup(
				bodyId, character.Body.FrontSide ? 2 : 3, out var capeSprite, false, false
			)) {
				Cloth.DrawCape(character, capeSprite);
			}

			// Shoulder
			if (shoulderID != 0 && CellRenderer.TryGetSprite(shoulderID, out var shoulderSprite)) {
				Cloth.AttachClothOn(character.ShoulderL, shoulderSprite, 1000, 1000, 16);
				Cloth.AttachClothOn(character.ShoulderR, shoulderSprite, 1000, 1000, 16);
			}

			// Arm
			if (upperArmID != 0) {
				Cloth.CoverClothOn(character.UpperArmL, upperArmID, 3);
				Cloth.CoverClothOn(character.UpperArmR, upperArmID, 3);
			}
			if (lowerArmID != 0) {
				Cloth.CoverClothOn(character.LowerArmL, lowerArmID, 3);
				Cloth.CoverClothOn(character.LowerArmR, lowerArmID, 3);
			}

			// Leg
			if (upperLegID != 0) {
				Cloth.CoverClothOn(character.UpperLegL, upperLegID, 3);
				Cloth.CoverClothOn(character.UpperLegR, upperLegID, 3);
			}
			if (lowerLegID != 0) {
				Cloth.CoverClothOn(character.LowerLegL, lowerLegID, 3);
				Cloth.CoverClothOn(character.LowerLegR, lowerLegID, 3);
			}

		}

		private void DrawHelmet (Character character) {

			var head = character.Head;
			int spriteID = SpritesID[head.FrontSide ? 0 : 1];
			if (spriteID == 0 || !CellRenderer.TryGetSprite(spriteID, out var sprite)) return;
			Cell[] cells;

			// Draw Helmet
			if (CellRenderer.TryGetMeta(spriteID, out var meta) && meta.IsTrigger) {
				// Attach
				cells = Cloth.AttachClothOn(
					head, sprite, 500, 1000, 34 - head.Z, Scale, Scale, defaultHideLimb: false
				);
			} else {
				// Cover
				cells = Cloth.CoverClothOn(
					head, spriteID, 34 - head.Z, Const.WHITE, false
				);
				// Grow Padding
				if (!sprite.GlobalBorder.IsZero && cells != null) {
					var center = head.GetGlobalCenter();
					int widthAbs = head.Width.Abs();
					int heightAbs = head.Height.Abs();
					float scaleX = (widthAbs + sprite.GlobalBorder.horizontal) / (float)widthAbs.GreaterOrEquel(1);
					float scaleY = (heightAbs + sprite.GlobalBorder.vertical) / (float)heightAbs.GreaterOrEquel(1);
					foreach (var cell in cells) {
						cell.ReturnPosition(center.x, center.y);
						cell.Width = (int)(cell.Width * scaleX);
						cell.Height = (int)(cell.Height * scaleY);
					}
				}
			}

			// Head Rotate
			if (cells != null && character.HeadRotation != 0) {
				int offsetY = character.Head.Height.Abs() * character.HeadRotation.Abs() / 360;
				foreach (var cell in cells) {
					cell.RotateAround(character.HeadRotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
					cell.Y -= offsetY;
				}
			}
		}

		private void DrawShoes (Character character) {
			int spriteID = SpritesID[0];
			if (spriteID == 0) return;
			Cloth.DrawClothForFoot(character.FootL, spriteID);
			Cloth.DrawClothForFoot(character.FootR, spriteID);
		}

		private void DrawGloves (Character character) {
			int spriteID = SpritesID[0];
			if (spriteID == 0) return;
			Cloth.CoverClothOn(character.HandL, spriteID);
			Cloth.CoverClothOn(character.HandR, spriteID);
		}


		public override void OnTakeDamage_FromEquipment (Entity holder, Entity sender, ref int damage) {
			base.OnTakeDamage_FromEquipment(holder, sender, ref damage);
			if (PrevEquipmentID != 0 && damage > 0) {
				Inventory.SetEquipment(holder.TypeID, EquipmentType, PrevEquipmentID);
				SpawnEquipmentDamageParticle(TypeID, PrevEquipmentID, holder.X, holder.Y);
				damage--;
			}
		}


		public override void OnSquat (Entity holder) {
			base.OnSquat(holder);
			if (NextEquipmentID == 0) return;
			foreach (var materialID in RepairMaterialsID) {
				if (OnRepair(holder, materialID)) {
					break;
				}
			}
		}


		public virtual bool OnRepair (Entity holder, int materialID) {
			if (materialID == 0) return false;
			int tookCount = Inventory.FindAndTakeItem(holder.TypeID, materialID, 1);
			if (tookCount <= 0) return false;
			Inventory.SetEquipment(holder.TypeID, EquipmentType, NextEquipmentID);
			SpawnItemLostParticle(materialID, holder.X, holder.Y);
			return true;
		}


	}
}
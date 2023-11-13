using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Armor : Equipment {


		// SUB
		protected enum WrapMode { Cover, Attach, }

		// VAR
		protected virtual System.Type PrevEquipment => null;
		protected virtual System.Type NextEquipment => null;
		protected virtual System.Type[] RepairMaterials => null;
		protected virtual WrapMode WrapingMode => WrapMode.Cover;

		private readonly int[] SpritesID = new int[8];
		private readonly int PrevEquipmentID;
		private readonly int NextEquipmentID;
		private readonly int[] RepairMaterialsID;


		// MSG
		public Armor () {
			PrevEquipmentID = AngeUtil.GetAngeHash(PrevEquipment);
			NextEquipmentID = AngeUtil.GetAngeHash(NextEquipment);
			RepairMaterialsID = AngeUtil.GetAngeHashs(RepairMaterials);
			string basicName = GetType().AngeName();
			switch (EquipmentType) {
				case EquipmentType.Body:
					SpritesID[0] = $"{basicName}.Body".AngeHash();
					SpritesID[1] = $"{basicName}.Back".AngeHash();
					SpritesID[2] = $"{basicName}.Hip".AngeHash();
					SpritesID[3] = $"{basicName}.Shoulder".AngeHash();
					SpritesID[4] = $"{basicName}.UpperArm".AngeHash();
					SpritesID[5] = $"{basicName}.LowerArm".AngeHash();
					SpritesID[6] = $"{basicName}.UpperLeg".AngeHash();
					SpritesID[7] = $"{basicName}.LowerLeg".AngeHash();
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
				if (id != 0 && !CellRenderer.HasSprite(id)) SpritesID[i] = 0;
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

			int bodyID = SpritesID[character.Body.FrontSide ? 0 : 1];
			int hipID = SpritesID[2];
			int shoulderID = SpritesID[3];
			int upperArmID = SpritesID[4];
			int lowerArmID = SpritesID[5];
			int upperLegID = SpritesID[6];
			int lowerLegID = SpritesID[7];

			// Body
			if (bodyID != 0) {
				if (WrapingMode == WrapMode.Cover) {
					Cloth.CoverClothOn(character.Body, bodyID, 8);
				} else if (CellRenderer.TryGetSprite(bodyID, out var bodySprite)) {
					Cloth.AttachClothOn(character.Body, bodySprite, 500, 1000, 8, defaultHideLimb: false);
				}
			}

			// Hip
			if (hipID != 0 && CellRenderer.TryGetSprite(hipID, out var hipSprite)) {
				Cloth.AttachClothOn(character.Hip, hipSprite, 500, 1000, 8, defaultHideLimb: false);
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
			if (WrapingMode == WrapMode.Cover) {
				cells = Cloth.CoverClothOn(
					head, spriteID, 34 - head.Z, Const.WHITE, false
				);
			} else {
				cells = Cloth.AttachClothOn(
					head, sprite, 500, 1000, 34 - head.Z, defaultHideLimb: false
				);
			}

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
			Cloth.CoverClothOn(character.FootL, spriteID);
			Cloth.CoverClothOn(character.FootR, spriteID);
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
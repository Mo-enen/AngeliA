using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Armor : Equipment {


		// VAR
		protected virtual System.Type PrevEquipment => null;
		protected virtual System.Type NextEquipment => null;
		protected virtual System.Type[] RepairMaterials => null;

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
			if (bodyID != 0 && CellRenderer.TryGetSprite(bodyID, out var bodySprite)) {
				//Cloth.AttachClothOn(
				//	character.Body, bodySprite, 500, 1000, character.Body.Z + 10,
				//	defaultHideLimb: false
				//);
			}

			// Hip
			if (hipID != 0) {


			}



		}

		private void DrawHelmet (Character character) {

		}

		private void DrawShoes (Character character) {

		}

		private void DrawGloves (Character character) {

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
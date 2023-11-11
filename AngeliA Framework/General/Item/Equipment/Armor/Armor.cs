using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Armor : Equipment {


		// VAR
		protected virtual System.Type PrevEquipment => null;
		protected virtual System.Type NextEquipment => null;
		protected virtual System.Type[] RepairMaterials => null;

		private readonly int SpriteID;
		private readonly int PrevEquipmentID;
		private readonly int NextEquipmentID;
		private readonly int[] RepairMaterialsID;


		// MSG
		public Armor () {
			PrevEquipmentID = AngeUtil.GetAngeHash(PrevEquipment);
			NextEquipmentID = AngeUtil.GetAngeHash(NextEquipment);
			RepairMaterialsID = AngeUtil.GetAngeHashs(RepairMaterials);
			SpriteID = $"{GetType().AngeName()}.Main".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID)) SpriteID = 0;
		}


		public override void OnItemUpdate_FromEquipment (Entity holder) {
			base.OnItemUpdate_FromEquipment(holder);

			if (
				holder is not Character character ||
				character.RenderWithSheet ||
				character.AnimatedPoseType == CharacterPoseAnimationType.Sleep ||
				character.AnimatedPoseType == CharacterPoseAnimationType.PassOut ||
				!CellRenderer.TryGetSprite(SpriteID, out var sprite)
			) return;

			// Draw Armor
			switch (EquipmentType) {
				case EquipmentType.BodySuit:

					break;
				case EquipmentType.Helmet:
					break;
				case EquipmentType.Shoes:
					break;
				case EquipmentType.Gloves:
					break;
			}

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
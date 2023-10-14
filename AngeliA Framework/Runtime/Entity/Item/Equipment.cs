using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public enum EquipmentType { Weapon, BodySuit, Helmet, Shoes, Gloves, Jewelry, }


	public abstract class ProgressiveEquipment<P, N> : Equipment where P : Equipment where N : Equipment {

		protected virtual System.Type RepairMaterial => null;
		protected virtual System.Type RepairMaterialAlt => null;

		private int PrevEquipmentID { get; init; }
		private int NextEquipmentID { get; init; }
		private int MaterialID { get; init; }
		private int MaterialAltID { get; init; }

		public ProgressiveEquipment () {
			PrevEquipmentID = typeof(P).AngeHash();
			NextEquipmentID = typeof(N).AngeHash();
			if (PrevEquipmentID == TypeID) PrevEquipmentID = 0;
			if (NextEquipmentID == TypeID) NextEquipmentID = 0;
			MaterialID = RepairMaterial != null ? RepairMaterial.AngeHash() : 0;
			MaterialAltID = RepairMaterialAlt != null ? RepairMaterialAlt.AngeHash() : 0;
		}

		public override void OnTakeDamage_FromEquipment (Entity holder, Entity sender, ref int damage) {
			base.OnTakeDamage_FromEquipment(holder, sender, ref damage);
			if (PrevEquipmentID != 0 && damage > 0) {
				Inventory.SetEquipment(holder.TypeID, EquipmentType, PrevEquipmentID);
				SpawnEquipmentDamageParticle(TypeID, PrevEquipmentID, holder.X, holder.Y);
				damage--;
			}
		}

		public override void OnRepair (Entity holder) {
			base.OnRepair(holder);
			if (NextEquipmentID == 0) return;
			// Try 0
			if (MaterialID != 0) {
				int tookCount = Inventory.FindAndTakeItem(holder.TypeID, MaterialID, 1);
				if (tookCount > 0) {
					Inventory.SetEquipment(holder.TypeID, EquipmentType, NextEquipmentID);
					SpawnItemLostParticle(MaterialID, holder.X, holder.Y);
					return;
				}
			}
			// Try 1
			if (MaterialAltID != 0) {
				int tookCount = Inventory.FindAndTakeItem(holder.TypeID, MaterialAltID, 1);
				if (tookCount > 0) {
					Inventory.SetEquipment(holder.TypeID, EquipmentType, NextEquipmentID);
					SpawnItemLostParticle(MaterialAltID, holder.X, holder.Y);
					return;
				}
			}
		}

	}



	public abstract class Equipment : Item {

		public static int ItemLostParticleID { get; set; } = 0;
		public static int EquipmentBrokeParticleID { get; set; } = 0;
		public static int EquipmentDamageParticleID { get; set; } = 0;
		public abstract EquipmentType EquipmentType { get; }
		public sealed override int MaxStackCount => 1;

		public static void SpawnItemLostParticle (int itemID, int x, int y) {
			if (ItemLostParticleID == 0) return;
			if (Stage.SpawnEntity(ItemLostParticleID, x, y) is not Particle particle) return;
			particle.UserData = itemID;
		}

		public static void SpawnEquipmentBrokeParticle (int itemID, int x, int y) {
			if (EquipmentBrokeParticleID == 0) return;
			if (Stage.SpawnEntity(EquipmentBrokeParticleID, x, y) is not Particle particle) return;
			particle.UserData = itemID;
		}

		public static void SpawnEquipmentDamageParticle (int itemBeforeID, int itemAfterID, int x, int y) {
			if (EquipmentDamageParticleID == 0) return;
			if (Stage.SpawnEntity(EquipmentDamageParticleID, x, y) is not Particle particle) return;
			particle.UserData = new Vector2Int(itemBeforeID, itemAfterID);
		}

	}
}
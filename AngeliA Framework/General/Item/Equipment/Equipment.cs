using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public enum EquipmentType { Weapon, Body, Helmet, Shoes, Gloves, Jewelry, }


	[EntityAttribute.MapEditorGroup("ItemEquipment")]
	public abstract class Equipment : Item {

		public static int ItemLostParticleID { get; set; } = typeof(ItemLostParticle).AngeHash();
		public static int EquipmentBrokeParticleID { get; set; } = typeof(EquipmentBrokeParticle).AngeHash();
		public static int EquipmentDamageParticleID { get; set; } = typeof(EquipmentDamageParticle).AngeHash();
		public abstract EquipmentType EquipmentType { get; }
		public sealed override int MaxStackCount => 1;

		// API
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
using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	[EntityAttribute.MapEditorGroup("ItemFood")]
	public abstract class Food : Item {
		public override int MaxStackCount => 16;
	}


	[EntityAttribute.MapEditorGroup("Item")]
	public abstract class Item : IMapEditorItem {


		public virtual int MaxStackCount => 64;
		public int TypeID { get; init; }


		public Item () => TypeID = GetType().AngeHash();


		// Inventory
		public virtual void OnItemUpdate_FromInventory (Entity holder) { }
		public virtual void PoseAnimationUpdate_FromInventory (Entity holder) { }
		public virtual void OnTakeDamage_FromInventory (Entity holder, Entity sender, ref int damage) { }

		// Equipment
		public virtual void OnItemUpdate_FromEquipment (Entity holder) { }
		public virtual void PoseAnimationUpdate_FromEquipment (Entity holder) { }
		public virtual void OnTakeDamage_FromEquipment (Entity holder, Entity sender, ref int damage) { }
		public virtual void OnAttack (Entity holder) { }
		public virtual void OnRepair (Entity holder) { }

		// Ground
		public virtual void OnItemUpdate_FromGround (Entity holder) { }

		// Misc
		public virtual void OnCollect (Entity holder) { }

		public virtual bool CanUse (Entity holder) => false;
		public virtual bool Use (Entity holder) => false;

	}
}
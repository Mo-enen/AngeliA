using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	[EntityAttribute.MapEditorGroup("ItemFood")]
	public abstract class Food : Item {
		public override int MaxStackCount => 16;
	}


	public enum ItemLocation { Inventory, Equipment, Ground, }


	[EntityAttribute.MapEditorGroup("Item")]
	public abstract class Item : IMapEditorItem {


		public virtual int MaxStackCount => 64;
		public int TypeID { get; init; }


		public Item () => TypeID = GetType().AngeHash();

		public virtual void Update (Entity holder, ItemLocation location) { }
		public virtual void PoseAnimationUpdate (Entity holder, ItemLocation location) { }
		public virtual void OnAttack (Entity holder, ItemLocation location) { }
		public virtual void OnTakeDamage (Entity holder, ItemLocation location, ref int damage, Entity sender) { }
		public virtual void OnRepair (Entity holder, ItemLocation location) { }
		public virtual void OnCollect (Entity holder) { }

		public virtual bool CanUse (Entity holder) => false;
		public virtual bool Use (Entity holder) => false;

	}
}
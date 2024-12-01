using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


[EntityAttribute.MapEditorGroup("Item")]
public abstract class Item : IMapItem {


	// Api
	public abstract int MaxStackCount { get; }
	public virtual bool AllowDuplicateUpdate => true;
	public virtual bool EmbedIntoLevel => false;
	public int TypeID { get; init; }

	// Cache
	internal int LastUpdateInventory = 0;
	internal int LastUpdateFrame = -1;


	// MSG
	public Item () => TypeID = GetType().AngeHash();

	internal bool CheckUpdateAvailable (int inventoryID) {
		if (AllowDuplicateUpdate) return true;
		bool available = Game.GlobalFrame != LastUpdateFrame || inventoryID != LastUpdateInventory;
		LastUpdateFrame = Game.GlobalFrame;
		LastUpdateInventory = inventoryID;
		return available;
	}

	// Inventory
	public virtual void BeforeItemUpdate_FromInventory (Entity holder, int stackCount) { }
	public virtual void OnItemUpdate_FromInventory (Entity holder, int stackCount) { }
	public virtual void OnPoseAnimationUpdate_FromInventory (PoseCharacterRenderer rendering, int stackCount) { }
	public virtual void OnTakeDamage_FromInventory (Entity holder, int stackCount, Entity sender, ref Damage damage) { }

	// Equipment
	public virtual void BeforeItemUpdate_FromEquipment (Entity holder) { }
	public virtual void OnItemUpdate_FromEquipment (Entity holder) { }
	public virtual void BeforePoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) { }
	public virtual void OnPoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) { }
	public virtual void OnTakeDamage_FromEquipment (Entity holder, Entity sender, ref Damage damage) { }
	public virtual void OnCharacterAttack_FromEquipment (Character character, Bullet bullet) { }
	public virtual bool TryRepairEquipment (Entity holder) => false;

	// Ground
	public virtual void OnItemUpdate_FromGround (Entity holder, int count) { }

	// Misc
	public virtual void OnCollect (Entity holder) { }
	public virtual bool CanUse (Entity holder) => false;
	public virtual bool Use (Entity holder, int inventoryID, int itemIndex, out bool consume) {
		consume = false;
		return false;
	}

}
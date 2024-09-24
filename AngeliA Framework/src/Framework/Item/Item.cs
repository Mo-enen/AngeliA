using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


[EntityAttribute.MapEditorGroup("Item")]
public abstract class Item : IMapItem {


	// Api
	public delegate void ItemHandler (Character character, int item);
	public delegate void ItemDamageHandler (Character character, int itemFrom, int itemTo);
	public static event ItemHandler OnItemLost;
	public static event ItemDamageHandler OnItemDamage;
	public static event ItemHandler OnItemInsufficient;
	public abstract int MaxStackCount { get; }
	public int TypeID { get; init; }


	// MSG
	public Item () => TypeID = GetType().AngeHash();


	// Inventory
	public virtual void BeforeItemUpdate_FromInventory (Entity holder) { }
	public virtual void OnItemUpdate_FromInventory (Entity holder) { }
	public virtual void PoseAnimationUpdate_FromInventory (Entity holder) { }
	public virtual void OnTakeDamage_FromInventory (Entity holder, Entity sender, ref int damage) { }

	// Equipment
	public virtual void BeforeItemUpdate_FromEquipment (Entity holder) { }
	public virtual void OnItemUpdate_FromEquipment (Entity holder) { }
	public virtual void PoseAnimationUpdate_FromEquipment (Entity holder) { }
	public virtual void OnTakeDamage_FromEquipment (Entity holder, Entity sender, ref int damage) { }
	public virtual void OnCharacterAttack (Entity holder) { }
	public virtual bool TryRepair (Entity holder) => false;

	// Callback
	public static void InvokeItemLost (Character holder, int itemID) => OnItemLost?.Invoke(holder, itemID);
	public static void InvokeItemDamage (Character holder, int itemBeforeID, int itemAfterID) => OnItemDamage?.Invoke(holder, itemBeforeID, itemAfterID);
	public static void InvokeItemInsufficient (Character holder, int itemID) => OnItemInsufficient?.Invoke(holder, itemID);

	// Ground
	public virtual void OnItemUpdate_FromGround (Entity holder, int count) { }

	// Misc
	public virtual void OnCollect (Entity holder) { }

	public virtual bool CanUse (Entity holder) => false;
	public virtual bool Use (Entity holder) => false;

}
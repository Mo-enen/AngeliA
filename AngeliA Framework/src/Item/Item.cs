using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


/// <summary>
/// Basic unit inside inventory system. ※※⚠ Use global single instance ⚠※※
/// </summary>
[EntityAttribute.MapEditorGroup("Item")]
public abstract class Item : IMapItem {




	#region --- VAR ---


	// Api
	/// <summary>
	/// Count limitation for multiple items inside one inventory slot
	/// </summary>
	public abstract int MaxStackCount { get; }
	/// <summary>
	/// True if same type of item can update multiple times during same frame for same holder
	/// </summary>
	public virtual bool AllowDuplicateUpdate => true;
	/// <summary>
	/// True if this item can Embed into level blocks and get spawn when the block is picked. (like ore inside stone)
	/// </summary>
	public virtual bool EmbedIntoLevel => false;
	/// <summary>
	/// Unique ID for this type of item
	/// </summary>
	public int TypeID { get; init; }

	// Cache
	internal int LastUpdateFrame = -1;
	private int LastUpdateInventory = 0;


	#endregion




	#region --- MSG ---


	public Item () => TypeID = GetType().AngeHash();

	internal bool CheckUpdateAvailable (int inventoryID) {
		if (AllowDuplicateUpdate) return true;
		bool available = Game.GlobalFrame != LastUpdateFrame || inventoryID != LastUpdateInventory;
		LastUpdateFrame = Game.GlobalFrame;
		LastUpdateInventory = inventoryID;
		return available;
	}


	// Inventory
	/// <summary>
	/// This function is called every frame when this item is in an holder's inventory
	/// </summary>
	/// <param name="holder">The holder of the inventory</param>
	/// <param name="inventoryID">ID of the inventory</param>
	/// <param name="itemIndex"></param>
	public virtual void BeforeItemUpdate_FromInventory (Character holder, int inventoryID, int itemIndex) { }

	/// <summary>
	/// This function is called every frame when this item is in an holder's inventory
	/// </summary>
	/// <param name="holder">The holder of the inventory</param>
	/// <param name="inventoryID">ID of the inventory</param>
	/// <param name="itemIndex"></param>
	public virtual void OnItemUpdate_FromInventory (Character holder, int inventoryID, int itemIndex) { }

	/// <summary>
	/// This function is called every frame when pose-style character update after rendering is ready
	/// </summary>
	/// <param name="rendering">The inventory holder</param>
	/// <param name="inventoryID">ID of the inventory</param>
	/// <param name="itemIndex"></param>
	public virtual void OnPoseAnimationUpdate_FromInventory (PoseCharacterRenderer rendering, int inventoryID, int itemIndex) { }

	/// <summary>
	/// This function is called when holder take damage
	/// </summary>
	/// <param name="holder">The inventory holder</param>
	/// <param name="inventoryID">ID of the inventory</param>
	/// <param name="itemIndex"></param>
	/// <param name="damage"></param>
	public virtual void OnTakeDamage_FromInventory (Character holder, int inventoryID, int itemIndex, ref Damage damage) { }

	/// <summary>
	/// This function is called when holder attacks
	/// </summary>
	/// <param name="character">The inventory holder</param>
	/// <param name="bullet">Bullet entity this attack spawns</param>
	/// <param name="inventoryID">ID of the inventory</param>
	/// <param name="itemIndex"></param>
	public virtual void OnCharacterAttack_FromInventory (Character character, Bullet bullet, int inventoryID, int itemIndex) { }


	// Equipment
	/// <summary>
	/// This function is called every frame when the holder is equipping this item 
	/// </summary>
	public virtual void BeforeItemUpdate_FromEquipment (Character holder) { }

	/// <summary>
	/// This function is called every frame when the holder is equipping this item 
	/// </summary>
	public virtual void OnItemUpdate_FromEquipment (Character holder) { }

	/// <summary>
	/// This function is called every frame when a pose-style character equipping this item
	/// </summary>
	public virtual void BeforePoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) { }

	/// <summary>
	/// This function is called every frame when a pose-style character equipping this item
	/// </summary>
	public virtual void OnPoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) { }

	/// <summary>
	/// This function is called when the holder took damage with this item equipping
	/// </summary>
	public virtual void OnTakeDamage_FromEquipment (Character holder, ref Damage damage) { }

	/// <summary>
	/// This function is called when the holder attack with this item equipping
	/// </summary>
	/// <param name="character"></param>
	/// <param name="bullet">Instance of the bullet entity from the attack</param>
	public virtual void OnCharacterAttack_FromEquipment (Character character, Bullet bullet) { }

	/// <summary>
	/// Perform a repair for this item as an equipment
	/// </summary>
	public virtual bool TryRepairEquipment (Character holder) => false;


	// Ground
	/// <summary>
	/// This funtion is called every frame when it's being holded by a ItemHolder entity on stage
	/// </summary>
	public virtual void OnItemUpdate_FromItemHolder (ItemHolder holder, int count) { }


	// Misc
	/// <summary>
	/// This function is called when this item get collect from ItemHolder
	/// </summary>
	/// <param name="holder">Character that collects this item</param>
	public virtual void OnCollect (Character holder) { }

	/// <summary>
	/// True if this item can be use at current frame
	/// </summary>
	/// <param name="holder">Holder that trying to use this item</param>
	public virtual bool CanUse (Character holder) => false;

	/// <summary>
	/// Perform the logic when the item get used
	/// </summary>
	/// <param name="holder">Holder that using this item</param>
	/// <param name="inventoryID">Inventory ID of this holder</param>
	/// <param name="itemIndex">Index of this item inside the inventory</param>
	/// <param name="consume">True if the item should disappear after being used</param>
	/// <returns>True if the item is used</returns>
	public virtual bool Use (Character holder, int inventoryID, int itemIndex, out bool consume) {
		consume = false;
		return false;
	}

	/// <summary>
	/// True if the item should receive update callback at current condition
	/// </summary>
	/// <param name="holder">Holder that own this item</param>
	public virtual bool ItemConditionCheck (Character holder) => true;


	#endregion




	#region --- API ---


	/// <summary>
	/// Call this function to render the item
	/// </summary>
	/// <param name="holder">Holder that own this item</param>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="tint">Color tint</param>
	/// <param name="z">Z value for sort rendering cells</param>
	public virtual void DrawItem (Entity holder, IRect rect, Color32 tint, int z) {
		if (Renderer.TryGetSpriteForGizmos(TypeID, out var sprite)) {
			Renderer.Draw(sprite, rect.Fit(sprite), tint, z: z);
		} else {
			Renderer.Draw(BuiltInSprite.ICON_ENTITY, rect, tint, z: z);
		}
	}


	#endregion




}
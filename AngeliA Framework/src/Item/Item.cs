using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


[EntityAttribute.MapEditorGroup("Item")]
public abstract class Item : IMapItem {




	#region --- VAR ---


	// Api
	public abstract int MaxStackCount { get; }
	public virtual bool AllowDuplicateUpdate => true;
	public virtual bool EmbedIntoLevel => false;
	public int TypeID { get; init; }

	// Cache
	internal int LastUpdateInventory = 0;
	internal int LastUpdateFrame = -1;


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
	public virtual void BeforeItemUpdate_FromInventory (Character holder, int inventoryID, int itemIndex) { }
	public virtual void OnItemUpdate_FromInventory (Character holder, int inventoryID, int itemIndex) { }
	public virtual void OnPoseAnimationUpdate_FromInventory (PoseCharacterRenderer rendering, int inventoryID, int itemIndex) { }
	public virtual void OnTakeDamage_FromInventory (Character holder, int inventoryID, int itemIndex, ref Damage damage) { }
	public virtual void OnCharacterAttack_FromInventory (Character character, Bullet bullet, int inventoryID, int itemIndex) { }

	// Equipment
	public virtual void BeforeItemUpdate_FromEquipment (Character holder) { }
	public virtual void OnItemUpdate_FromEquipment (Character holder) { }
	public virtual void BeforePoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) { }
	public virtual void OnPoseAnimationUpdate_FromEquipment (PoseCharacterRenderer rendering) { }
	public virtual void OnTakeDamage_FromEquipment (Character holder, Entity sender, ref Damage damage) { }
	public virtual void OnCharacterAttack_FromEquipment (Character character, Bullet bullet) { }
	public virtual bool TryRepairEquipment (Character holder) => false;

	// Ground
	public virtual void OnItemUpdate_FromItemHolder (ItemHolder holder, int count) { }

	// Misc
	public virtual void OnCollect (Character holder) { }
	public virtual bool CanUse (Character holder) => false;
	public virtual bool Use (Character holder, int inventoryID, int itemIndex, out bool consume) {
		consume = false;
		return false;
	}


	#endregion




	#region --- API ---


	public virtual void DrawItem (IRect rect, Color32 tint, int z) {
		if (Renderer.TryGetSpriteForGizmos(TypeID, out var sprite)) {
			Renderer.Draw(sprite, rect.Fit(sprite), tint, z: z);
		} else {
			Renderer.Draw(BuiltInSprite.ICON_ENTITY, rect, tint, z: z);
		}
	}


	#endregion




	#region --- LGC ---



	#endregion




}
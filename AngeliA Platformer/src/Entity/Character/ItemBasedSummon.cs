using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Summon character that summons when other characters put the origin item in their inventory
/// </summary>
public abstract class ItemBasedSummon : Summon {

	/// <summary>
	/// ID of the item that make this character summons
	/// </summary>
	public int OriginItemID { get; set; } = 0;

	public override void OnActivated () {
		base.OnActivated();
		OriginItemID = 0;
	}

	public override void Update () {
		base.Update();
		// Check Item Exists
		if (
			Owner != null &&
			OriginItemID != 0 &&
			Game.GlobalFrame > InventoryUpdatedFrame + 1 &&
			Inventory.ItemTotalCount(Owner.InventoryID, OriginItemID, true) == 0
		) {
			Active = false;
			return;
		}
	}

}

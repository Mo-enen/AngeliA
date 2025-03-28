﻿using AngeliA;

namespace AngeliA.Platformer;

public abstract class ItemBasedSummon : Summon {

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

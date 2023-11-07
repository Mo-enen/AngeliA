using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {

	// Map Chest
	public class MapChestWood : MapChest, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class MapChestIron : MapChest { }


	// Player Chest
	public class PlayerItemChest : InventoryChest, ICombustible {
		protected override int InventoryColumn => 10;
		protected override int InventoryRow => 8;
		int ICombustible.BurnStartFrame { get; set; }
		[OnGameInitialize(64)]
		public static void AfterGameInitialize () {
			Inventory.SetUnlockInside(typeof(PlayerItemChest).AngeHash(), true);
		}
	}


}
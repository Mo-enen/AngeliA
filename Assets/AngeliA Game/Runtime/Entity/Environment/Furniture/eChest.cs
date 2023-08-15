using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	// Map Chest
	public class eMapChestWood : MapChest, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class eMapChestIron : MapChest { }


	// Player Chest
	public class ePlayerItemChest : InventoryChest, ICombustible {
		protected override int InventoryColumn => 10;
		protected override int InventoryRow => 8;
		int ICombustible.BurnStartFrame { get; set; }
		[AfterGameInitialize]
		public static void AfterGameInitialize () {
			Inventory.SetUnlockInside(typeof(ePlayerItemChest).AngeHash(), true);
		}
	}


}
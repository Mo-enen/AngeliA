using System.Collections;
using System.Collections.Generic;


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
		[OnSlotChanged(2048)]
		public static void OnSlotChanged () => Inventory.SetUnlockInside(typeof(PlayerItemChest).AngeHash(), true);
	}


}
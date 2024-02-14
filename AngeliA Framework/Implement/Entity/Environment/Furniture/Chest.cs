using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {

	// Map Chest
	public class MapChestWood : MapChest, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}
	public class MapChestIron : MapChest { }


	// Player Chest
	public class PlayerItemChest : InventoryChest, ICombustible {
		private static readonly int TYPE_ID = typeof(PlayerItemChest).AngeHash();
		protected override int InventoryColumn => 10;
		protected override int InventoryRow => 8;
		int ICombustible.BurnStartFrame { get; set; }
		[OnProjectOpen(32)]
		public static void OnGameInitialize () => Inventory.SetUnlockItemsInside(TYPE_ID, true);
	}


}
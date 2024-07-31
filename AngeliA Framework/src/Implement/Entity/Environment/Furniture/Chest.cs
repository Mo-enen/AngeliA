using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

// Map Chest
public class MapChestWood : MapChest, ICombustible {
	int ICombustible.BurnStartFrame { get; set; }
}
public class MapChestIron : MapChest { }


// Player Chest
public class PlayerItemChest : InventoryChest, ICombustible {
	protected override int InventoryColumn => 10;
	protected override int InventoryRow => 8;
	protected override bool UnlockItemInside => true;
	int ICombustible.BurnStartFrame { get; set; }
}
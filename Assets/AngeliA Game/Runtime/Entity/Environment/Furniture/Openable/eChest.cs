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
	public class ePlayerItemChest : InventoryChest {
		protected override int InventoryColumn => 10;
		protected override int InventoryRow => 8;
	}


}
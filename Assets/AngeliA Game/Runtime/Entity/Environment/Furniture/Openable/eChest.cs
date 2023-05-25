using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	// Map Chest
	public class eMapChestWood : MapChest, ICombustible {
		private static readonly int CODE_OPEN = "MapChestWood Open".AngeHash();
		protected override int OpenArtworkCode => CODE_OPEN;
		int ICombustible.BurnStartFrame { get; set; }
	}


	public class eMapChestIron : MapChest {
		private static readonly int CODE_OPEN = "MapChestIron Open".AngeHash();
		protected override int OpenArtworkCode => CODE_OPEN;
	}


	// Player Chest
	public class ePlayerItemChest : InventoryChest {
		private static readonly int CODE_OPEN = "PlayerItemChest Open".AngeHash();
		protected override int OpenArtworkCode => CODE_OPEN;
	}


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	[EntityAttribute.Capacity(256)]
	public class eCoin : Collectable {


		public override bool OnCollect (Entity source) {
			if (source is not Player) return false;
			Inventory.AddCoin(source.TypeID, 1);
			return true;
		}


	}
}
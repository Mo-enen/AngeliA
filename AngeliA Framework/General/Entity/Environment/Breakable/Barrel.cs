using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class Barrel : BreakableRigidbody, ICombustible {
		private static readonly int ITEM_CODE = typeof(iTreeStump).AngeHash();
		int ICombustible.BurnStartFrame { get; set; }
		protected override void OnBreak () {
			base.OnBreak();
			if (AngeUtil.RandomInt(0, 32) == 0) {
				ItemSystem.ItemSpawnItemAtPlayer(ITEM_CODE);
			}
		}
	}
}

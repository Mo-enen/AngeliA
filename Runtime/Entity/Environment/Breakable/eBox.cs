using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eBox : BreakableRigidbody, ICombustible {
		private static readonly int ITEM_CODE = typeof(iItemWoodBoard).AngeHash();
		public int BurnedDuration => 320;
		int ICombustible.BurnStartFrame { get; set; }
		protected override void OnBreak () {
			base.OnBreak();
			if (AngeUtil.RandomInt(0, 32) == 0) {
				ItemSystem.ItemSpawnItemAtPlayer(ITEM_CODE);
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaGame {
	public class eWoodBoard : BreakableRigidbody, ICombustible {
		private static readonly int ITEM_CODE = typeof(iItemWoodBoard).AngeHash();
		public int BurnedDuration => 30;
		int ICombustible.BurnStartFrame { get; set; }
		protected override int PhysicsLayer => Const.LAYER_ENVIRONMENT;
		protected override bool DestroyWhenInsideGround => true;
		protected override void OnBreak () {
			base.OnBreak();
			if (AngeUtil.RandomInt(0, 32) == 0) {
				ItemSystem.SpawnItem(ITEM_CODE);
			}
		}
	}
}

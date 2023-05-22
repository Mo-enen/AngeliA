using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class ePhoto : Furniture, ICombustible {

		protected override bool LoopArtworkIndex => true;
		int ICombustible.BurnStartFrame { get; set; }

		// MSG
		public override void OnActivated () {
			base.OnActivated();
			ArtworkIndex = Random.Range(int.MinValue, int.MaxValue);
		}

		public override void FillPhysics () {
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}

		public override bool Invoke (Entity target) {
			ArtworkIndex++;
			return true;
		}

		public override bool AllowInvoke (Entity target) => true;


	}
}

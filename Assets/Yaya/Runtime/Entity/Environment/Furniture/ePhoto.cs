using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePhoto : Furniture {


		protected override bool LoopArtworkIndex => true;

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

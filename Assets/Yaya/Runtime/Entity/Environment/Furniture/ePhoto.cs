using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePhoto : eFurniture, IActionEntity {


		protected override bool LoopArtworkIndex => true;

		// MSG
		public override void OnActived () {
			base.OnActived();
			ArtworkIndex = Random.Range(int.MinValue, int.MaxValue);
		}

		public override void FillPhysics () {
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}

		public bool Invoke (Entity target) {
			ArtworkIndex++;
			return true;
		}

		public void CancelInvoke (Entity target) { }


	}
}

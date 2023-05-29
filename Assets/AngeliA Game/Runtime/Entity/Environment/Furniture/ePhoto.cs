using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class ePhoto : Furniture, ICombustible, IActionEntity {

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

		void IActionEntity.Invoke (Entity target) => ArtworkIndex++;

		bool IActionEntity.AllowInvoke (Entity target) => true;


	}
}

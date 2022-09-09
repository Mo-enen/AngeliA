using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePhoto : eFurniture, IActionEntity {


		private static readonly int CODE = "Photo".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => CODE;
		protected override bool LoopArtworkIndex => true;

		// MSG
		public override void OnActived () {
			base.OnActived();
			ArtworkIndex = Random.Range(int.MinValue, int.MaxValue);
		}

		public override void FillPhysics () {
			Physics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}

		public bool Invoke (Entity target) {
			ArtworkIndex++;
			return true;
		}

		public bool CancelInvoke (Entity target) => false;


	}
}

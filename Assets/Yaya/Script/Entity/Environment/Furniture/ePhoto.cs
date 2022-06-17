using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePhoto : eFurniture {

		private static readonly int CODE = "Photo".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => CODE;

		public override void FillPhysics () {
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}

	}
}

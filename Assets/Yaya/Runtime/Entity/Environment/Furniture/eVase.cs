using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eVase : Furniture {

		private static readonly int CODES_DOWN = "Vase Down".AngeHash();
		private static readonly int CODES_MID = "Vase Mid".AngeHash();
		private static readonly int CODES_UP = "Vase Up".AngeHash();
		private static readonly int CODES_SINGLE = "Vase Single".AngeHash();

		protected override Direction3 ModuleType => Direction3.Vertical;
		protected override int ArtworkCode_LeftDown => CODES_DOWN;
		protected override int ArtworkCode_Mid => CODES_MID;
		protected override int ArtworkCode_RightUp => CODES_UP;
		protected override int ArtworkCode_Single => CODES_SINGLE;

		public override void FillPhysics () {
			if (Pose == FittingPose.Up || Pose == FittingPose.Single) {
				CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
			} else {
				CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
			}
		}

	}
}

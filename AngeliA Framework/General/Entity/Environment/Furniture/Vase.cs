using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace AngeliaFramework {
	public class Vase : Furniture {

		protected override Direction3 ModuleType => Direction3.Vertical;

		public override void FillPhysics () {
			if (Pose == FittingPose.Up || Pose == FittingPose.Single) {
				CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
			} else {
				CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
			}
		}

	}
}

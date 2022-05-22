using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eVase : eFurniture {

		private static readonly int[] CODES_DOWN = new int[] { "Vase Down 0".AngeHash(), };
		private static readonly int[] CODES_MID = new int[] { "Vase Mid 0".AngeHash(), };
		private static readonly int[] CODES_UP = new int[] { "Vase Up 0".AngeHash(), };
		private static readonly int[] CODES_SINGLE = new int[] { "Vase Single 0".AngeHash(), };

		protected override Direction3 ModuleType => Direction3.Vertical;
		protected override int[] ArtworkCodes_LeftDown => CODES_DOWN;
		protected override int[] ArtworkCodes_Mid => CODES_MID;
		protected override int[] ArtworkCodes_RightUp => CODES_UP;
		protected override int[] ArtworkCodes_Single => CODES_SINGLE;

		public override void FillPhysics () {
			if (Pose == FurniturePose.Up || Pose == FurniturePose.Single) {
				CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true, Const.ONEWAY_UP_TAG);
			} else {
				CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true);
			}
		}

	}
}

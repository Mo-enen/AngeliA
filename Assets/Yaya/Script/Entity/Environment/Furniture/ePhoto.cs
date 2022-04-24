using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePhoto : eFurniture {

		private static readonly int[] CODES = new int[] { "Photo 0".AngeHash(), "Photo 1".AngeHash(), "Photo 2".AngeHash(), "Photo 3".AngeHash(), };

		protected override Direction3 ModuleType => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => CODES;

		public override void FillPhysics (int frame) {
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this, true);
		}

	}
}

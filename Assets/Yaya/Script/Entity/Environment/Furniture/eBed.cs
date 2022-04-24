using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBed : eFurniture {

		private static readonly int[] CODES_LEFT = new int[] { "Bed Left 0".AngeHash(), "Bed Left 1".AngeHash(), };
		private static readonly int[] CODES_MID = new int[] { "Bed Mid 0".AngeHash(), "Bed Mid 1".AngeHash(), };
		private static readonly int[] CODES_RIGHT = new int[] { "Bed Right 0".AngeHash(), "Bed Right 1".AngeHash(), };
		private static readonly int[] CODES_SINGLE = new int[] { "Bed Single 0".AngeHash(), "Bed Single 1".AngeHash(), };

		protected override Direction3 ModuleType => Direction3.Horizontal;
		protected override int[] ArtworkCodes_LeftDown => CODES_LEFT;
		protected override int[] ArtworkCodes_Mid => CODES_MID;
		protected override int[] ArtworkCodes_RightUp => CODES_RIGHT;
		protected override int[] ArtworkCodes_Single => CODES_SINGLE;

	}
}

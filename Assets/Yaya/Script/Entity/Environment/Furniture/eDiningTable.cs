using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eDiningTable : eFurniture {

		private static readonly int[] CODES_LEFT = new int[] { "Dining Table Left 0".AngeHash(), "Dining Table Left 1".AngeHash(), "Dining Table Left 2".AngeHash(), "Dining Table Left 3".AngeHash(), };
		private static readonly int[] CODES_MID = new int[] { "Dining Table Mid 0".AngeHash(), "Dining Table Mid 1".AngeHash(), "Dining Table Mid 2".AngeHash(), "Dining Table Mid 3".AngeHash(), };
		private static readonly int[] CODES_RIGHT = new int[] { "Dining Table Right 0".AngeHash(), "Dining Table Right 1".AngeHash(), "Dining Table Right 2".AngeHash(), "Dining Table Right 3".AngeHash(), };
		private static readonly int[] CODES_SINGLE = new int[] { "Dining Table Single 0".AngeHash(), "Dining Table Single 1".AngeHash(), "Dining Table Single 2".AngeHash(), "Dining Table Single 3".AngeHash(), };

		protected override Direction3 Direction => Direction3.Horizontal;
		protected override int[] ArtworkCodes_LeftDown => CODES_LEFT;
		protected override int[] ArtworkCodes_Mid => CODES_MID;
		protected override int[] ArtworkCodes_RightUp => CODES_RIGHT;
		protected override int[] ArtworkCodes_Single => CODES_SINGLE;

	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eGrandfatherClock : eFurniture {

		private static readonly int[] CODES_DOWN = new int[] { "Grandfather Clock Down 0".AngeHash(), "Grandfather Clock Down 1".AngeHash(), };
		private static readonly int[] CODES_MID = new int[] { "Grandfather Clock Mid 0".AngeHash(), "Grandfather Clock Mid 1".AngeHash(), };
		private static readonly int[] CODES_UP = new int[] { "Grandfather Clock Up 0".AngeHash(), "Grandfather Clock Up 1".AngeHash(), };
		private static readonly int[] CODES_SINGLE = new int[] { "Grandfather Clock Single 0".AngeHash(), "Grandfather Clock Single 1".AngeHash(), };

		protected override Direction3 Direction => Direction3.Vertical;
		protected override int[] ArtworkCodes_LeftDown => CODES_DOWN;
		protected override int[] ArtworkCodes_Mid => CODES_MID;
		protected override int[] ArtworkCodes_RightUp => CODES_UP;
		protected override int[] ArtworkCodes_Single => CODES_SINGLE;

	}
}

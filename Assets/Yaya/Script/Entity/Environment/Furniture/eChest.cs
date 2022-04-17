using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eChest : eFurniture {

		private static readonly int[] CODES = new int[] { "Chest 0".AngeHash(), "Chest 1".AngeHash(), };
		private static readonly int[] CODES_OPEN = new int[] { "Chest 0 Open".AngeHash(), "Chest 1 Open".AngeHash(), };

		protected override Direction3 Direction => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => Opening ? CODES_OPEN : CODES;

		private bool Opening = false;

	}
}

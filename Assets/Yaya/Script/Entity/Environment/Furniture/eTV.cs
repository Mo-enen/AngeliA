using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eTV : eFurniture {

		private static readonly int[] CODES = new int[] { "TV 0".AngeHash(), "TV 1".AngeHash(), };

		protected override Direction3 Direction => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => CODES;

	}
}

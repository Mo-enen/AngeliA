using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eStoveCabinet : eFurniture {

		private static readonly int[] CODES = new int[] {
			"Stove Cabinet 0".AngeHash(), "Stove Cabinet 1".AngeHash(), "Stove Cabinet 2".AngeHash(), "Stove Cabinet 3".AngeHash(),
			"Stove Cabinet 4".AngeHash(), "Stove Cabinet 5".AngeHash(), "Stove Cabinet 6".AngeHash(), "Stove Cabinet 7".AngeHash(),
		};

		protected override Direction3 Direction => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => CODES;

	}
}

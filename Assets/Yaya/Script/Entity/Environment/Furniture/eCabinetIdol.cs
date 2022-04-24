using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eCabinetIdol : eFurniture {

		private static readonly int[] CODES = new int[] { "Cabinet Idol 0".AngeHash(), "Cabinet Idol 1".AngeHash(), "Cabinet Idol 2".AngeHash(), "Cabinet Idol 3".AngeHash(), "Cabinet Idol 4".AngeHash(), "Cabinet Idol 5".AngeHash(), };

		protected override Direction3 ModuleType => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => CODES;

	}
}

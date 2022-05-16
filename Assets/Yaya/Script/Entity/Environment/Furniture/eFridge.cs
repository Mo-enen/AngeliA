using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eFridge : eFurniture {

		private static readonly int[] CODES = new int[] { "Fridge 0".AngeHash(), };
		private static readonly int[] CODES_OPEN = new int[] { "Fridge 0 Open".AngeHash(), };

		protected override Direction3 ModuleType => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => Open ? CODES_OPEN : CODES;
		public bool Open { get; private set; } = false;

	}
}

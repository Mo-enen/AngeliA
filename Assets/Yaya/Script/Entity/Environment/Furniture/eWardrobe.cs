using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eWardrobe : eFurniture {


		private static readonly int[] CODES = new int[] { "Wardrobe 0".AngeHash(), "Wardrobe 1".AngeHash(), "Wardrobe 2".AngeHash(), "Wardrobe 3".AngeHash(), };
		private static readonly int[] CODES_OPEN = new int[] { "Wardrobe 0 Open".AngeHash(), "Wardrobe 1 Open".AngeHash(), "Wardrobe 2 Open".AngeHash(), "Wardrobe 3 Open".AngeHash(), };

		protected override Direction3 ModuleType => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => Open ? CODES_OPEN : CODES;
		public bool Open { get; private set; } = false;

	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eApple : eItem {

		private static readonly int[] CODES = new int[] { "".AngeHash(), "".AngeHash(), };
		private static readonly int[] CODES_CUT = new int[] { "".AngeHash(), "".AngeHash(), };

		protected override int ItemCode => Cut ? CODES_CUT[ArtworkIndex % CODES_CUT.Length] : CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;
		private bool Cut = false;



	}
}

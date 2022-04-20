using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eFish : eItem {

		private static readonly int[] CODES = new int[] { "Fish 0 Raw".AngeHash(), "Fish 1 Raw".AngeHash(), "Fish 2 Raw".AngeHash(), };
		private static readonly int[] CODES_COOKED = new int[] { "Fish 0 Cooked".AngeHash(), "Fish 1 Cooked".AngeHash(), "Fish 2 Cooked".AngeHash(), };
		protected override int ItemCode => Cooked ? CODES_COOKED[ArtworkIndex % CODES_COOKED.Length] : CODES[ArtworkIndex % CODES.Length];
		private bool Cooked = false;
		private int ArtworkIndex = 0;










	}
}

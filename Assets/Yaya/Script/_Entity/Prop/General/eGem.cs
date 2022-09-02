using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eGem : eItem {
		private static readonly int[] CODES = new int[] {
			"Gem Red".AngeHash(), "Gem Green".AngeHash(), "Gem Orange".AngeHash(), "Gem Grey".AngeHash(),
		};
		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;










	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eMushroom : eItem {

		private static readonly int[] CODES = new int[] { "Mushroom Red".AngeHash(), "Mushroom Green".AngeHash(), "Mushroom Blue".AngeHash(), "Mushroom Orange".AngeHash(), };
		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];
		private int ArtworkIndex = 0;










	}
}

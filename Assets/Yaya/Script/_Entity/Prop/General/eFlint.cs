using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eFlint : eItem {
		private static readonly int[] CODES = new int[] { "Flint 0".AngeHash(), "Flint 1".AngeHash(), "Flint 2".AngeHash(), };
		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;










	}
}

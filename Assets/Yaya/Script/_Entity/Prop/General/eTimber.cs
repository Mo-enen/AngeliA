using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eTimber : eItem {
		private static readonly int[] CODES = new int[] { "Timber 0".AngeHash(), "Timber 1".AngeHash(), "Timber 2".AngeHash(), "Timber 3".AngeHash(), };
		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;










	}
}

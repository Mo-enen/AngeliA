using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eKey : eItem {
		private static readonly int[] CODES = new int[] { "Key 0".AngeHash(), "Key 1".AngeHash(), "Key 2".AngeHash(), };

		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;










	}
}

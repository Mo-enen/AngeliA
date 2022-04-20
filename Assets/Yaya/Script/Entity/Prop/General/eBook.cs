using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eBook : eItem {

		private static readonly int[] CODES = new int[] { "Book 0".AngeHash(), "Book 1".AngeHash(), "Book 2".AngeHash(), "Book 3".AngeHash(), };
		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;


	}
}

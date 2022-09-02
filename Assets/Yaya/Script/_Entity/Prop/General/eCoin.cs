using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eCoin : eItem {
		private static readonly int[] CODES = new int[] {
			"Coin 0".AngeHash(), "Coin 1".AngeHash(), "Coin 2".AngeHash(), "Coin 3".AngeHash(),
			"Coin 4".AngeHash(), "Coin 5".AngeHash(), "Coin 6".AngeHash(), "Coin 7".AngeHash(),
		};

		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;



	}
}

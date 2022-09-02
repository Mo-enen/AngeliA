using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eVolPotion : eItem {


		private static readonly int[] CODES = new int[] { "Vol Potion Red".AngeHash(), "Vol Potion Blue".AngeHash(), "Vol Potion Orange".AngeHash(), };
		private static readonly int EMPTY_CODE = "Vol Potion Empty".AngeHash();

		protected override int ItemCode => HasContent ? CODES[ArtworkIndex % CODES.Length] : EMPTY_CODE;

		private int ArtworkIndex = 0;
		private bool HasContent = true;




	}
}

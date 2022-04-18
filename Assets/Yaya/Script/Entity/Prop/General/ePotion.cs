using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePotion : eItem {


		private static readonly int[] CODES = new int[] { "Potion Red".AngeHash(), "Potion Blue".AngeHash(), "Potion Orange".AngeHash(), };
		private static readonly int EMPTY_CODE = "Potion Empty".AngeHash();

		protected override int ItemCode => HasContent ? CODES[ArtworkIndex % CODES.Length] : EMPTY_CODE;

		private int ArtworkIndex = 0;
		private bool HasContent = true;




	}
}

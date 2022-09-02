using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eRune : eItem {
		private static readonly int[] CODES = new int[] {
			"Rune Fire".AngeHash(), "Rune Water".AngeHash(), "Rune Lightning".AngeHash(), "Rune Poison".AngeHash(),
		};
		private static readonly int CODE_EMPTY = "Rune Empty".AngeHash();

		protected override int ItemCode => HasContent ? CODES[ArtworkIndex % CODES.Length] : CODE_EMPTY;

		private int ArtworkIndex = 0;
		private bool HasContent = true;









	}
}

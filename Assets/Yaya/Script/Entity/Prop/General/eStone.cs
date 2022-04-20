using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eStone : eItem {
		private static readonly int[] CODES = new int[] { "Stone 0".AngeHash(), "Stone 1".AngeHash(), "Stone 2".AngeHash(), };
		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;









	}
}

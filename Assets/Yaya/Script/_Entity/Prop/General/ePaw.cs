using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class ePaw : eItem {
		private static readonly int[] CODES = new int[] {
			"Cat Paw".AngeHash(), "Bear Paw".AngeHash(), 
		};
		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;










	}
}

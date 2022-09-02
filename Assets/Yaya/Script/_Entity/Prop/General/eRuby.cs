using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eRuby : eItem {
		private static readonly int[] CODES = new int[] {
			"Ruby Red".AngeHash(), "Ruby Green".AngeHash(), "Ruby Orange".AngeHash(), "Ruby Grey".AngeHash(),
		};
		protected override int ItemCode => CODES[ArtworkIndex % CODES.Length];

		private int ArtworkIndex = 0;










	}
}

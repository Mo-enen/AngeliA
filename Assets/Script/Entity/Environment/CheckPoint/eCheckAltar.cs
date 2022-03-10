using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eCheckAltar : eCheckPoint {


		private static readonly int[] ARTWORK_CODES = new int[] { "Check Altar 0".AngeHash(), "Check Altar 1".AngeHash(), "Check Altar 2".AngeHash(), "Check Altar 3".AngeHash(), "Check Altar 4".AngeHash(), "Check Altar 5".AngeHash(), "Check Altar 6".AngeHash(), "Check Altar 7".AngeHash(), "Check Altar 8".AngeHash(), "Check Altar 9".AngeHash(), "Check Altar 10".AngeHash(), "Check Altar 11".AngeHash(), "Check Altar 12".AngeHash(), "Check Altar 13".AngeHash(), "Check Altar 14".AngeHash(), "Check Altar 15".AngeHash(), "Check Altar 16".AngeHash(), "Check Altar 17".AngeHash(), "Check Altar 18".AngeHash(), "Check Altar 19".AngeHash(), "Check Altar 20".AngeHash(), "Check Altar 21".AngeHash(), "Check Altar 22".AngeHash(), "Check Altar 23".AngeHash(), };
		protected override int ArtCode => Data >= 0 && Data < ARTWORK_CODES.Length ? ARTWORK_CODES[Data] : 0;




	}
}

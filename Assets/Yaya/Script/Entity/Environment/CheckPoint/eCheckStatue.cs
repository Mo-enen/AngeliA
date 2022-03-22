using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eCheckStatue : eCheckPoint {


		private static readonly int[] ARTWORK_STATUE_CODES = new int[] { "Check Statue 0".AngeHash(), "Check Statue 1".AngeHash(), "Check Statue 2".AngeHash(), "Check Statue 3".AngeHash(), "Check Statue 4".AngeHash(), "Check Statue 5".AngeHash(), "Check Statue 6".AngeHash(), "Check Statue 7".AngeHash(), "Check Statue 8".AngeHash(), "Check Statue 9".AngeHash(), "Check Statue 10".AngeHash(), "Check Statue 11".AngeHash(), "Check Statue 12".AngeHash(), "Check Statue 13".AngeHash(), "Check Statue 14".AngeHash(), "Check Statue 15".AngeHash(), "Check Statue 16".AngeHash(), "Check Statue 17".AngeHash(), "Check Statue 18".AngeHash(), "Check Statue 19".AngeHash(), "Check Statue 20".AngeHash(), "Check Statue 21".AngeHash(), "Check Statue 22".AngeHash(), "Check Statue 23".AngeHash(), };
		protected override int ArtCode => Data >= 0 && Data < ARTWORK_STATUE_CODES.Length ? ARTWORK_STATUE_CODES[Data] : 0;



	}
}

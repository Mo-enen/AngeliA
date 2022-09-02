using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eApricot : eItem {

		private static readonly int CODE = "Apricot".AngeHash();
		private static readonly int CODE_CUT = "Apricot Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;









	}
}

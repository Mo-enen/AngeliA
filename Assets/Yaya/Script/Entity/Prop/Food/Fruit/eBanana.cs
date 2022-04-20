using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eBanana : eItem {

		private static readonly int CODE = "Banana".AngeHash();
		private static readonly int CODE_CUT = "Banana Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;









	}
}

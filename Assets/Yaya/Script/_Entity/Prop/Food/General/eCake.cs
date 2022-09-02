using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eCake : eItem {
		private static readonly int CODE = "Cake 0".AngeHash();
		private static readonly int CODE_OIL = "Cake 1".AngeHash();
		protected override int ItemCode => Oil ? CODE_OIL : CODE;
		public bool Oil = false;


	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eBread : eItem {
		private static readonly int CODE = "Bread 0".AngeHash();
		private static readonly int CODE_OIL = "Bread 1".AngeHash();
		protected override int ItemCode => Oil ? CODE_OIL : CODE;
		public bool Oil = false;


	}
}

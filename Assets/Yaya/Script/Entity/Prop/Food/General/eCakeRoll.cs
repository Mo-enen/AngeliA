using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eCakeRoll : eItem {
		private static readonly int CODE = "Cake Roll 0".AngeHash();
		private static readonly int CODE_OIL = "Cake Roll 1".AngeHash();
		protected override int ItemCode => Oil ? CODE_OIL : CODE;
		public bool Oil = false;




	}
}

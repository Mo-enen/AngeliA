using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eBeef : eItem {
		private static readonly int CODE = "Beef Raw".AngeHash();
		private static readonly int CODE_COOKED = "Beef Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;





	}
}
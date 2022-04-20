using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eSteak : eItem {
		private static readonly int CODE = "Steak Raw".AngeHash();
		private static readonly int CODE_COOKED = "Steak Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;











	}
}

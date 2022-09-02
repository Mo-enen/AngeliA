using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eSausage : eItem {
		private static readonly int CODE = "Sausage Raw".AngeHash();
		private static readonly int CODE_COOKED = "Sausage Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;











	}
}

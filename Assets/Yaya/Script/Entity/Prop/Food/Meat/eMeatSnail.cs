using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eMeatSnail : eItem {
		private static readonly int CODE = "Snail Raw".AngeHash();
		private static readonly int CODE_COOKED = "Snail Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;











	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eMeatball : eItem {
		private static readonly int CODE = "Meatball Raw".AngeHash();
		private static readonly int CODE_COOKED = "Meatball Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;











	}
}

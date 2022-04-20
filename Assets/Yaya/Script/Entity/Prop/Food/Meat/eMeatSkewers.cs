using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eMeatSkewers : eItem {
		private static readonly int CODE = "Meat Skewers Raw".AngeHash();
		private static readonly int CODE_COOKED = "Meat Skewers Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;











	}
}

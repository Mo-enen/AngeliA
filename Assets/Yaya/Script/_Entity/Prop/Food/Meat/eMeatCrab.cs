using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eMeatCrab : eItem {
		private static readonly int CODE = "Crab Raw".AngeHash();
		private static readonly int CODE_COOKED = "Crab Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;











	}
}

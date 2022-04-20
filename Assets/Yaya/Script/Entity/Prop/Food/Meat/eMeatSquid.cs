using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eMeatSquid : eItem {
		private static readonly int CODE = "Squid Raw".AngeHash();
		private static readonly int CODE_COOKED = "Squid Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;











	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eLemon : eItem {
		private static readonly int CODE = "Lemon".AngeHash();
		private static readonly int CODE_CUT = "Lemon Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;









	}
}

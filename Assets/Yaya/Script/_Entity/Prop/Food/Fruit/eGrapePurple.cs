using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eGrapePurple : eItem {
		private static readonly int CODE = "Grape Purple".AngeHash();
		private static readonly int CODE_CUT = "Grape Purple Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;










	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class ePeach : eItem {
		private static readonly int CODE = "Peach".AngeHash();
		private static readonly int CODE_CUT = "Peach Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;










	}
}

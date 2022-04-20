using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class ePear : eItem {
		private static readonly int CODE = "Pear".AngeHash();
		private static readonly int CODE_CUT = "Pear Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;










	}
}

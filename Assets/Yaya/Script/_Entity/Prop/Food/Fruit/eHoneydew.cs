using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eHoneydew : eItem {
		private static readonly int CODE = "Honeydew".AngeHash();
		private static readonly int CODE_CUT = "Honeydew Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;









	}
}

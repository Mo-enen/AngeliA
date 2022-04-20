using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eHamburger : eItem {
		private static readonly int CODE = "Hamburger 0".AngeHash();
		private static readonly int CODE_OIL = "Hamburger 1".AngeHash();
		protected override int ItemCode => Oil ? CODE_OIL : CODE;
		public bool Oil = false;









	}
}

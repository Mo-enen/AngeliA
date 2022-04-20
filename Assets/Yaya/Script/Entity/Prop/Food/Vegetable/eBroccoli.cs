using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eBroccoli : eItem {
		private static readonly int CODE = "Broccoli".AngeHash();
		protected override int ItemCode => CODE;



	}
}

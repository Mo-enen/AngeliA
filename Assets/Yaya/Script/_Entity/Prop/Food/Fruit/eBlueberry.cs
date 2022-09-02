using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eBlueberry : eItem {
		private static readonly int CODE = "Blueberry".AngeHash();
		private static readonly int CODE_CUT = "Blueberry Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;


	}
}

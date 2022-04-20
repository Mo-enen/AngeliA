using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	public class eAppleGreen : eItem {

		private static readonly int CODE = "Apple Green".AngeHash();
		private static readonly int CODE_CUT = "Apple Green Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;


	}
}

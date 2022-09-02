using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eAppleRed : eItem {


		private static readonly int CODE = "Apple Red".AngeHash();
		private static readonly int CODE_CUT = "Apple Red Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;



	}
}

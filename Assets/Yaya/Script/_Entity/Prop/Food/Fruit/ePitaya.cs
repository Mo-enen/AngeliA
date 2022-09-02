using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class ePitaya : eItem {
		private static readonly int CODE = "Pitaya".AngeHash();
		private static readonly int CODE_CUT = "Pitaya Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;









	}
}

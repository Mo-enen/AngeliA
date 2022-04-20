using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eOrange : eItem {
		private static readonly int CODE = "Orange".AngeHash();
		private static readonly int CODE_CUT = "Orange Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;










	}
}

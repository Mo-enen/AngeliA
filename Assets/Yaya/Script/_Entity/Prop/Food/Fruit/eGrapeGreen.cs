using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eGrapeGreen : eItem {
		private static readonly int CODE = "Grape Green".AngeHash();
		private static readonly int CODE_CUT = "Grape Green Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;










	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eStrawberry : eItem {
		private static readonly int CODE = "Strawberry".AngeHash();
		private static readonly int CODE_CUT = "Strawberry Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;










	}
}

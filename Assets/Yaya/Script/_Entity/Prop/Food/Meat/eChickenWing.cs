using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eChickenWing : eItem {
		private static readonly int CODE = "Chicken Wing Raw".AngeHash();
		private static readonly int CODE_COOKED = "Chicken Wing Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;



	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eChickenLeg : eItem {

		private static readonly int CODE = "Chicken Leg Raw".AngeHash();
		private static readonly int CODE_COOKED = "Chicken Leg Cooked".AngeHash();
		protected override int ItemCode => Cooked ? CODE_COOKED : CODE;
		private bool Cooked = false;



	}
}

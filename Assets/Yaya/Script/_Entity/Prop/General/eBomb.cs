using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eBomb : eItem {
		private static readonly int CODE = "Bomb".AngeHash(); 
		protected override int ItemCode => CODE;



	}
}

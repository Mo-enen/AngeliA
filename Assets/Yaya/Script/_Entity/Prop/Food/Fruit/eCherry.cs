using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eCherry : eItem {
		private static readonly int CODE = "Cherry".AngeHash();
		private static readonly int CODE_CUT = "Cherry Cut".AngeHash();

		protected override int ItemCode => Cut ? CODE_CUT : CODE;
		private bool Cut = false;


	}
}

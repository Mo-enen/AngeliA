using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eDumpling : eItem {
		private static readonly int CODE = "Dumpling".AngeHash();
		protected override int ItemCode => CODE;



	}
}

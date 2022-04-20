using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eCucumber : eItem {
		private static readonly int CODE = "Cucumber".AngeHash();
		protected override int ItemCode => CODE;



	}
}

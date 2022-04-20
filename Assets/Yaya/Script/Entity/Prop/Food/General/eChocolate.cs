using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eChocolate : eItem {
		private static readonly int CODE = "Chocolate".AngeHash();
		protected override int ItemCode => CODE;



	}
}

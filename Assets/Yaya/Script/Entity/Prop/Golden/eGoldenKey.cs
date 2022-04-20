using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eGoldenKey : eGoldenItem {
		private static readonly int CODE = "Golden Key".AngeHash();
		protected override int ItemCode => CODE;











	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AngeliaFramework;
namespace Yaya {
	public class eMeatBone : eItem {
		private static readonly int CODE = "Meat Bone 0".AngeHash();
		private static readonly int CODE_FISH = "Meat Bone 1".AngeHash();
		protected override int ItemCode => Fish ? CODE_FISH : CODE;
		private bool Fish = false;











	}
}

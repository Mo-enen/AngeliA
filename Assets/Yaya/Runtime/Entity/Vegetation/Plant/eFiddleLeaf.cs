using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL)]
	public class eFiddleLeaf : ePlant {


		// Api
		protected override int ArtworkCode => "Fiddle Leaf".AngeHash();


	}
}

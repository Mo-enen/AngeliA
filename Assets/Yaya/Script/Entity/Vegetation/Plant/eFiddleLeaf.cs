using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	[EntityAttribute.EntityBounds(0, 0, Const.CELL_SIZE * 2, Const.CELL_SIZE)]
	public class eFiddleLeaf : ePlant {


		// Api
		protected override int ArtworkCode => "Fiddle Leaf".AngeHash();


	}
}

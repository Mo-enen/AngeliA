using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityBounds(0, 0, Const.CELL_SIZE * 2, Const.CELL_SIZE * 2)]
	public class eCoral : ePlant {


		protected override int ArtworkCode => "Coral".AngeHash();


	}
}

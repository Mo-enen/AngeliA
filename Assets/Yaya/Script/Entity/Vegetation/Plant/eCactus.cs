using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	[EntityAttribute.EntityBounds(0, 0, Const.CELL_SIZE * 2, Const.CELL_SIZE * 2)]
	public class eCactus : ePlant {


		protected override int ArtworkCode => "Cactus".AngeHash();
		protected override bool Breath => false;


	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityBounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	public class ePaspalum : ePlant {


		// Api
		protected override int ArtworkCode => "Paspalum".AngeHash();


	}
}

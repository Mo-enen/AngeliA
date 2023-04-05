using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	[EntityAttribute.Bounds(0, 0, Const.CEL * 2, Const.CEL * 2)]
	public class eCactus : Plant {


		protected override int ArtworkCode => "Cactus".AngeHash();
		protected override bool Breath => false;


	}
}

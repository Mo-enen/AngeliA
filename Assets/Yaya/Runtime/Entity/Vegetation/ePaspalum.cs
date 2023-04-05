using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	public class ePaspalum : Plant {


		// Api
		protected override int ArtworkCode => "Paspalum".AngeHash();


	}
}

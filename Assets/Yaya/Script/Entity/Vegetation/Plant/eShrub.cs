using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eShrub : ePlant {


		// Api
		protected override int ArtworkCode => "Shrub".AngeHash();
		protected override RangeInt SizeOffset => new(12, 24);


	}
}

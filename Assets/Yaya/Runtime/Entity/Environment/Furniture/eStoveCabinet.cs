using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eStoveCabinet : Furniture {


		protected override bool LoopArtworkIndex => true;


		public override void OnActivated () {
			base.OnActivated();
			ArtworkIndex = (X + Y * 17) / Const.CEL;
		}



	}
}

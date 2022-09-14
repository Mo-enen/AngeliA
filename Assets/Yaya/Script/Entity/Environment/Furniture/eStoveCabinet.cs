using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eStoveCabinet : eFurniture {


		protected override bool LoopArtworkIndex => true;


		public override void OnActived () {
			base.OnActived();
			ArtworkIndex = (X + Y * 17) / Const.CELL_SIZE;
		}



	}
}

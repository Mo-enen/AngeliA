using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eStoveCabinet : eFurniture {


		private static readonly int CODE = "Stove Cabinet".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => CODE;
		protected override bool LoopArtworkIndex => true;


		public override void OnActived () {
			base.OnActived();
			ArtworkIndex = (X + Y * 17) / Const.CELL_SIZE;
		}



	}
}

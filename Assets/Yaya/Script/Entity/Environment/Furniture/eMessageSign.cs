using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eMessageSign : eFurniture {

		// Single
		private static readonly int CODE_DOWN = "Message Sign Down".AngeHash();
		private static readonly int CODE_MID = "Message Sign Mid".AngeHash();
		private static readonly int CODE_UP = "Message Sign Up".AngeHash();
		private static readonly int CODE_SINGLE = "Message Sign Single".AngeHash();

		protected override Direction3 ModuleType => Direction3.Vertical;
		protected override int ArtworkCode_LeftDown => CODE_DOWN;
		protected override int ArtworkCode_Mid => CODE_MID;
		protected override int ArtworkCode_RightUp => CODE_UP;
		protected override int ArtworkCode_Single => CODE_SINGLE;


		public override void FillPhysics () {
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}



	}
}

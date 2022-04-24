using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eMessageSign : eFurniture {

		// Single
		private static readonly int[] CODES_DOWN = new int[] { "Message Sign Down".AngeHash(), };
		private static readonly int[] CODES_MID = new int[] { "Message Sign Mid".AngeHash(), };
		private static readonly int[] CODES_UP = new int[] { "Message Sign Up".AngeHash(), };
		private static readonly int[] CODES_SINGLE = new int[] { "Message Sign Single".AngeHash(), };

		protected override Direction3 ModuleType => Direction3.Vertical;
		protected override int[] ArtworkCodes_LeftDown => CODES_DOWN;
		protected override int[] ArtworkCodes_Mid => CODES_MID;
		protected override int[] ArtworkCodes_RightUp => CODES_UP;
		protected override int[] ArtworkCodes_Single => CODES_SINGLE;


		public override void FillPhysics (int frame) {
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this, true);
		}



	}
}

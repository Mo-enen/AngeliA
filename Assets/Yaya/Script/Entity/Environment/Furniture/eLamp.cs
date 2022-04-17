using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eLamp : eFurniture {

		private static readonly int[] CODES = new int[] { "Lamp 0".AngeHash(), "Lamp 1".AngeHash(), "Lamp 2".AngeHash(), "Lamp 3".AngeHash(), };

		protected override Direction3 Direction => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => CODES;

		public override void FillPhysics (int frame) {
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this, true);
		}

	}
}

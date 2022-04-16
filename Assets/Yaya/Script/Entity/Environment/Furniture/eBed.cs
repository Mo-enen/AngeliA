using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBed : eModularFurniture {



		// Const
		private static readonly int[] CODES_LEFT = new int[] { "Bed Left 0".AngeHash(), "Bed Left 1".AngeHash(), };
		private static readonly int[] CODES_MID = new int[] { "Bed Mid 0".AngeHash(), "Bed Mid 1".AngeHash(), };
		private static readonly int[] CODES_RIGHT = new int[] { "Bed Right 0".AngeHash(), "Bed Right 1".AngeHash(), };
		private static readonly int[] CODES_SINGLE = new int[] { "Bed Single 0".AngeHash(), "Bed Single 1".AngeHash(), };

		// Api
		public override int Capacity => 32;
		protected override Direction2 Direction => Direction2.Horizontal;

		// Data
		private int ArtworkIndex = 0;



		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this, true, Const.ONEWAY_UP_TAG);
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			if (Pose != FurniturePose.Unknown) CellRenderer.Draw(Pose switch {
				FurniturePose.Left => CODES_LEFT[ArtworkIndex % CODES_LEFT.Length],
				FurniturePose.Mid => CODES_MID[ArtworkIndex % CODES_MID.Length],
				FurniturePose.Right => CODES_RIGHT[ArtworkIndex % CODES_RIGHT.Length],
				FurniturePose.Single => CODES_SINGLE[ArtworkIndex % CODES_SINGLE.Length],
				_ => 0,
			}, Rect);
		}





	}
}

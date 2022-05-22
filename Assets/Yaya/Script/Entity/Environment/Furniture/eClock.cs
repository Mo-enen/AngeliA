using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eClock : eFurniture {

		private static readonly int[] CODES = new int[] { "Clock 0".AngeHash(), "Clock 1".AngeHash(), "Clock 2".AngeHash(), "Clock 3".AngeHash(), };
		private static readonly int HAND_CODE = "Clock Hand".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int[] ArtworkCodes_LeftDown => CODES;
		protected override int[] ArtworkCodes_Mid => CODES;
		protected override int[] ArtworkCodes_RightUp => CODES;
		protected override int[] ArtworkCodes_Single => CODES;


		public override void FillPhysics () {
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawClockHands(Rect.Shrink(8), HAND_CODE, 20, 10);
		}


	}
}

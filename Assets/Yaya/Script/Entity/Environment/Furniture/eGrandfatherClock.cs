using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eGrandfatherClock : eFurniture {

		private static readonly int[] CODES_DOWN = new int[] { "Grandfather Clock Down 0".AngeHash(), "Grandfather Clock Down 1".AngeHash(), };
		private static readonly int[] CODES_MID = new int[] { "Grandfather Clock Mid 0".AngeHash(), "Grandfather Clock Mid 1".AngeHash(), };
		private static readonly int[] CODES_UP = new int[] { "Grandfather Clock Up 0".AngeHash(), "Grandfather Clock Up 1".AngeHash(), };
		private static readonly int[] CODES_SINGLE = new int[] { "Grandfather Clock Single 0".AngeHash(), "Grandfather Clock Single 1".AngeHash(), };
		private static readonly int HAND_CODE = "Clock Hand".AngeHash();

		protected override Direction3 ModuleType => Direction3.Vertical;
		protected override int[] ArtworkCodes_LeftDown => CODES_DOWN;
		protected override int[] ArtworkCodes_Mid => CODES_MID;
		protected override int[] ArtworkCodes_RightUp => CODES_UP;
		protected override int[] ArtworkCodes_Single => CODES_SINGLE;


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			if (Pose == FurniturePose.Up) {
				DrawClockHands(Rect.Shrink(36), HAND_CODE, 16, 8);
			} else if (Pose == FurniturePose.Single) {
				DrawClockHands(Rect.Shrink(36).Shift(0, 24), HAND_CODE, 16, 8);
			}
		}


	}
}

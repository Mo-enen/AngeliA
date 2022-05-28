using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eGrandfatherClock : eFurniture {

		private static readonly int CODE_DOWN = "Grandfather Clock Down".AngeHash();
		private static readonly int CODE_MID = "Grandfather Clock Mid".AngeHash();
		private static readonly int CODE_UP = "Grandfather Clock Up".AngeHash();
		private static readonly int CODE_SINGLE = "Grandfather Clock Single".AngeHash();
		private static readonly int HAND_CODE = "Clock Hand".AngeHash();

		protected override Direction3 ModuleType => Direction3.Vertical;
		protected override int ArtworkCode_LeftDown => CODE_DOWN;
		protected override int ArtworkCode_Mid => CODE_MID;
		protected override int ArtworkCode_RightUp => CODE_UP;
		protected override int ArtworkCode_Single => CODE_SINGLE;


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Pose == FurniturePose.Up) {
				DrawClockHands(Rect.Shrink(36), HAND_CODE, 16, 8);
			} else if (Pose == FurniturePose.Single) {
				DrawClockHands(Rect.Shrink(36).Shift(0, 24), HAND_CODE, 16, 8);
			}
		}


	}
}

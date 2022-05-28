using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eClock : eFurniture {

		private static readonly int CODE = "Clock".AngeHash();
		private static readonly int HAND_CODE = "Clock Hand".AngeHash();

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => CODE;
		protected override int ArtworkCode_Mid => CODE;
		protected override int ArtworkCode_RightUp => CODE;
		protected override int ArtworkCode_Single => CODE;


		public override void FillPhysics () {
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawClockHands(Rect.Shrink(8), HAND_CODE, 20, 10);
		}


	}
}

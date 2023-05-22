using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	public class eClockGreen : eClock, ICombustible {
		private static readonly int CODE = "Clock 0".AngeHash();
		protected override int ArtworkCode => CODE;
		int ICombustible.BurnStartFrame { get; set; }
	}

	public class eClockPurple : eClock, ICombustible {
		private static readonly int CODE = "Clock 1".AngeHash();
		protected override int ArtworkCode => CODE;
		int ICombustible.BurnStartFrame { get; set; }
	}

	public abstract class eClock : Furniture {

		private static readonly int HAND_CODE = "Clock Hand".AngeHash();
		protected abstract int ArtworkCode { get; }

		protected override Direction3 ModuleType => Direction3.None;
		protected override int ArtworkCode_LeftDown => ArtworkCode;
		protected override int ArtworkCode_Mid => ArtworkCode;
		protected override int ArtworkCode_RightUp => ArtworkCode;
		protected override int ArtworkCode_Single => ArtworkCode;


		public override void FillPhysics () {
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawClockHands(Rect.Shrink(8), HAND_CODE, 20, 10);
		}


	}
}

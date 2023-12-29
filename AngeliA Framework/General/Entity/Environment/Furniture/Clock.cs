using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {

	public class ClockGreen : Clock, ICombustible {
		private static readonly int CODE = "Clock 0".AngeHash();
		protected override int ArtworkCode => CODE;
		int ICombustible.BurnStartFrame { get; set; }
	}

	public class ClockPurple : Clock, ICombustible {
		private static readonly int CODE = "Clock 1".AngeHash();
		protected override int ArtworkCode => CODE;
		int ICombustible.BurnStartFrame { get; set; }
	}

	public abstract class Clock : Furniture {

		private static readonly int HAND_CODE = "Clock Hand".AngeHash();
		protected abstract int ArtworkCode { get; }

		protected override Direction3 ModuleType => Direction3.None;

		public override void FillPhysics () {
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}

		public override void FrameUpdate () {
			base.FrameUpdate();
			DrawClockHands(Rect.Shrink(8), HAND_CODE, 20, 10);
		}

	}
}

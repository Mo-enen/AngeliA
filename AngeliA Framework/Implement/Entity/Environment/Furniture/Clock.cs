using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {

	public class ClockGreen : Clock, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}

	public class ClockPurple : Clock, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}


	[RequireSpriteFromField]
	public abstract class Clock : Furniture {

		private static readonly SpriteCode HAND_CODE = "Clock Hand";

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

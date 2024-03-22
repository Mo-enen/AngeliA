using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 

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

	public override void FirstUpdate () {
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}

	public override void LateUpdate () {
		base.LateUpdate();
		DrawClockHands(Rect.Shrink(8), HAND_CODE, 20, 10);
	}

}

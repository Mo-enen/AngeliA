using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class BarrelIron : BarrelVehicle {

	public override void OnActivated () {
		base.OnActivated();
		const int SIZE_DELTA = 16;
		Width -= SIZE_DELTA;
		Height -= SIZE_DELTA;
		X += SIZE_DELTA / 2;
	}

}

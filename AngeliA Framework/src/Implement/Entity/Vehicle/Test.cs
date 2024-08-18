using System.Collections;
using System.Collections.Generic;

namespace AngeliA;




public class TestM : VehicleMovement {
	public TestM (Rigidbody rig) : base(rig) { }
}


public class TestV : Vehicle<TestM> {

	public override void OnActivated () {
		Width = 48 * Const.ART_SCALE;
		Height = 32 * Const.ART_SCALE;
		base.OnActivated();
	}

	protected override void LateUpdateVehicle () {
		base.LateUpdateVehicle();

		Renderer.Draw(TypeID, Rect, 1024);

	}


}


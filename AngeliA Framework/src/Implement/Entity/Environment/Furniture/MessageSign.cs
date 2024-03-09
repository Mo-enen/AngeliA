using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class MessageSign : Furniture, ICombustible {

	protected override Direction3 ModuleType => Direction3.Vertical;
	int ICombustible.BurnStartFrame { get; set; }

	public override void FirstUpdate () {
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}

}

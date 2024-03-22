using System.Collections;
using System.Collections.Generic;

namespace AngeliA; 
public class Vase : Furniture {

	protected override Direction3 ModuleType => Direction3.Vertical;

	public override void FirstUpdate () {
		if (Pose == FittingPose.Up || Pose == FittingPose.Single) {
			Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true, SpriteTag.ONEWAY_UP_TAG);
		} else {
			Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}
	}

}

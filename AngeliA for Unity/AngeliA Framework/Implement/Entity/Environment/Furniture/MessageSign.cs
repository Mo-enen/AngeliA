using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class MessageSign : Furniture, ICombustible {

		protected override Direction3 ModuleType => Direction3.Vertical;
		int ICombustible.BurnStartFrame { get; set; }

		public override void FillPhysics () {
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
		}

	}
}

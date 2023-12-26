using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class Barrel : Breakable, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
		protected override bool PhysicsEnable => true;
	}
}

using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {


	public class SpringWoodHorizontal : Spring, ICombustible {
		protected override bool Horizontal => true;
		protected override int Power => 64;
		int ICombustible.BurnStartFrame { get; set; }
	}


	public class SpringWoodVertical : Spring, ICombustible {
		protected override bool Horizontal => false;
		protected override int Power => 64;
		int ICombustible.BurnStartFrame { get; set; }
	}


	public class SpringMetalHorizontal : Spring {
		protected override bool Horizontal => true;
		protected override int Power => 128;
	}


	public class SpringMetalVertical : Spring {
		protected override bool Horizontal => false;
		protected override int Power => 128;
	}


}

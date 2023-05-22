using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaGame {


	public class eSpringWoodHorizontal : Spring, ICombustible {
		protected override bool Horizontal => true;
		protected override int Power => 64;
		int ICombustible.BurnStartFrame { get; set; }
	}


	public class eSpringWoodVertical : Spring, ICombustible {
		protected override bool Horizontal => false;
		protected override int Power => 64;
		int ICombustible.BurnStartFrame { get; set; }
	}


	public class eSpringMetalHorizontal : Spring {
		protected override bool Horizontal => true;
		protected override int Power => 128;
	}


	public class eSpringMetalVertical : Spring {
		protected override bool Horizontal => false;
		protected override int Power => 128;
	}


}

using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaFramework {


	public class DonutBlockDirt : DonutBlock {
		protected override BreakMode BreakCondition => BreakMode.BreakOnCollideGround;
	}


}

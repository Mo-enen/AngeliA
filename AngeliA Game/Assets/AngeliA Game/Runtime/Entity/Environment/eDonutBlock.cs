using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace AngeliaGame {


	public class eDonutBlockDirt : DonutBlock {
		protected override BreakMode BreakCondition => BreakMode.BreakOnCollideGround;
	}


}

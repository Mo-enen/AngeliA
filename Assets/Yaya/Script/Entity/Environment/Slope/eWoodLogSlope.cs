using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;

namespace Yaya {
	public class eWoodLogSlopeA : eSlope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Right;
	}


	public class eWoodLogSlopeB : eSlope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Left;
	}

}

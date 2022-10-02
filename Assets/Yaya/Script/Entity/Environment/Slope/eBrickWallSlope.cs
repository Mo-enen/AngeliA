using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;

namespace Yaya {
	public class eBrickWallSlopeA : eSlope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Right;
	}


	public class eBrickWallSlopeB : eSlope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Left;
	}


	public class eBrickWallSlopeC : eSlope {
		public override Direction3 DirectionVertical => Direction3.Down;
		public override Direction3 DirectionHorizontal => Direction3.Right;
	}


	public class eBrickWallSlopeD : eSlope {
		public override Direction3 DirectionVertical => Direction3.Down;
		public override Direction3 DirectionHorizontal => Direction3.Left;
	}
}

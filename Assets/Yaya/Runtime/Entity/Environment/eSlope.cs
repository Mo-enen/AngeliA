using AngeliaFramework;

namespace Yaya {


	public class eWoodLogSlopeA : Slope {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Right;
	}


	public class eWoodLogSlopeB : Slope {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Left;
	}


	public class eBrickWallSlopeA : Slope {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Right;
	}


	public class eBrickWallSlopeB : Slope {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Left;
	}


	public class eBrickWallSlopeC : Slope {
		public override Direction2 DirectionVertical => Direction2.Down;
		public override Direction2 DirectionHorizontal => Direction2.Right;
	}


	public class eBrickWallSlopeD : Slope {
		public override Direction2 DirectionVertical => Direction2.Down;
		public override Direction2 DirectionHorizontal => Direction2.Left;
	}


}

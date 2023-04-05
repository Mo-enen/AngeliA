using AngeliaFramework;

namespace Yaya {


	public class eWoodLogSlopeA : Slope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Right;
	}


	public class eWoodLogSlopeB : Slope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Left;
	}


	public class eBrickWallSlopeA : Slope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Right;
	}


	public class eBrickWallSlopeB : Slope {
		public override Direction3 DirectionVertical => Direction3.Up;
		public override Direction3 DirectionHorizontal => Direction3.Left;
	}


	public class eBrickWallSlopeC : Slope {
		public override Direction3 DirectionVertical => Direction3.Down;
		public override Direction3 DirectionHorizontal => Direction3.Right;
	}


	public class eBrickWallSlopeD : Slope {
		public override Direction3 DirectionVertical => Direction3.Down;
		public override Direction3 DirectionHorizontal => Direction3.Left;
	}


}

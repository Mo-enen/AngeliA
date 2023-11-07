using AngeliaFramework;

namespace AngeliaFramework {


	public class WoodLogSlopeA : Slope, ICombustible {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Right;
		int ICombustible.BurnStartFrame { get; set; }
	}


	public class WoodLogSlopeB : Slope, ICombustible {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Left;
		int ICombustible.BurnStartFrame { get; set; }
	}


	public class BrickWallSlopeA : Slope {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Right;
	}


	public class BrickWallSlopeB : Slope {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Left;
	}


	public class BrickWallSlopeC : Slope {
		public override Direction2 DirectionVertical => Direction2.Down;
		public override Direction2 DirectionHorizontal => Direction2.Right;
	}


	public class BrickWallSlopeD : Slope {
		public override Direction2 DirectionVertical => Direction2.Down;
		public override Direction2 DirectionHorizontal => Direction2.Left;
	}


}

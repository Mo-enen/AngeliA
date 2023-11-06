using AngeliaFramework;

namespace AngeliaGeneral {


	public class eWoodLogSlopeA : Slope, ICombustible {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Right;
		int ICombustible.BurnStartFrame { get; set; }
	}


	public class eWoodLogSlopeB : Slope, ICombustible {
		public override Direction2 DirectionVertical => Direction2.Up;
		public override Direction2 DirectionHorizontal => Direction2.Left;
		int ICombustible.BurnStartFrame { get; set; }
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

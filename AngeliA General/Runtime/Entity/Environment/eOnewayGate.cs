using AngeliaFramework;


namespace AngeliaGeneral {


	public class eOnewayGateDown : Oneway {
		public override Direction4 GateDirection => Direction4.Down;
	}


	public class eOnewayGateLeft : Oneway {
		public override Direction4 GateDirection => Direction4.Left;
	}


	public class eOnewayGateRight : Oneway {
		public override Direction4 GateDirection => Direction4.Right;
	}


	public class eOnewayGateUp : Oneway {
		public override Direction4 GateDirection => Direction4.Up;
	}


}

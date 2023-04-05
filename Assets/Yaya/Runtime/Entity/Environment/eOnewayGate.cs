using AngeliaFramework;


namespace Yaya {


	public class eOnewayGateDown : Oneway {
		private static readonly int ONEWAY_CODE = "Oneway Gate".AngeHash();
		public override Direction4 GateDirection => Direction4.Down;
		protected override int ArtworkCode => ONEWAY_CODE;
	}


	public class eOnewayGateLeft : Oneway {
		private static readonly int ONEWAY_CODE = "Oneway Gate".AngeHash();
		public override Direction4 GateDirection => Direction4.Left;
		protected override int ArtworkCode => ONEWAY_CODE;
	}


	public class eOnewayGateRight : Oneway {
		private static readonly int ONEWAY_CODE = "Oneway Gate".AngeHash();
		public override Direction4 GateDirection => Direction4.Right;
		protected override int ArtworkCode => ONEWAY_CODE;
	}


	public class eOnewayGateUp : Oneway {
		private static readonly int ONEWAY_CODE = "Oneway Gate".AngeHash();
		public override Direction4 GateDirection => Direction4.Up;
		protected override int ArtworkCode => ONEWAY_CODE;
	}


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGeneral {


	// Wood
	public class eConveyorWoodLeft : eConveyorWood { protected override int MoveSpeed => -12; }
	public class eConveyorWoodRight : eConveyorWood { protected override int MoveSpeed => 12; }
	public abstract class eConveyorWood : Conveyor {

		private static readonly int CODE_L = "ConveyorWood Left".AngeHash();
		private static readonly int CODE_M = "ConveyorWood Mid".AngeHash();
		private static readonly int CODE_R = "ConveyorWood Right".AngeHash();
		private static readonly int CODE_S = "ConveyorWood Single".AngeHash();

		protected override int ArtCodeLeft => CODE_L;
		protected override int ArtCodeMid => CODE_M;
		protected override int ArtCodeRight => CODE_R;
		protected override int ArtCodeSingle => CODE_S;

	}


	// Iron
	public class eConveyorIronLeft : eConveyorIron { protected override int MoveSpeed => -24; }
	public class eConveyorIronRight : eConveyorIron { protected override int MoveSpeed => 24; }
	public abstract class eConveyorIron : Conveyor {

		private static readonly int CODE_L = "ConveyorIron Left".AngeHash();
		private static readonly int CODE_M = "ConveyorIron Mid".AngeHash();
		private static readonly int CODE_R = "ConveyorIron Right".AngeHash();
		private static readonly int CODE_S = "ConveyorIron Single".AngeHash();

		protected override int ArtCodeLeft => CODE_L;
		protected override int ArtCodeMid => CODE_M;
		protected override int ArtCodeRight => CODE_R;
		protected override int ArtCodeSingle => CODE_S;

	}


}
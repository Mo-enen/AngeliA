using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;



namespace Yaya {

	// Wood
	public class eConveyorWoodLeft : eConveyorWood { protected override int MoveSpeed => -12; }
	public class eConveyorWoodRight : eConveyorWood { protected override int MoveSpeed => 12; }
	public abstract class eConveyorWood : Conveyor {
		private static readonly int[] MID_CODES = new int[8] { "ConveyorWood Mid 0".AngeHash(), "ConveyorWood Mid 1".AngeHash(), "ConveyorWood Mid 2".AngeHash(), "ConveyorWood Mid 3".AngeHash(), "ConveyorWood Mid 4".AngeHash(), "ConveyorWood Mid 5".AngeHash(), "ConveyorWood Mid 6".AngeHash(), "ConveyorWood Mid 7".AngeHash(), };
		private static readonly int[] LEFT_CODES = new int[8] { "ConveyorWood Left 0".AngeHash(), "ConveyorWood Left 1".AngeHash(), "ConveyorWood Left 2".AngeHash(), "ConveyorWood Left 3".AngeHash(), "ConveyorWood Left 4".AngeHash(), "ConveyorWood Left 5".AngeHash(), "ConveyorWood Left 6".AngeHash(), "ConveyorWood Left 7".AngeHash(), };
		private static readonly int[] RIGHT_CODES = new int[8] { "ConveyorWood Right 0".AngeHash(), "ConveyorWood Right 1".AngeHash(), "ConveyorWood Right 2".AngeHash(), "ConveyorWood Right 3".AngeHash(), "ConveyorWood Right 4".AngeHash(), "ConveyorWood Right 5".AngeHash(), "ConveyorWood Right 6".AngeHash(), "ConveyorWood Right 7".AngeHash(), };
		private static readonly int[] SINGLE_CODES = new int[8] { "ConveyorWood Single 0".AngeHash(), "ConveyorWood Single 1".AngeHash(), "ConveyorWood Single 2".AngeHash(), "ConveyorWood Single 3".AngeHash(), "ConveyorWood Single 4".AngeHash(), "ConveyorWood Single 5".AngeHash(), "ConveyorWood Single 6".AngeHash(), "ConveyorWood Single 7".AngeHash(), };
		public override int GetArtworkCode (FittingPose pose, int frame) => pose switch {
			FittingPose.Left => LEFT_CODES[frame],
			FittingPose.Mid => MID_CODES[frame],
			FittingPose.Right => RIGHT_CODES[frame],
			FittingPose.Single => SINGLE_CODES[frame],
			_ => 0,
		};
	}


	// Iron
	public class eConveyorIronLeft : eConveyorIron { protected override int MoveSpeed => -24; }
	public class eConveyorIronRight : eConveyorIron { protected override int MoveSpeed => 24; }
	public abstract class eConveyorIron : Conveyor {
		private static readonly int[] MID_CODES = new int[8] { "ConveyorIron Mid 0".AngeHash(), "ConveyorIron Mid 1".AngeHash(), "ConveyorIron Mid 2".AngeHash(), "ConveyorIron Mid 3".AngeHash(), "ConveyorIron Mid 4".AngeHash(), "ConveyorIron Mid 5".AngeHash(), "ConveyorIron Mid 6".AngeHash(), "ConveyorIron Mid 7".AngeHash(), };
		private static readonly int[] LEFT_CODES = new int[8] { "ConveyorIron Left 0".AngeHash(), "ConveyorIron Left 1".AngeHash(), "ConveyorIron Left 2".AngeHash(), "ConveyorIron Left 3".AngeHash(), "ConveyorIron Left 4".AngeHash(), "ConveyorIron Left 5".AngeHash(), "ConveyorIron Left 6".AngeHash(), "ConveyorIron Left 7".AngeHash(), };
		private static readonly int[] RIGHT_CODES = new int[8] { "ConveyorIron Right 0".AngeHash(), "ConveyorIron Right 1".AngeHash(), "ConveyorIron Right 2".AngeHash(), "ConveyorIron Right 3".AngeHash(), "ConveyorIron Right 4".AngeHash(), "ConveyorIron Right 5".AngeHash(), "ConveyorIron Right 6".AngeHash(), "ConveyorIron Right 7".AngeHash(), };
		private static readonly int[] SINGLE_CODES = new int[8] { "ConveyorIron Single 0".AngeHash(), "ConveyorIron Single 1".AngeHash(), "ConveyorIron Single 2".AngeHash(), "ConveyorIron Single 3".AngeHash(), "ConveyorIron Single 4".AngeHash(), "ConveyorIron Single 5".AngeHash(), "ConveyorIron Single 6".AngeHash(), "ConveyorIron Single 7".AngeHash(), };
		public override int GetArtworkCode (FittingPose pose, int frame) => pose switch {
			FittingPose.Left => LEFT_CODES[frame],
			FittingPose.Mid => MID_CODES[frame],
			FittingPose.Right => RIGHT_CODES[frame],
			FittingPose.Single => SINGLE_CODES[frame],
			_ => 0,
		};
	}

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityBounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE)]
	public class eTrunk : eTree {


		protected override string TrunkCode => "Trunk";
		protected override string LeafCode => "";
		protected override int LeafCount => 0;




	}
}

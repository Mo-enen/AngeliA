using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityBounds(0, 0, Const.CELL_SIZE, Const.CELL_SIZE)]
	public class eTrunkDark : eTree {


		protected override string TrunkCode => "Trunk Dark";
		protected override string LeafCode => "";
		protected override int LeafCount => 0;




	}
}

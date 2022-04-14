using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class ePalm : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 1".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 1".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Palm 0".AngeHash(), "Leaf Palm 1".AngeHash(), "Leaf Palm 2".AngeHash(), };

		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			// Leaf



		}


	}
}
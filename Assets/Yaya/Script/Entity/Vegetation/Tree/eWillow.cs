using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eWillow : eTree {


		private static readonly int TRUNK_BOTTOM_CODE = "Trunk Bottom 2".AngeHash();
		private static readonly int TRUNK_MID_CODE = "Trunk Mid 2".AngeHash();
		private static readonly int[] LEAF_CODES = new int[] { "Leaf Willow 0".AngeHash(), "Leaf Willow 1".AngeHash(), "Leaf Willow 2".AngeHash(), };

		protected override int TrunkBottomCode => TRUNK_BOTTOM_CODE;
		protected override int TrunkMidCode => TRUNK_MID_CODE;


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			// Leaf



		}

	}
}
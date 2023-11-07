using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class LeafMaple : Leaf { }


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class LeafPine : Leaf { }


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class LeafPoplar : Leaf { }


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class LeafPalm : Leaf {

		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillBlock(Const.LAYER_ENVIRONMENT, TypeID, Rect, true, Const.ONEWAY_UP_TAG);
		}

		public override void FrameUpdate () {
			CellRenderer.Draw(LeafArtworkCode, base.Rect.Shift(0, GetLeafShiftY(-24)));
		}

	}


	public class LeafWillow : Leaf {

		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillBlock(
				Const.LAYER_ENVIRONMENT, TypeID,
				Rect.Shrink(0, 0, 0, Height / 2),
				true, Const.CLIMB_TAG
			);
		}

		public override void FrameUpdate () => CellRenderer.Draw(LeafArtworkCode, base.Rect.Shift(GetLeafShiftY(Y, 120, 12), 0));

	}
}
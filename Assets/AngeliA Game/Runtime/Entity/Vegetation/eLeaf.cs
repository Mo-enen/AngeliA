using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class eLeafMaple : Leaf {
		public override void FillPhysics () {
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}
	}


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class eLeafPine : Leaf {
		public override void FillPhysics () {
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}
	}


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class eLeafPoplar : Leaf {
		public override void FillPhysics () {
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}
	}


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class eLeafPalm : Leaf {

		public override void FillPhysics () {
			CellPhysics.FillBlock(Const.LAYER_ENVIRONMENT, TypeID, Rect, true, Const.ONEWAY_UP_TAG);
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}

		public override void FrameUpdate () {
			CellRenderer.Draw(LeafArtworkCode, base.Rect.Shift(0, GetLeafShiftY(-24)));
		}

	}


	public class eLeafWillow : Leaf {

		public override void FillPhysics () {
			CellPhysics.FillBlock(
				Const.LAYER_ENVIRONMENT, TypeID,
				Rect.Shrink(0, 0, 0, Height / 2),
				true, Const.CLIMB_TAG
			);
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}

		public override void FrameUpdate () => CellRenderer.Draw(LeafArtworkCode, base.Rect.Shift(GetLeafShiftY(Y, 120, 12), 0));

	}
}
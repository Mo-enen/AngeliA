using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eWoodBoard : eYayaRigidbody {

		public override int PhysicsLayer => YayaConst.LAYER_ENVIRONMENT;
		public override bool DestroyWhenInsideGround => true;

		private static readonly int BARREL_CODE = "Wood Board".AngeHash();


		public override void FrameUpdate () {
			CellRenderer.Draw(BARREL_CODE, Rect);
			base.FrameUpdate();
		}

	}
}

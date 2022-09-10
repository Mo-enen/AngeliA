using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.EntityCapacity(32)]
	public class eBarrel : eYayaRigidbody {


		public override int PhysicsLayer => YayaConst.LAYER_ENVIRONMENT;
		public override bool DestroyWhenInsideGround => true;

		private static readonly int BARREL_CODE = "Barrel".AngeHash();


		public override void FrameUpdate () {
            CellRenderer.Draw(BARREL_CODE, base.Rect);
			base.FrameUpdate();
		}


	}
}

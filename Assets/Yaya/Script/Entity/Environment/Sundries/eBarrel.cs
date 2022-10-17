using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.Capacity(32)]
	public class eBarrel : eYayaRigidbody {


		public override int PhysicsLayer => YayaConst.LAYER_ENVIRONMENT;
		protected override bool DestroyWhenInsideGround => true;


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, base.Rect);
		}


	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBox : eRigidbody {

		public override int PhysicsLayer => YayaConst.ENVIRONMENT;
		public override bool DestroyOnInsideGround => true;

		private static readonly int BARREL_CODE = "Box".AngeHash();


		public override void FrameUpdate () {
			CellRenderer.Draw(BARREL_CODE, Rect);
			base.FrameUpdate();
		}

	}
}

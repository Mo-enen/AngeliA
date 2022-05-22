using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityCapacity(32)]
	public class eBarrel : eRigidbody {


		public override int PhysicsLayer => YayaConst.ENVIRONMENT;
		public override bool DestroyOnInsideGround => true;

		private static readonly int BARREL_CODE = "Barrel".AngeHash();


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(BARREL_CODE, Rect);
			base.FrameUpdate(frame);
		}


	}
}

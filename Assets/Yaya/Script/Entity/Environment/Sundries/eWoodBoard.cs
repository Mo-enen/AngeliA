using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	public class eWoodBoard : eRigidbody {

		public override int CollisionLayer => (int)PhysicsLayer.Environment;
		public override bool DestroyOnInsideGround => true;

		private static readonly int BARREL_CODE = "Wood Board".AngeHash();


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(BARREL_CODE, Rect);
			base.FrameUpdate(frame);
		}

	}
}

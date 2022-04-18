using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityCapacity(32)]
	public class eBarrel : eRigidbody {


		public override int CollisionLayer => (int)PhysicsLayer.Environment;
		public override bool DestroyOnInsideGround => true;

		private static readonly int BARREL_CODE = "Barrel".AngeHash();


		public override void OnActived (int frame) {
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
			base.OnActived(frame);
		}


		public override void FrameUpdate (int frame) {
			int size = Const.CELL_SIZE;
			CellRenderer.Draw(BARREL_CODE, X, Y, size, size);
			base.FrameUpdate(frame);
		}


	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBarrelSmall : eRigidbody {


		public override int Layer => (int)EntityLayer.Environment;
		public override int CollisionLayer => (int)PhysicsLayer.Environment;
		public override bool DestroyOnInsideGround => true;

		private static readonly int BARREL_CODE = "Barrel Small".AngeHash();


		public override void OnCreate (int frame) {
			Width = Const.CELL_SIZE / 2;
			Height = Const.CELL_SIZE / 2;
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(BARREL_CODE, X, Y, Width, Height);
			base.FrameUpdate(frame);
		}


	}
}

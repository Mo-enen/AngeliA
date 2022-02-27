using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class eBarrelSmall : eRigidbody {


		public override EntityLayer Layer => EntityLayer.Environment;
		public override PhysicsLayer CollisionLayer => PhysicsLayer.Environment;

		private static readonly int BARREL_CODE = "Barrel Small".AngeHash();


		public override void OnCreate (int frame) {
			Width = Const.CELL_SIZE / 2;
			Height = Const.CELL_SIZE / 2;
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {

			CellRenderer.Draw(BARREL_CODE, Rect);

			base.FrameUpdate(frame);
		}



	}
}

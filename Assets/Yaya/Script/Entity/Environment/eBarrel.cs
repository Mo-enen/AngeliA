using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class eBarrel : eRigidbody {


		public override EntityLayer Layer => EntityLayer.Environment;
		public override PhysicsLayer CollisionLayer => PhysicsLayer.Environment;
		
		private static readonly int BARREL_CODE = "Barrel 2".ACode();


		public override void OnCreate (int frame) {
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {

			CellRenderer.Draw(BARREL_CODE, Rect);

			base.FrameUpdate(frame);
		}



	}
}

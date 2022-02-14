using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class eBarrel : eRigidbody {


		public override EntityLayer Layer => EntityLayer.Environment;
		protected override string ThumbnailName => "Barrel 2";
		public override PhysicsLayer CollisionLayer => PhysicsLayer.Environment;
		public override PhysicsMask CollisionMask => PhysicsMask.Character | PhysicsMask.Environment | PhysicsMask.Level;


		public override void OnCreate (int frame) {
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {

			CellRenderer.Draw(Thumbnail, Rect);

			base.FrameUpdate(frame);
		}



	}
}

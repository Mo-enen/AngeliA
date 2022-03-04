using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class eBarrel : eRigidbody {


		public override EntityLayer Layer => EntityLayer.Environment;
		public override PhysicsLayer CollisionLayer => PhysicsLayer.Environment;

		private static readonly int BARREL_CODE = "Barrel".AngeHash();
		private static readonly int BARREL_SMALL_CODE = "Barrel Small".AngeHash();

		private bool IsSmallBarrel => Data == 1;


		public override void OnCreate (int frame) {
			Width = IsSmallBarrel ? Const.CELL_SIZE / 2 : Const.CELL_SIZE;
			Height = IsSmallBarrel ? Const.CELL_SIZE / 2 : Const.CELL_SIZE;
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {

			CellRenderer.Draw(IsSmallBarrel ? BARREL_SMALL_CODE : BARREL_CODE, Rect);

			base.FrameUpdate(frame);
		}



	}
}

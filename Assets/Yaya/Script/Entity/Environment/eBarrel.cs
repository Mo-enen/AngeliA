using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eBarrel : eRigidbody {


		public override int Layer => (int)EntityLayer.Environment;
		public override int CollisionLayer => (int)PhysicsLayer.Environment;
		public override bool DestroyOnInsideGround => true;

		private static readonly int BARREL_CODE = "Barrel".AngeHash();
		private static readonly int BARREL_SMALL_CODE = "Barrel Small".AngeHash();

		private bool IsSmallBarrel => Data == 1;


		public override void OnCreate (int frame) {
			Width = IsSmallBarrel ? Const.CELL_SIZE / 2 : Const.CELL_SIZE;
			Height = IsSmallBarrel ? Const.CELL_SIZE / 2 : Const.CELL_SIZE;
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {
			int size = IsSmallBarrel ? Const.CELL_SIZE / 2 : Const.CELL_SIZE;
			CellRenderer.Draw(IsSmallBarrel ? BARREL_SMALL_CODE : BARREL_CODE, X, Y, size, size);
			base.FrameUpdate(frame);
		}


	}
}

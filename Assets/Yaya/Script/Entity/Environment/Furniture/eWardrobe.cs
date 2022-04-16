using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eWardrobe : Entity {


		private static readonly int[] CODES = new int[] { "Wardrobe 0".AngeHash(), "Wardrobe 1".AngeHash(), "Wardrobe 2".AngeHash(), "Wardrobe 3".AngeHash(), };


		// Api
		public override int Capacity => 8;

		// Short
		private int Code => CODES[0];


		// MSG
		public override void OnActived (int frame) {
			base.OnActived(frame);
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE;
		}


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this, true, Const.ONEWAY_UP_TAG);
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(Code, Rect);
		}


	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eVine : Entity {


		public override int Layer => (int)EntityLayer.Environment;


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillBlock((int)PhysicsLayer.Environment, Rect, true, YayaConst.VINE_TAG);
		}


	}
}

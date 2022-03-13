using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eVine : Entity {


		public override EntityLayer Layer => EntityLayer.Environment;


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillBlock(PhysicsLayer.Environment, Rect, true, YayaUtil.VINE_TAG);
		}


	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eClimbable : Entity {


		public override int Layer => (int)EntityLayer.Environment;
		public virtual bool CorrectPosition { get; } = true;


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this, true, YayaConst.CLIMB_TAG);
		}


	}
}

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
			CellPhysics.FillEntity(Layer, this, true, YayaConst.CLIMB_TAG);
		}


	}
}

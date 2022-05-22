using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eClimbable : Entity {


		public virtual bool CorrectPosition { get; } = true;


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true, YayaConst.CLIMB_TAG);
		}


	}
}

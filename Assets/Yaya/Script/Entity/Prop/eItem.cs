using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eItem : Entity {




		#region --- VAR ---


		// Api
		public override EntityLayer Layer => EntityLayer.Item;
		public int VelocityY { get; private set; } = 0;


		#endregion




		#region --- MSG ---


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity(PhysicsLayer.Item, this, true, Const.ITEM_TAG);
		}


		public override void PhysicsUpdate (int frame) {
			base.PhysicsUpdate(frame);
			if (VelocityY != 0) {


			}
		}


		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---




		#endregion




	}
}

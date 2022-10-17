using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.MapEditorGroup("Collectable")]
	public abstract class eCollectable : Entity {


		// MSG
		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, X + Width / 2, Y + Height / 2, 500, 500, 0, Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE);
		}


		public abstract bool OnCollect (Entity source);


	}


	[EntityAttribute.Capacity(128)]
	public class eCoin : eCollectable {


		public override bool OnCollect (Entity source) {
			if (source is not ePlayer) return false;



			return true;
		}


	}
}
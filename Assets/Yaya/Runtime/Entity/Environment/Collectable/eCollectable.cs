using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.MapEditorGroup("Collectable")]
	public abstract class eCollectable : Entity {


		// MSG
		public override void OnActived () {
			base.OnActived();
			if (CellRenderer.TryGetSprite(TypeID, out var sprite)) {
				X += (Width - sprite.GlobalWidth) / 2;
				Y += (Height - sprite.GlobalHeight) / 2;
				Width = sprite.GlobalWidth;
				Height = sprite.GlobalHeight;
			}
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this, true);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, Rect);
		}


		public abstract bool OnCollect (Entity source);


	}
}
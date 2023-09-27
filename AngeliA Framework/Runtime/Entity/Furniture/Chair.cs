using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Chair : Furniture {

		protected sealed override Direction3 ModuleType => Direction3.None;

		private bool? DockedToRight = null;

		public override void OnActivated () {
			base.OnActivated();
			DockedToRight = null;
		}

		public override void FrameUpdate () {
			if (!DockedToRight.HasValue) {
				DockedToRight = !CellPhysics.HasEntity<Table>(
					Rect.Expand(ColliderBorder).Shift(-Const.CEL, 0).Shrink(1),
					Const.MASK_ENVIRONMENT, this, OperationMode.ColliderAndTrigger
				);
			}
			// Render
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, 0, out var sprite, false, true)) {
				var rect = Rect.Expand(ColliderBorder);
				if (DockedToRight.HasValue && DockedToRight.Value) {
					CellRenderer.Draw(sprite.GlobalID, rect);
					AngeUtil.DrawShadow(sprite.GlobalID, rect);
				} else {
					var cell = CellRenderer.Draw(sprite.GlobalID, rect.CenterX(), rect.y, 500, 0, 0, -rect.width, rect.height);
					AngeUtil.DrawShadow(sprite.GlobalID, cell);
				}
			}
		}

	}
}

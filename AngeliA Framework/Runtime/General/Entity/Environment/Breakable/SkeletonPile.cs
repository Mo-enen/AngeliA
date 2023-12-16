using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class SkeletonPile : BreakableEntity {

		private int ArtworkCode = 0;
		private IRect FullRect = default;

		public override void OnActivated () {
			base.OnActivated();
			Width = Const.CEL;
			Height = Const.CEL;
			FullRect = Rect;
			int artworkIndex = X.UDivide(Const.CEL) + Y.UDivide(Const.CEL);
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, artworkIndex, out var sprite)) {
				var rect = base.Rect.Shrink(sprite.GlobalBorder.left, sprite.GlobalBorder.right, sprite.GlobalBorder.down, sprite.GlobalBorder.up);
				X = rect.x;
				Y = rect.y;
				Width = rect.width;
				Height = rect.height;
				ArtworkCode = sprite.GlobalID;
			}
		}

		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this);
		}

		public override void FrameUpdate () {
			CellRenderer.Draw(ArtworkCode, FullRect);
		}

	}
}

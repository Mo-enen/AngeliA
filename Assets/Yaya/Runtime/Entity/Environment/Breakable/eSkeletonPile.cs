using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eSkeletonPile : eBreakable {


		private static readonly int CODE = "Skeleton Pile".AngeHash();
		private int ArtworkCode = 0;
		private RectInt FullRect = default;


		public override void OnActived () {
			base.OnActived();
			Width = Const.CEL;
			Height = Const.CEL;
			FullRect = Rect;
			int artworkIndex = X.UDivide(Const.CEL) + Y.UDivide(Const.CEL);
			if (CellRenderer.TryGetSpriteFromGroup(CODE, artworkIndex, out var sprite)) {
				var rect = base.Rect.Shrink(sprite.GlobalBorder.Left, sprite.GlobalBorder.Right, sprite.GlobalBorder.Down, sprite.GlobalBorder.Up);
				X = rect.x;
				Y = rect.y;
				Width = rect.width;
				Height = rect.height;
				ArtworkCode = sprite.GlobalID;
			}
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this);
		}


		public override void FrameUpdate () {
			CellRenderer.Draw(ArtworkCode, FullRect);
			base.FrameUpdate();
		}


	}
}

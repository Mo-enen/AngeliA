using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eSkeletonPile : Entity {


		private static readonly int CODE = "Skeleton Pile".AngeHash();
		private int ArtworkCode = 0;
		private RectInt FullRect = default;


		public override void OnActived () {
			base.OnActived();
			FullRect = Rect;
			int artworkIndex = X.UDivide(Const.CELL_SIZE) + Y.UDivide(Const.CELL_SIZE);
			if (AngeliaFramework.Renderer.TryGetSpriteFromGroup(CODE, artworkIndex, out var sprite)) {
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
			Physics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this);
		}


		public override void FrameUpdate () {
            AngeliaFramework.Renderer.Draw(ArtworkCode, FullRect);
			base.FrameUpdate();
		}


	}
}

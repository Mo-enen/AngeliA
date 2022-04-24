using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eSkeletonPile : Entity {


		private static readonly int[] CODES = new int[] {
			"Skeleton Pile 0".AngeHash(), "Skeleton Pile 1".AngeHash(), "Skeleton Pile 2".AngeHash(),
			"Skeleton Pile 3".AngeHash(), "Skeleton Pile 4".AngeHash(), "Skeleton Pile 5".AngeHash(),
			"Skeleton Pile 6".AngeHash(), "Skeleton Pile 7".AngeHash(),
		};
		private int ArtworkIndex = 0;
		private RectInt FullRect = default;


		public override void OnActived (int frame) {
			base.OnActived(frame);
			FullRect = Rect;
			ArtworkIndex = (X.UDivide(Const.CELL_SIZE) + Y.UDivide(Const.CELL_SIZE)).UMod(CODES.Length);
			if (CellRenderer.TryGetSprite(CODES[ArtworkIndex % CODES.Length], out var sprite)) {
				var rect = Rect.Shrink(sprite.GlobalBorder.Left, sprite.GlobalBorder.Right, sprite.GlobalBorder.Down, sprite.GlobalBorder.Up);
				X = rect.x;
				Y = rect.y;
				Width = rect.width;
				Height = rect.height;
			}
		}


		public override void FillPhysics (int frame) {
			base.FillPhysics(frame);
			CellPhysics.FillEntity((int)PhysicsLayer.Environment, this);
		}


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(CODES[ArtworkIndex % CODES.Length], FullRect);
			base.FrameUpdate(frame);
		}


	}
}

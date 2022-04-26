using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eRock : Entity {


		private static readonly int[] CODES = new int[] {
			"Rock 0".AngeHash(), "Rock 1".AngeHash(), "Rock 2".AngeHash(),
			"Rock 3".AngeHash(), "Rock 4".AngeHash(), "Rock 5".AngeHash(),
			"Rock 6".AngeHash(), "Rock 7".AngeHash(), "Rock 8".AngeHash(),
			"Rock 9".AngeHash(), "Rock 10".AngeHash(), "Rock 11".AngeHash(), "Rock 12".AngeHash(),
		};
		private int ArtworkIndex = 0;
		private RectInt FullRect = default;


		public override void OnActived (int frame) {
			base.OnActived(frame);
			FullRect = new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE);
			Width = Height = Const.CELL_SIZE;
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
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this);
		}


		public override void FrameUpdate (int frame) {
			CellRenderer.Draw(CODES[ArtworkIndex % CODES.Length], FullRect);
			base.FrameUpdate(frame);
		}


	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eRock : Entity {


		private static readonly int CODE = "Rock".AngeHash();
		private int ArtworkCode = 0;
		private RectInt FullRect = default;


		public override void OnActived () {
			base.OnActived();
			FullRect = new(X, Y, Const.CELL_SIZE, Const.CELL_SIZE);
			Width = Height = Const.CELL_SIZE;
			int artworkIndex = X.UDivide(Const.CELL_SIZE) + Y.UDivide(Const.CELL_SIZE);
			if (CellRenderer.TryGetSpriteFromGroup(CODE, artworkIndex, out var sprite)) {
				var rect = Rect.Shrink(sprite.GlobalBorder.Left, sprite.GlobalBorder.Right, sprite.GlobalBorder.Down, sprite.GlobalBorder.Up);
				X = rect.x;
				Y = rect.y;
				Width = rect.width;
				Height = rect.height;
				ArtworkCode = sprite.GlobalID;
			}
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this);
		}


		public override void FrameUpdate () {
			CellRenderer.Draw(ArtworkCode, FullRect);
			base.FrameUpdate();
		}


	}
}

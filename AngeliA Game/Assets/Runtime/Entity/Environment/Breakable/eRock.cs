using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eRock : Breakable {

		private static readonly int ITEM_CODE = typeof(iStone).AngeHash();
		private static readonly int CODE = "Rock".AngeHash();
		private int ArtworkCode = 0;
		private RectInt FullRect = default;


		public override void OnActivated () {
			base.OnActivated();
			FullRect = new(X, Y, Const.CEL, Const.CEL);
			Width = Height = Const.CEL;
			int artworkIndex = X.UDivide(Const.CEL) + Y.UDivide(Const.CEL);
			if (CellRenderer.TryGetSpriteFromGroup(CODE, artworkIndex, out var sprite)) {
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
			CellPhysics.FillEntity(Const.LAYER_ENVIRONMENT, this);
		}


		public override void FrameUpdate () {
			CellRenderer.Draw(ArtworkCode, FullRect);
			AngeUtil.DrawShadow(ArtworkCode, FullRect);
		}


		protected override void OnBreak () {
			base.OnBreak();
			if (AngeUtil.RandomInt(0, 32) == 0) {
				ItemSystem.ItemSpawnItemAtPlayer(ITEM_CODE);
			}
		}


	}
}

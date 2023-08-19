using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {
	public class eSkeletonPile : Breakable {


		private const int SPAWN_ITEM_POSSIBILITY = 32;
		private static readonly int SKULL_CODE = typeof(iSkull).AngeHash();
		private int ArtworkCode = 0;
		private RectInt FullRect = default;
		private static readonly System.Random Ran = new(2334768);


		public override void OnActivated () {
			base.OnActivated();
			Width = Const.CEL;
			Height = Const.CEL;
			FullRect = Rect;
			int artworkIndex = X.UDivide(Const.CEL) + Y.UDivide(Const.CEL);
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, artworkIndex, out var sprite)) {
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
			AngeUtil.DrawShadow(ArtworkCode, FullRect);
		}


		protected override void OnBreak () {
			Stage.MarkAsAntiSpawn(this);
			BreakingParticle.SpawnParticles(ArtworkCode, Rect);
			if (Ran.Next(0, SPAWN_ITEM_POSSIBILITY) == 0) {
				ItemSystem.SpawnItem(SKULL_CODE);
			}
		}


	}
}

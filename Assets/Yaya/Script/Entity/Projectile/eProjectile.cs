using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityCapacity(512)]
	[ExcludeInMapEditor]
	public abstract class eProjectile : Entity {


		// Api
		protected abstract string ArtworkName { get; }
		protected virtual int CollisionMask => YayaConst.MASK_SOLID;
		protected int VelocityX { get; set; } = 0;
		protected int VelocityY { get; set; } = 0;
		protected int LocalFrame => Game.GlobalFrame - SpawnFrame;

		// Data
		private int ArtworkCode = 0;
		private int SpawnFrame = int.MinValue;


		// MSG
		public override void OnActived () {
			base.OnActived();
			ArtworkCode = ArtworkName.AngeHash();
			SpawnFrame = Game.GlobalFrame;
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();




		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (CellRenderer.TryGetSprite(ArtworkCode, out var sprite)) {
				var rect = Rect;
				if (!sprite.GlobalBorder.IsZero) {
					rect = rect.Expand(
						sprite.GlobalBorder.Left, sprite.GlobalBorder.Right,
						sprite.GlobalBorder.Up, sprite.GlobalBorder.Down
					);
				}
				CellRenderer.Draw(sprite.GlobalID, rect);
			}
		}



	}
}

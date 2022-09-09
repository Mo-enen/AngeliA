using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	[EntityAttribute.MapEditorGroup("Vegetation")]
	[EntityAttribute.EntityCapacity(128)]
	[EntityAttribute.DrawBehind]
	public abstract class ePlant : Entity {


		// Api
		protected abstract int ArtworkCode { get; }
		protected virtual bool Interactable { get; } = true;
		protected virtual bool Breath { get; } = true;
		protected virtual RangeInt SizeOffset { get; } = default;

		// Data
		private int ArtworkID = 0;
		private bool Interacting = false;


		// MSG
		public override void OnActived () {
			base.OnActived();
			Interacting = false;
			// Rect from Sprite
			if (AngeliaFramework.Renderer.TryGetSpriteFromGroup(
                    ArtworkCode,
					(X * 3 + Y * 7) / Const.CELL_SIZE,
					out var sp
				) || AngeliaFramework.Renderer.TryGetSprite(ArtworkCode, out sp)
			) {
                ArtworkID = sp.GlobalID;
                Width = sp.GlobalWidth;
                Height = sp.GlobalHeight;
                X = X + Const.CELL_SIZE / 2 - sp.GlobalWidth / 2;
			} else {
                Width = Const.CELL_SIZE;
                Height = Const.CELL_SIZE;
			}
			// Size Offset
			if (SizeOffset.length > 0) {
				int offset = SizeOffset.start + ((X * 7 + Y * 3) / Const.CELL_SIZE).UMod(SizeOffset.length);
				Width += offset;
				Height += offset;
				X -= offset / 2;
			}
		}


		public override void FillPhysics () {
			base.FillPhysics();
			Physics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (Interactable) {
				Interacting = Interacting || Physics.HasEntity<eYayaRigidbody>(Rect.Shrink(Width / 4, Width / 4, 0, 0), YayaConst.MASK_RIGIDBODY, this);
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			var rect = Rect;
			int rot = 0;
			// Interacting
			if (Interacting) {
				Interacting = false;


			}
			// Breath
			if (Breath) {
				int fixedGameFrame = Game.GlobalFrame - X * 6 / Const.CELL_SIZE;
				rect.height += (fixedGameFrame - X / Const.CELL_SIZE).PingPong(60) / 6 - 5;
				rot += (Mathf.PingPong(fixedGameFrame, 120f) / 60f - 1f).RoundToInt();
			}
            AngeliaFramework.Renderer.Draw(ArtworkID, rect.x + rect.width / 2, rect.y, 500, 0, rot, rect.width, rect.height);
		}


	}
}

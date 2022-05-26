using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace Yaya {
	[MapEditorGroup("Vegetation")]
	[EntityCapacity(128)]
	public abstract class ePlant : Entity {


		// Const
		private const int INTERACT_DURATION = 30;

		// Api
		protected abstract int ArtworkCode { get; }
		protected virtual bool Interactable { get; } = true;
		protected virtual bool Breath { get; } = true;

		// Data
		private int ArtworkID = 0;
		private bool Interacting = false;
		private int InteractFrame = int.MinValue;


		// MSG
		public override void OnActived () {
			base.OnActived();
			if (CellRenderer.TryGetSpriteFromGroup(
					ArtworkCode,
					(X * 3 + Y * 7) / Const.CELL_SIZE,
					out var sp
				) || CellRenderer.TryGetSprite(ArtworkCode, out sp)
			) {
				ArtworkID = sp.GlobalID;
				Width = sp.GlobalWidth;
				Height = sp.GlobalHeight;
				X = X + Const.CELL_SIZE / 2 - Width / 2;
			} else {
				Width = Const.CELL_SIZE;
				Height = Const.CELL_SIZE;
			}
			Interacting = false;
			InteractFrame = int.MinValue;
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.ENVIRONMENT, this, true);
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (Interactable) {
				bool interacting = CellPhysics.HasEntity<eRigidbody>(Rect.Shrink(Width / 4, Width / 4, 0, 0), YayaConst.MASK_RIGIDBODY, this);
				if (interacting && !Interacting) InteractFrame = Game.GlobalFrame;
				Interacting = interacting;
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			var rect = Rect;
			int rot = 0;
			// Interacting
			if (Game.GlobalFrame < InteractFrame + INTERACT_DURATION) {
				int frame = InteractFrame + INTERACT_DURATION - Game.GlobalFrame;
				int amount = Height > Const.CELL_SIZE ? 1 : 3;
				rot = frame.PingPong(6) * amount - 3 * amount;
				rot = rot.MoveTowards(0, (INTERACT_DURATION - frame) / 2);
			}
			// Breath
			if (Breath) {
				int fixedGameFrame = Game.GlobalFrame - X * 6 / Const.CELL_SIZE;
				rect.height += (fixedGameFrame - X / Const.CELL_SIZE).PingPong(60) / 6 - 5;
				rot += (Mathf.PingPong(fixedGameFrame, 120f) / 60f - 1f).RoundToInt();
			}
			CellRenderer.Draw(ArtworkID, rect.x + rect.width / 2, rect.y, 500, 0, rot, rect.width, rect.height);
		}


	}
}

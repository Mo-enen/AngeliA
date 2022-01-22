using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;
using AngeliaFramework.Rendering;


namespace AngeliaFramework.Entities.Editor {
	public class eDebugPlayer : ePlayer {


		public override bool Despawnable => false;
		public override CharacterMovement Movement => _Movement ??= new() {

		};
		private CharacterMovement _Movement = null;

		public override CharacterRenderer Renderer => _Renderer ??= new() {

		};
		private CharacterRenderer _Renderer = null;




		public int DebugGroundX = 0;
		public int DebugGroundY = 0;
		public int DebugGroundWidth = 24;
		public int DebugGroundHeight = 2;
		public string DebugGroundSprite = "Pixel";
		public Vector2Int TestTo = default;
		public Vector2Int TestSize = new(256, 256);



		// MSG
		public override void FillPhysics (int frame) {
			// Debug Ground
			for (int y = 0; y < DebugGroundHeight; y++) {
				for (int x = 0; x < DebugGroundWidth; x++) {
					CellPhysics.Fill(PhysicsLayer.Level, new RectInt(
						(x + DebugGroundX) * Const.CELL_SIZE,
						(y + DebugGroundY) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					), null);
				}
			}
			base.FillPhysics(frame);
		}


		public override void FrameUpdate (int frame) {
			// Debug Ground
			int id = DebugGroundSprite.ACode();
			for (int y = 0; y < DebugGroundHeight; y++) {
				for (int x = 0; x < DebugGroundWidth; x++) {
					CellRenderer.Draw(id, new RectInt(
						(x + DebugGroundX) * Const.CELL_SIZE,
						(y + DebugGroundY) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					));
				}
			}

			base.FrameUpdate(frame);
		}





	}
}

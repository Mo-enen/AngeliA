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




		public Vector2Int GroundPos = new(0, 0);
		public Vector2Int GroundSize = new(26, 2);
		public string GroundSprite = "Pixel";
		public Vector2Int WaterPos = new(12, 4);
		public Vector2Int WaterSize = new(6, 2);
		public string WaterSprite = "Test Water";




		// MSG
		public override void FillPhysics (int frame) {
			// Debug Ground
			for (int y = 0; y < GroundSize.y; y++) {
				for (int x = 0; x < GroundSize.x; x++) {
					CellPhysics.Fill(PhysicsLayer.Level, new RectInt(
						(x + GroundPos.x) * Const.CELL_SIZE,
						(y + GroundPos.y) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					), null);
				}
			}
			// Debug Water
			int waterTag = "Water".ACode();
			for (int y = 0; y < WaterSize.y; y++) {
				for (int x = 0; x < WaterSize.x; x++) {
					CellPhysics.Fill(PhysicsLayer.Level, new RectInt(
						(x + WaterPos.x) * Const.CELL_SIZE,
						(y + WaterPos.y) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					), null, true, waterTag);
				}
			}

			base.FillPhysics(frame);
		}


		public override void FrameUpdate (int frame) {
			// Debug Ground
			int id = GroundSprite.ACode();
			for (int y = 0; y < GroundSize.y; y++) {
				for (int x = 0; x < GroundSize.x; x++) {
					CellRenderer.Draw(id, new RectInt(
						(x + GroundPos.x) * Const.CELL_SIZE,
						(y + GroundPos.y) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					));
				}
			}
			// Debug Water
			id = WaterSprite.ACode();
			for (int y = 0; y < WaterSize.y; y++) {
				for (int x = 0; x < WaterSize.x; x++) {
					CellRenderer.Draw(id, new RectInt(
						(x + WaterPos.x) * Const.CELL_SIZE,
						(y + WaterPos.y) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					));
				}
			}
			// Debug Info
			CellRenderer.DrawChar(
				$"c_{Movement.CurrentJumpCount.ToString()[0]}".ACode(),
				X - Renderer.Width / 2, Y + Renderer.Height + 64, 256, 256, new Color32(255, 255, 255, 255), out _
			);

			base.FrameUpdate(frame);
		}





	}
}

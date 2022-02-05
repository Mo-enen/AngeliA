using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;
using AngeliaFramework.Rendering;


namespace AngeliaFramework.Entities.Editor {
	public class eDebugPlayer : ePlayer {


		public override bool Despawnable => false;
		public override CharacterMovement Movement => _Movement ??= new(this) {
			//SwimInFreeStyle = true,
		};
		public override CharacterRenderer Renderer => _Renderer ??= new(this) {

		};

		[AngeliaInspector] CharacterMovement _Movement = null;
		[AngeliaInspector] CharacterRenderer _Renderer = null;




		public Vector2Int GroundPosA = new(0, 0);
		public Vector2Int GroundPosB = new(0, 2);
		public Vector2Int GroundSizeA = new(26, 2);
		public Vector2Int GroundSizeB = new(8, 2);
		public string GroundSprite = "Pixel";
		public Vector2Int WaterPos = new(12, 4);
		public Vector2Int WaterSize = new(6, 2);
		public string WaterSprite = "Pixel";




		// MSG
		public override void FillPhysics (int frame) {
			// Debug Ground
			for (int y = 0; y < GroundSizeA.y; y++) {
				for (int x = 0; x < GroundSizeA.x; x++) {
					CellPhysics.FillBlock(PhysicsLayer.Level, new RectInt(
						(x + GroundPosA.x) * Const.CELL_SIZE,
						(y + GroundPosA.y) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					));
				}
			}
			for (int y = 0; y < GroundSizeB.y; y++) {
				for (int x = 0; x < GroundSizeB.x; x++) {
					CellPhysics.FillBlock(PhysicsLayer.Level, new RectInt(
						(x + GroundPosB.x) * Const.CELL_SIZE,
						(y + GroundPosB.y) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					));
				}
			}
			// Debug Water
			int waterTag = "Water".ACode();
			for (int y = 0; y < WaterSize.y; y++) {
				for (int x = 0; x < WaterSize.x; x++) {
					CellPhysics.FillBlock(PhysicsLayer.Level, new RectInt(
						(x + WaterPos.x) * Const.CELL_SIZE,
						(y + WaterPos.y) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					), true, waterTag);
				}
			}

			base.FillPhysics(frame);
		}


		public override void FrameUpdate (int frame) {

			// Debug Ground
			int id = GroundSprite.ACode();
			for (int y = 0; y < GroundSizeA.y; y++) {
				for (int x = 0; x < GroundSizeA.x; x++) {
					CellRenderer.Draw(id, new RectInt(
						(x + GroundPosA.x) * Const.CELL_SIZE,
						(y + GroundPosA.y) * Const.CELL_SIZE,
						Const.CELL_SIZE,
						Const.CELL_SIZE
					));
				}
			}
			for (int y = 0; y < GroundSizeB.y; y++) {
				for (int x = 0; x < GroundSizeB.x; x++) {
					CellRenderer.Draw(id, new RectInt(
						(x + GroundPosB.x) * Const.CELL_SIZE,
						(y + GroundPosB.y) * Const.CELL_SIZE,
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
			for (int i = 0; i < Movement.JumpCount - Movement.CurrentJumpCount; i++) {
				CellRenderer.DrawChar(
					"c_.".ACode(),
					X - Renderer.Width / 2 + i * 32, Y + Renderer.Height + 64, 128, 128, new Color32(255, 255, 255, 255), out _
				);
			}

			base.FrameUpdate(frame);
		}



	}
}

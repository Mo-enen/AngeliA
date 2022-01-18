using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Physics;
using AngeliaFramework.Rendering;


namespace AngeliaFramework.Entities.Editor {
	public class eDebugPlayer : ePlayer {


		protected override CharacterMovement Movement {
			get {
				if (_Movement == null) {
					_Movement = GetAsset("Debug Character Movement".ACode()) as CharacterMovement;
					if (_Movement == null) {
						_Movement = new();
					}
				}
				return _Movement;
			}
		}
		protected override CharacterRenderer Renderer {
			get {
				if (_Renderer == null) {
					_Renderer = GetAsset("Debug Character Renderer".ACode()) as CharacterRenderer;
					if (_Renderer == null) {
						_Renderer = new();
					}
				}
				return _Renderer;
			}
		}

		[SerializeField] CharacterMovement _Movement = null;
		[SerializeField] CharacterRenderer _Renderer = null;
		public int DebugGroundX = 0;
		public int DebugGroundY = 0;
		public int DebugGroundWidth = 24;
		public int DebugGroundHeight = 2;
		public string DebugGroundSprite = "Pixel";


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

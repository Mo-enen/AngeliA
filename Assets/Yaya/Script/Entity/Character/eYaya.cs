using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class YayaRenderer : CharacterRenderer {


		public override RectInt LocalBounds => new(-Const.CELL_SIZE / 2, 0, Const.CELL_SIZE, Const.CELL_SIZE);


		public YayaRenderer (eCharacter ch) : base(ch) { }


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);


			var movement = Character.Movement;



			////////////////////////// Test ////////////////////////

			// Test Pump
			CellRenderer.Draw(
				movement.FacingFront ? "Test Pump".AngeHash() : "Test Pump Back".AngeHash(),
				Character.X, Character.Y, 500, 0, 0,
				movement.FacingRight ? Width : -Width,
				movement.IsSquating ? Height * movement.SquatHeight / movement.Height : Height
			);

			// Jump Dots
			var rect = Character.Rect;
			int count = movement.JumpCount - movement.CurrentJumpCount;
			for (int i = 0; i < count; i++) {
				CellRenderer.Draw("Pixel".AngeHash(), new(rect.xMin + i * 64, rect.y + Const.CELL_SIZE, 48, 48), Color.black);
				CellRenderer.Draw("Pixel".AngeHash(), new(rect.xMin + i * 64 + 8, rect.y + Const.CELL_SIZE + 8, 32, 32));
			}
			////////////////////////// Test ////////////////////////



		}


	}


	public class eYaya : ePlayer {


		public override CharacterMovement Movement => _Movement ??= new(this) {

		};
		public override CharacterRenderer Renderer => _Renderer ??= new(this) {
			Width = Const.CELL_SIZE,
			Height = Const.CELL_SIZE,
		};

		private CharacterMovement _Movement = null;
		private YayaRenderer _Renderer = null;




	}
}

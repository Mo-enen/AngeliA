using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class YayaRenderer : CharacterRenderer {

		public YayaRenderer (eCharacter ch) : base(ch) { }

		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);


			var movement = Character.Movement;



			////////////////////////// Test ////////////////////////

			// Test Pump
			CellRenderer.Draw(
				movement.FacingFront ? "Test Pump".AngeHash() : "Test Pump Back".AngeHash(),
				Character.X, Character.Y, 500, 0, 0,
				movement.FacingRight ? Const.CELL_SIZE : -Const.CELL_SIZE,
				movement.IsSquating ? Const.CELL_SIZE * movement.SquatHeight / movement.Height : Const.CELL_SIZE
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
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Editor {
	public class eDebugPlayer : ePlayer {


		public override bool Despawnable => false;
		public override CharacterMovement Movement => _Movement ??= new(this) {
			//SwimInFreeStyle = true,
		};
		public override CharacterRenderer Renderer => _Renderer ??= new(this) {

		};

		[AngeliaInspector] CharacterMovement _Movement = null;
		[AngeliaInspector] CharacterRenderer _Renderer = null;


		// MSG
		public override void FillPhysics (int frame) {
			
			base.FillPhysics(frame);
		}


		public override void FrameUpdate (int frame) {

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

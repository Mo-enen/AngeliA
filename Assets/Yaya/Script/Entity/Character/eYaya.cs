using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class YayaRenderer : CharacterRenderer {
		public YayaRenderer (eCharacter ch) : base(ch) { }
		public override void FrameUpdate (CharacterPose pose) {
			base.FrameUpdate(pose);

			const int W = Const.CELL_SIZE;
			const int H = Const.CELL_SIZE;

			var rect = Character.Rect;
			var movement = Character.Movement;



			////////////////////////// Test ////////////////////////
			CellRenderer.Draw(
				pose.FacingFront ? "Test Pump".AngeHash() : "Test Pump Back".AngeHash(),
				rect.x + rect.width / 2,
				rect.y,
				500, 0, 0,
				pose.FacingRight ? W : -W,
				movement.IsSquating ? H * movement.SquatHeight / movement.Height : Const.CELL_SIZE
			);

			int count = movement.JumpCount - movement.CurrentJumpCount;
			for (int i = 0; i < count; i++) {
				CellRenderer.Draw("Pixel".AngeHash(), new(rect.xMin + i * 64, rect.y + Const.CELL_SIZE, 48, 48), Color.black);
				CellRenderer.Draw("Pixel".AngeHash(), new(rect.xMin + i * 64 + 8, rect.y + Const.CELL_SIZE + 8, 32, 32));
			}
			////////////////////////// Test ////////////////////////



		}
	}


	public class YayaPose : CharacterPose {
		public YayaPose (eCharacter character) : base(character) { }
		public override void CalculatePose (int frame) {
			base.CalculatePose(frame);




		}
	}


	public class eYaya : ePlayer {

		public override CharacterMovement Movement => _Movement ??= new(this) {

		};
		public override CharacterRenderer Renderer => _Renderer ??= new(this) {

		};
		public override CharacterPose Pose => _Pose ??= new(this) {

		};

		[AngeliaInspector] CharacterMovement _Movement = null;
		[AngeliaInspector] YayaRenderer _Renderer = null;
		[AngeliaInspector] YayaPose _Pose = null;

	}
}

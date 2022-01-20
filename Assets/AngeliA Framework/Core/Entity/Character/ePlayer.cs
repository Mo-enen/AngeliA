using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Input;


namespace AngeliaFramework.Entities {
	public abstract class ePlayer : eCharacter {




		#region --- VAR ---


		// Data
		private int LeftDownFrame = int.MinValue;
		private int RightDownFrame = int.MinValue;
		private int DownDownFrame = int.MinValue;
		private int UpDownFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void FrameUpdate (int frame) {
			Update_Move(frame);
			Update_JumpDashPound();
			base.FrameUpdate(frame);
		}


		private void Update_Move (int frame) {

			var x = Direction3.None;
			var y = Direction3.None;

			// Left
			if (FrameInput.KeyPressing(GameKey.Left)) {
				if (LeftDownFrame < 0) {
					LeftDownFrame = frame;
				}
				if (LeftDownFrame > RightDownFrame) {
					x = Direction3.Negative;
				}
			} else if (LeftDownFrame > 0) {
				LeftDownFrame = int.MinValue;
			}

			// Right
			if (FrameInput.KeyPressing(GameKey.Right)) {
				if (RightDownFrame < 0) {
					RightDownFrame = frame;
				}
				if (RightDownFrame > LeftDownFrame) {
					x = Direction3.Positive;
				}
			} else if (RightDownFrame > 0) {
				RightDownFrame = int.MinValue;
			}

			// Down
			if (FrameInput.KeyPressing(GameKey.Down)) {
				if (DownDownFrame < 0) {
					DownDownFrame = frame;
				}
				if (DownDownFrame > UpDownFrame) {
					y = Direction3.Negative;
				}
			} else if (DownDownFrame > 0) {
				DownDownFrame = int.MinValue;
			}

			// Up
			if (FrameInput.KeyPressing(GameKey.Up)) {
				if (UpDownFrame < 0) {
					UpDownFrame = frame;
				}
				if (UpDownFrame > DownDownFrame) {
					y = Direction3.Positive;
				}
			} else if (UpDownFrame > 0) {
				UpDownFrame = int.MinValue;
			}

			Movement.Move(x, y);

		}


		private void Update_JumpDashPound () {
			Movement.HoldJump(FrameInput.KeyPressing(GameKey.Jump));
			if (FrameInput.KeyDown(GameKey.Jump)) {
				if (FrameInput.KeyPressing(GameKey.Down)) {
					Movement.Dash();
				} else {
					Movement.Jump();
				}
			}
			if (FrameInput.KeyDown(GameKey.Down)) {
				Movement.Pound();
			}
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}

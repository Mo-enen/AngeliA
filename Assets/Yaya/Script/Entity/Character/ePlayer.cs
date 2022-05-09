using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[ExcludeInMapEditor]
	[EntityCapacity(1)]
	[ForceUpdate]
	public abstract class ePlayer : eCharacter {




		#region --- VAR ---


		// Api
		public static ePlayer CurrentPlayer { get; private set; } = null;

		// Data
		private Game Game = null;
		private int LeftDownFrame = int.MinValue;
		private int RightDownFrame = int.MinValue;
		private int DownDownFrame = int.MinValue;
		private int UpDownFrame = int.MinValue;
		private int LastGroundedY = 0;
		private int AimX = 0;
		private int AimY = 0;


		#endregion




		#region --- MSG ---


		public override void OnInitialize () {
			base.OnInitialize();
			Game = Object.FindObjectOfType<Game>();
		}


		public override void OnActived (int frame) {
			LastGroundedY = Y;
			AimX = Game.ViewRect.x;
			AimY = Game.ViewRect.y;
			CurrentPlayer = this;
			base.OnActived(frame);
		}


		public override void FrameUpdate (int frame) {
			if (CurrentPlayer != this) CurrentPlayer = this;
			Update_Move(frame);
			Update_JumpDashPound();
			Update_View();
			base.FrameUpdate(frame);
		}


		private void Update_Move (int frame) {

			if (Movement == null) return;

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
				Movement.Jump();
				if (FrameInput.KeyPressing(GameKey.Down)) {
					Movement.Dash();
				}
			}
			if (FrameInput.KeyDown(GameKey.Down)) {
				Movement.Pound();
			}
		}


		private void Update_View () {
			var viewRect = Game.ViewRect;
			const int LINGER_RATE = 42;
			if (!IsInAir) LastGroundedY = Y;
			int linger = viewRect.width * LINGER_RATE / 1000;
			int centerX = viewRect.x + viewRect.width / 2;
			if (X < centerX - linger) {
				AimX = X + linger - viewRect.width / 2;
			} else if (X > centerX + linger) {
				AimX = X - linger - viewRect.width / 2;
			}
			AimY = !IsInAir || Y < LastGroundedY ? Y - viewRect.height * 382 / 1000 : AimY;
			Game.SetViewPositionDely(AimX, AimY, 62);
			if (!viewRect.Contains(X, Y)) {
				if (X >= viewRect.xMax) AimX = X - viewRect.width + 1;
				if (X <= viewRect.xMin) AimX = X - 1;
				if (Y >= viewRect.yMax) AimY = Y - viewRect.height + 1;
				if (Y <= viewRect.yMin) AimY = Y - 1;
				Game.SetViewPositionDely(AimX, AimY, 1000, int.MinValue + 1);
			}
		}


		public override void OnInactived (int frame) {
			if (CurrentPlayer == this) CurrentPlayer = null;
			base.OnInactived(frame);
		}


		#endregion




	}
}

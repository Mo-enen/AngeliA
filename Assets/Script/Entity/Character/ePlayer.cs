using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class ePlayer : eCharacter {




		#region --- VAR ---


		// Api
		public static ePlayer CurrentPlayer { get; private set; } = null;
		public override int PushLevel => 128;

		// Short
		private RectInt ViewRect => Game.Current.ViewRect;

		// Data
		private int LeftDownFrame = int.MinValue;
		private int RightDownFrame = int.MinValue;
		private int DownDownFrame = int.MinValue;
		private int UpDownFrame = int.MinValue;
		private int LastGroundedY = 0;
		private int AimX = 0;
		private int AimY = 0;


		#endregion




		#region --- MSG ---


		public override void OnCreate (int frame) {
			LastGroundedY = Y;
			AimX = ViewRect.x;
			AimY = ViewRect.y;
			CurrentPlayer = this;
			base.OnCreate(frame);
		}


		public override void FrameUpdate (int frame) {
			if (CurrentPlayer != this) CurrentPlayer = this;
			Update_Move(frame);
			Update_JumpDashPound();
			Update_View();
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
			const int LINGER_RATE = 42;
			if (!IsInAir) LastGroundedY = Y;
			int linger = ViewRect.width * LINGER_RATE / 1000;
			int centerX = ViewRect.x + ViewRect.width / 2;
			if (X < centerX - linger) {
				AimX = X + linger - ViewRect.width / 2;
			} else if (X > centerX + linger) {
				AimX = X - linger - ViewRect.width / 2;
			}
			AimY = !IsInAir || Y < LastGroundedY ? Y - ViewRect.height / 2 : AimY;
			Game.Current.SetViewPositionDely(AimX, AimY, 62);
			if (!ViewRect.Contains(X, Y)) {
				if (X >= ViewRect.xMax) AimX = X - ViewRect.width + 1;
				if (X <= ViewRect.xMin) AimX = X - 1;
				if (Y >= ViewRect.yMax) AimY = Y - ViewRect.height + 1;
				if (Y <= ViewRect.yMin) AimY = Y - 1;
				Game.Current.SetViewPositionDely(AimX, AimY, 1000, int.MinValue + 1);
			}
		}


		public override void OnDespawn (int frame) {
			if (CurrentPlayer == this) CurrentPlayer = null;
			base.OnDespawn(frame);
		}


		#endregion




	}
}

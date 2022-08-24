using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[ExcludeInMapEditor]
	[EntityCapacity(1)]
	[ForceUpdate]
	[EntityBounds(-Const.CELL_SIZE / 2, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2)]
	[DontDestroyOnSquadTransitionAttribute]
	public class ePlayer : eCharacter {




		#region --- VAR ---


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


		public override void OnInitialize (Game game) {
			base.OnInitialize(game);
			Game = game;
		}


		public override void OnActived () {
			LastGroundedY = Y;
			AimX = X - Game.ViewRect.width / 2;
			AimY = GetAimY(Y, Game.ViewRect.height);
			base.OnActived();
		}


		public override void FrameUpdate () {
			switch (CharacterState) {
				case State.General:
					// General
					Update_Move();
					Update_JumpDashPound();
					if (FrameInput.KeyDown(GameKey.Action)) {
						if (!InvokeAction()) InvokeAttack();
					}
					break;
				case State.Sleep:
					// Sleep
					if (FrameInput.KeyDown(GameKey.Action)) {
						Wakeup();
					}
					break;
			}
			Update_View();
			base.FrameUpdate();
		}


		private void Update_Move () {

			int frame = Game.GlobalFrame;
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
			const int LINGER_RATE = 32;
			const int LERP_RATE = 96;
			var viewRect = Game.ViewRect;
			if (!IsInAir) LastGroundedY = Y;
			int linger = viewRect.width * LINGER_RATE / 1000;
			int centerX = viewRect.x + viewRect.width / 2;
			if (X < centerX - linger) {
				AimX = X + linger - viewRect.width / 2;
			} else if (X > centerX + linger) {
				AimX = X - linger - viewRect.width / 2;
			}
			AimY = !IsInAir || Y < LastGroundedY ? GetAimY(Y, viewRect.height) : AimY;
			Game.SetViewPositionDely(AimX, AimY, LERP_RATE, Const.VIEW_PRIORITY_PLAYER);
			// Clamp
			if (!viewRect.Contains(X, Y)) {
				if (X >= viewRect.xMax) AimX = X - viewRect.width + 1;
				if (X <= viewRect.xMin) AimX = X - 1;
				if (Y >= viewRect.yMax) AimY = Y - viewRect.height + 1;
				if (Y <= viewRect.yMin) AimY = Y - 1;
				Game.SetViewPositionDely(AimX, AimY, 1000, Const.VIEW_PRIORITY_PLAYER + 1);
			}
		}


		#endregion




		#region --- API ---




		#endregion



		#region --- LGC ---


		private int GetAimY (int playerY, int viewHeight) => playerY - viewHeight * 382 / 1000;


		#endregion




	}
}

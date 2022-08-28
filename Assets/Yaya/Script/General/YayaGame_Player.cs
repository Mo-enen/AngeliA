using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System.Reflection;

namespace Yaya {
	// === Player ===
	public partial class YayaGame {



		#region --- VAR ---


		// Api
		public eCharacter CurrentPlayer { get; private set; } = null;

		// Data
		private int PlayerTypeID = 0;
		private int LeftDownFrame = int.MinValue;
		private int RightDownFrame = int.MinValue;
		private int DownDownFrame = int.MinValue;
		private int UpDownFrame = int.MinValue;
		private int LastGroundedY = 0;
		private int AimX = 0;
		private int AimY = 0;


		#endregion




		#region --- MSG ---


		public void Initialize_Player () {
			int firstPlayerID = 0;
			foreach (var type in typeof(ePlayer).AllChildClass()) {
				firstPlayerID = type.AngeHash();
				if (type.GetCustomAttribute<FirstSelectedPlayerAttribute>(true) != null) break;
			}
			SwitchPlayer(firstPlayerID, false);
			FrameStep.AddToLast(new sOpening());
		}


		public void Update_Player () {

			// Spawn Player when No Player Entity
			if (CurrentPlayer == null && !FrameStep.HasStep<sOpening>()) {
				var center = CellRenderer.CameraRect.CenterInt();
				SpawnPlayer(center.x, center.y, false);
			}

			// Reload Game and Player After Passout
			if (
				CurrentPlayer != null &&
				CurrentPlayer.Active &&
				CurrentPlayer.CharacterState == eCharacter.State.Passout &&
				GlobalFrame > CurrentPlayer.PassoutFrame + 48 &&
				FrameInput.KeyDown(GameKey.Action) &&
				!FrameStep.HasStep<sOpening>()
			) {
				FrameStep.AddToLast(new sFadeOut());
				FrameStep.AddToLast(new sOpening());
			}

			// Player Update
			if (CurrentPlayer != null) {
				switch (CurrentPlayer.CharacterState) {
					case eCharacter.State.General:
						// General
						Update_Move();
						Update_JumpDashPound();
						if (FrameInput.KeyDown(GameKey.Action)) {
							if (!CurrentPlayer.InvokeAction()) CurrentPlayer.Attackness.Attack();
						}
						break;
					case eCharacter.State.Sleep:
						// Sleep
						if (FrameInput.KeyDown(GameKey.Action)) {
							CurrentPlayer.Wakeup();
						}
						break;
				}
				Update_View();
			}

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

			CurrentPlayer.Movement.Move(x, y);

		}


		private void Update_JumpDashPound () {
			CurrentPlayer.Movement.HoldJump(FrameInput.KeyPressing(GameKey.Jump));
			if (FrameInput.KeyDown(GameKey.Jump)) {
				CurrentPlayer.Movement.Jump();
				if (FrameInput.KeyPressing(GameKey.Down)) {
					CurrentPlayer.Movement.Dash();
				}
			}
			if (FrameInput.KeyDown(GameKey.Down)) {
				CurrentPlayer.Movement.Pound();
			}
		}


		private void Update_View () {
			const int LINGER_RATE = 32;
			const int LERP_RATE = 96;
			var viewRect = ViewRect;
			if (!CurrentPlayer.IsInAir) LastGroundedY = CurrentPlayer.Y;
			int linger = viewRect.width * LINGER_RATE / 1000;
			int centerX = viewRect.x + viewRect.width / 2;
			if (CurrentPlayer.X < centerX - linger) {
				AimX = CurrentPlayer.X + linger - viewRect.width / 2;
			} else if (CurrentPlayer.X > centerX + linger) {
				AimX = CurrentPlayer.X - linger - viewRect.width / 2;
			}
			AimY = !CurrentPlayer.IsInAir || CurrentPlayer.Y < LastGroundedY ? GetAimY(CurrentPlayer.Y, viewRect.height) : AimY;
			SetViewPositionDely(AimX, AimY, LERP_RATE, Const.VIEW_PRIORITY_PLAYER);
			// Clamp
			if (!viewRect.Contains(CurrentPlayer.X, CurrentPlayer.Y)) {
				if (CurrentPlayer.X >= viewRect.xMax) AimX = CurrentPlayer.X - viewRect.width + 1;
				if (CurrentPlayer.X <= viewRect.xMin) AimX = CurrentPlayer.X - 1;
				if (CurrentPlayer.Y >= viewRect.yMax) AimY = CurrentPlayer.Y - viewRect.height + 1;
				if (CurrentPlayer.Y <= viewRect.yMin) AimY = CurrentPlayer.Y - 1;
				SetViewPositionDely(AimX, AimY, 1000, Const.VIEW_PRIORITY_PLAYER + 1);
			}
		}



		#endregion




		#region --- API ---


		public void SwitchPlayer (int newPlayerID, bool spawnEntity) => SwitchPlayer(newPlayerID, spawnEntity, CellRenderer.CameraRect.CenterInt());
		public void SwitchPlayer (int newPlayerID, bool spawnEntity, Vector2Int failbackPosition) {
			if (!IsEntityIdValid(newPlayerID)) return;
			PlayerTypeID = newPlayerID;
			if (spawnEntity) SpawnPlayer(failbackPosition.x, failbackPosition.y, false);
		}


		public void SpawnPlayer (int x, int y, bool trySpawnToBed) {

			var pos = new Vector2Int(x, y);

			// Try Get Existing Entity as Player
			if (CurrentPlayer == null && TryGetEntityInStage<ePlayer>(out var result)) {
				CurrentPlayer = result;
			}

			// Spawn to Bed
			if (trySpawnToBed) {
				// Find Best Bed
				eBed finalBed = null;
				int finalDistance = int.MaxValue;
				int count = StagedEntities.Length;
				for (int i = 0; i < count; i++) {
					var e = StagedEntities[i];
					if (e == null) break;
					if (e is not eBed bed) continue;
					if (finalBed == null) {
						finalBed = bed;
						finalDistance = Util.SqrtDistance(bed.Rect.position, pos);
					} else {
						int dis = Util.SqrtDistance(bed.Rect.position, pos);
						if (dis < finalDistance) {
							finalDistance = dis;
							finalBed = bed;
						}
					}
				}
				// Spawn on Bed
				if (finalBed != null) {
					CurrentPlayer ??= AddEntity(
						PlayerTypeID,
						finalBed.X, finalBed.Y
					) as eCharacter;
					finalBed.Invoke(CurrentPlayer);
				}
			}

			// Failback
			CurrentPlayer ??= AddEntity(
				PlayerTypeID, pos.x, pos.y
			) as eCharacter;

			// Init Player
			if (CurrentPlayer != null) {
				LastGroundedY = CurrentPlayer.Y;
				AimX = CurrentPlayer.X - ViewRect.width / 2;
				AimY = GetAimY(CurrentPlayer.Y, ViewRect.height);
			}

		}


		#endregion



		#region --- LGC ---


		private int GetAimY (int playerY, int viewHeight) => playerY - viewHeight * 382 / 1000;


		#endregion




	}
}
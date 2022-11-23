using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System.Reflection;


namespace Yaya {
	public partial class Yaya {




		#region --- VAR ---


		// Const
		private const int ATTACK_REQUIRE_GAP = 12;
		private const int VIEW_X = 10 * Const.CEL;
		private const int VIEW_Y_START = 19 * Const.CEL;
		private const int VIEW_Y_END = 8 * Const.CEL;

		// Api
		public ePlayer CurrentPlayer { get; private set; } = null;
		public int AimViewX { get; private set; } = 0;
		public int AimViewY { get; private set; } = 0;

		// Data
		private int PlayerTypeID = 0;
		private int LeftDownFrame = int.MinValue;
		private int RightDownFrame = int.MinValue;
		private int DownDownFrame = int.MinValue;
		private int UpDownFrame = int.MinValue;
		private int AttackRequiringFrame = int.MinValue;
		private int PlayerLastGroundedY = 0;


		#endregion




		#region --- MSG ---


		public void Initialize_Player () {

			// Switch Player
			int firstPlayerID = 0;
			foreach (var type in typeof(ePlayer).AllChildClass()) {
				firstPlayerID = type.AngeHash();
				if (type.GetCustomAttribute<FirstSelectedPlayerAttribute>(true) != null) break;
			}
			SwitchPlayer(firstPlayerID, false);

		}


		public void Update_Player () {

			// Stop when Not Playing
			if (State != GameState.Play) {
				if (CurrentPlayer != null) {
					CurrentPlayer.Movement.Stop();
				}
				return;
			}

			// Respawn
			UpdatePlayer_Respawn();

			// Update Player
			if (CurrentPlayer == null) return;
			bool hasRoute = FrameTask.HasTask(Const.TASK_ROUTE);
			switch (CurrentPlayer.CharacterState) {
				case CharacterState.GamePlay:
					if (!hasRoute) {
						UpdatePlayer_Move();
						UpdatePlayer_JumpDashPound();
						UpdatePlayer_Action_Attack();
					} else {
						CurrentPlayer.Movement.Stop();
					}
					break;
				case CharacterState.Sleep:
					if (!hasRoute) {
						UpdatePlayer_Sleep();
					}
					break;
			}
			if (!hasRoute) {
				UpdatePlayer_View();
			}
		}


		private void UpdatePlayer_Respawn () {

			// Spawn Player when No Player Entity
			if (CurrentPlayer != null && !CurrentPlayer.Active) CurrentPlayer = null;
			if (CurrentPlayer == null && !FrameTask.HasTask(Const.TASK_ROUTE)) {
				var center = CellRenderer.CameraRect.CenterInt();
				SpawnPlayer(center.x, center.y);
			}

			// Reload Game and Player After Passout
			if (
				CurrentPlayer != null && CurrentPlayer.Active &&
				CurrentPlayer.CharacterState == CharacterState.Passout &&
				GlobalFrame > CurrentPlayer.PassoutFrame + YayaConst.PASSOUT_WAIT &&
				FrameInput.GetGameKeyDown(GameKey.Action) &&
				!FrameTask.HasTask(Const.TASK_ROUTE)
			) {
				FrameTask.AddToLast(tFadeOut.TYPE_ID, Const.TASK_ROUTE);
				if (FrameTask.TryAddToLast(tOpening.TYPE_ID, Const.TASK_ROUTE, out var task) && task is tOpening oTask) {
					oTask.ViewX = VIEW_X;
					oTask.ViewYStart = VIEW_Y_START;
					oTask.ViewYEnd = VIEW_Y_END;
					oTask.RemovePlayerAtStart = true;
					oTask.SpawnPlayerAtStart = true;
				}
			}
		}


		private void UpdatePlayer_Move () {

			int frame = GlobalFrame;
			var x = Direction3.None;
			var y = Direction3.None;

			// Left
			if (FrameInput.GetGameKey(GameKey.Left)) {
				if (LeftDownFrame < 0) {
					LeftDownFrame = frame;
					AttackRequiringFrame = int.MinValue;
				}
				if (LeftDownFrame > RightDownFrame) {
					x = Direction3.Negative;
				}
			} else if (LeftDownFrame > 0) {
				LeftDownFrame = int.MinValue;
			}

			// Right
			if (FrameInput.GetGameKey(GameKey.Right)) {
				if (RightDownFrame < 0) {
					RightDownFrame = frame;
					AttackRequiringFrame = int.MinValue;
				}
				if (RightDownFrame > LeftDownFrame) {
					x = Direction3.Positive;
				}
			} else if (RightDownFrame > 0) {
				RightDownFrame = int.MinValue;
			}

			if (!CurrentPlayer.Action.LockingInput) {
				// Down
				if (FrameInput.GetGameKey(GameKey.Down)) {
					if (DownDownFrame < 0) {
						DownDownFrame = frame;
						AttackRequiringFrame = int.MinValue;
					}
					if (DownDownFrame > UpDownFrame) {
						y = Direction3.Negative;
					}
				} else if (DownDownFrame > 0) {
					DownDownFrame = int.MinValue;
				}

				// Up
				if (FrameInput.GetGameKey(GameKey.Up)) {
					if (UpDownFrame < 0) {
						UpDownFrame = frame;
						AttackRequiringFrame = int.MinValue;
					}
					if (UpDownFrame > DownDownFrame) {
						y = Direction3.Positive;
					}
				} else if (UpDownFrame > 0) {
					UpDownFrame = int.MinValue;
				}

			}

			// Final
			CurrentPlayer.Movement.Move(x, y);
		}


		private void UpdatePlayer_JumpDashPound () {

			if (CurrentPlayer.Action.LockingInput) return;

			var movement = CurrentPlayer.Movement;
			var attack = CurrentPlayer.Attackness;
			movement.HoldJump(FrameInput.GetGameKey(GameKey.Jump));
			if (FrameInput.GetGameKeyDown(GameKey.Jump)) {
				// Movement Jump
				movement.Jump();
				if (FrameInput.GetGameKey(GameKey.Down)) {
					movement.Dash();
				}
				AttackRequiringFrame = int.MinValue;
				if (attack.CancelAttackOnJump) {
					attack.CancelAttack();
				}
			}
			if (FrameInput.GetGameKeyDown(GameKey.Down)) {
				movement.Pound();
			}
		}


		private void UpdatePlayer_Action_Attack () {

			var action = CurrentPlayer.Action;
			var attack = CurrentPlayer.Attackness;

			// Try Perform Action
			if (action.CurrentTarget != null && FrameInput.AnyGameKeyDown()) {
				bool performed = action.Invoke();
				if (performed) return;
			}

			// Try Cancel Action
			if (action.CurrentTarget != null && FrameInput.GetGameKeyDown(GameKey.Jump)) {
				action.CancelInvoke();
				return;
			}

			// Lock Input Check
			if (action.LockingInput) return;

			// Try Perform Attack
			if (CurrentPlayer.CharacterState == CharacterState.GamePlay) {
				bool attDown = FrameInput.GetGameKeyDown(GameKey.Action);
				bool attHolding = FrameInput.GetGameKey(GameKey.Action) && attack.KeepTriggerWhenHold;
				if (attDown || attHolding) {
					if (CurrentPlayer.IsAttackAllowedByMovement()) {
						if (attack.CheckReady(!attDown)) {
							attack.Attack();
						} else if (attDown) {
							AttackRequiringFrame = GlobalFrame;
						}
					}
					return;
				}
			}

			// Perform Required Attack
			if (attack.CheckReady(false) && GlobalFrame < AttackRequiringFrame + ATTACK_REQUIRE_GAP) {
				AttackRequiringFrame = int.MinValue;
				attack.Attack();
			}
		}


		private void UpdatePlayer_Sleep () {
			if (FrameInput.GetGameKeyDown(GameKey.Action) || FrameInput.GetGameKeyDown(GameKey.Jump)) {
				CurrentPlayer.SetCharacterState(CharacterState.GamePlay);
				CurrentPlayer.Y -= 2;
			}
		}


		private void UpdatePlayer_View () {

			const int LINGER_RATE = 32;
			var viewRect = ViewRect;
			bool flying = CurrentPlayer.Movement.IsFlying;
			int playerX = CurrentPlayer.X;
			int playerY = CurrentPlayer.Y;
			bool inAir = CurrentPlayer.InAir;

			if (!inAir || flying) PlayerLastGroundedY = playerY;
			int linger = viewRect.width * LINGER_RATE / 1000;
			int centerX = viewRect.x + viewRect.width / 2;
			if (playerX < centerX - linger) {
				AimViewX = playerX + linger - viewRect.width / 2;
			} else if (playerX > centerX + linger) {
				AimViewX = playerX - linger - viewRect.width / 2;
			}
			AimViewY = !inAir || flying || playerY < PlayerLastGroundedY ? GetCameraY(playerY, viewRect.height) : AimViewY;
			SetViewPositionDely(AimViewX, AimViewY, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_PLAYER);

			// Clamp
			if (!viewRect.Contains(playerX, playerY)) {
				if (playerX >= viewRect.xMax) AimViewX = playerX - viewRect.width + 1;
				if (playerX <= viewRect.xMin) AimViewX = playerX - 1;
				if (playerY >= viewRect.yMax) AimViewY = playerY - viewRect.height + 1;
				if (playerY <= viewRect.yMin) AimViewY = playerY - 1;
				SetViewPositionDely(AimViewX, AimViewY, 1000, YayaConst.VIEW_PRIORITY_PLAYER + 1);
			}

		}


		#endregion




		#region --- API ---


		public void SwitchPlayer (int newPlayerID, bool spawnEntity) => SwitchPlayer(newPlayerID, spawnEntity, CellRenderer.CameraRect.CenterInt());
		public void SwitchPlayer (int newPlayerID, bool spawnEntity, Vector2Int failbackPosition) {
			if (!IsEntityIdValid(newPlayerID)) return;
			PlayerTypeID = newPlayerID;
			if (spawnEntity) SpawnPlayer(failbackPosition.x, failbackPosition.y);
		}


		public ePlayer SpawnPlayer (int x, int y, bool removeExists = true) {

			var pos = new Vector2Int(x, y);

			// Remove Exists
			if (removeExists) {
				for (int i = 0; i < EntityLen; i++) {
					var e = StagedEntities[i];
					if (e is ePlayer && e.TypeID != PlayerTypeID) {
						e.Active = false;
					}
				}
			}

			// Try Get Existing Entity as Player
			if (CurrentPlayer == null && TryGetEntity(PlayerTypeID, out var result)) {
				CurrentPlayer = result as ePlayer;
			}

			// Try Add Player
			if (CurrentPlayer == null && TryAddEntity(PlayerTypeID, pos.x, pos.y, out var entity) && entity is ePlayer player) {
				CurrentPlayer = player;
			}

			if (CurrentPlayer == null) return CurrentPlayer;

			// Init Player
			PlayerLastGroundedY = CurrentPlayer.Y;
			AimViewX = CurrentPlayer.X - ViewRect.width / 2;
			AimViewY = GetCameraY(CurrentPlayer.Y, ViewRect.height);
			return CurrentPlayer;
		}


		#endregion




		#region --- LGC ---


		private int GetCameraY (int playerY, int viewHeight) => playerY - viewHeight * 382 / 1000;


		#endregion




	}
}
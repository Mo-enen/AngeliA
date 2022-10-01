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
		private const int VIEW_X = 10 * Const.CELL_SIZE;
		private const int VIEW_Y_START = 19 * Const.CELL_SIZE;
		private const int VIEW_Y_END = 8 * Const.CELL_SIZE;

		// Api
		public ePlayer CurrentPlayer { get; private set; } = null;

		// Data
		private int PlayerTypeID = 0;
		private int LeftDownFrame = int.MinValue;
		private int RightDownFrame = int.MinValue;
		private int DownDownFrame = int.MinValue;
		private int UpDownFrame = int.MinValue;
		private int LastGroundedY = 0;
		private int AimX = 0;
		private int AimY = 0;
		private int AttackRequiringFrame = int.MinValue;


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

			// Gamepad UI
			FrameInput.AddCustomKey(KeyCode.F2);

			// Open the Game !!
			FrameStep.AddToLast(new sOpening() {
				ViewX = VIEW_X,
				ViewYStart = VIEW_Y_START,
				ViewYEnd = VIEW_Y_END,
				SpawnPlayerAtStart = true,
				RemovePlayerAtStart = true,
			});
		}


		public void Update_Player () {
			UpdatePlayer_Respawn();
			if (CurrentPlayer == null) return;
			switch (CurrentPlayer.CharacterState) {
				case CharacterState.GamePlay:
					UpdatePlayer_Move();
					UpdatePlayer_JumpDashPound();
					UpdatePlayer_Action_Attack();
					break;
				case CharacterState.Sleep:
					UpdatePlayer_Sleep();
					break;
			}
			UpdatePlayer_View();
		}


		private void UpdatePlayer_Respawn () {

			// Spawn Player when No Player Entity
			if (CurrentPlayer != null && !CurrentPlayer.Active) CurrentPlayer = null;
			if (CurrentPlayer == null && !FrameStep.HasStep<sOpening>()) {
				var center = CellRenderer.CameraRect.CenterInt();
				SpawnPlayer(center.x, center.y);
			}

			// Reload Game and Player After Passout
			if (
				CurrentPlayer != null && CurrentPlayer.Active &&
				CurrentPlayer.CharacterState == CharacterState.Passout &&
				GlobalFrame > CurrentPlayer.PassoutFrame + 48 &&
				FrameInput.KeyDown(GameKey.Action) &&
				!FrameStep.HasStep<sOpening>()
			) {
				FrameStep.AddToLast(new sFadeOut());
				FrameStep.AddToLast(new sOpening() {
					ViewX = VIEW_X,
					ViewYStart = VIEW_Y_START,
					ViewYEnd = VIEW_Y_END,
					RemovePlayerAtStart = true,
					SpawnPlayerAtStart = true,
				});
			}
		}


		private void UpdatePlayer_Move () {

			int frame = GlobalFrame;
			var x = Direction3.None;
			var y = Direction3.None;

			// Left
			if (FrameInput.KeyPressing(GameKey.Left)) {
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
			if (FrameInput.KeyPressing(GameKey.Right)) {
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

			// Down
			if (FrameInput.KeyPressing(GameKey.Down)) {
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
			if (FrameInput.KeyPressing(GameKey.Up)) {
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

			CurrentPlayer.Movement.Move(x, y);
		}


		private void UpdatePlayer_JumpDashPound () {

			if (CurrentPlayer.Action.CurrentTarget is eOpenableFurniture oFur && oFur.Open) return;

			var movement = CurrentPlayer.Movement;
			var attack = CurrentPlayer.Attackness;
			movement.HoldJump(FrameInput.KeyPressing(GameKey.Jump));
			if (FrameInput.KeyDown(GameKey.Jump)) {
				// Movement Jump
				movement.Jump();
				if (FrameInput.KeyPressing(GameKey.Down)) {
					movement.Dash();
				}
				AttackRequiringFrame = int.MinValue;
				if (attack.CancelAttackOnJump) {
					attack.CancelAttack();
				}
			}
			if (FrameInput.KeyDown(GameKey.Down)) {
				movement.Pound();
			}
		}


		private void UpdatePlayer_Action_Attack () {

			var action = CurrentPlayer.Action;
			var attack = CurrentPlayer.Attackness;

			// Try Perform Action
			if (FrameInput.AnyGameKeyDown()) {
				bool performed = action.Invoke();
				if (performed) return;
			}

			// Try Cancel Action
			if (action.CurrentTarget != null) {
				if (FrameInput.KeyDown(GameKey.Jump)) {
					action.CancelInvoke();
				}
				return;
			}

			// Try Perform Attack
			bool attDown = FrameInput.KeyDown(GameKey.Action);
			bool attHolding = FrameInput.KeyPressing(GameKey.Action) && attack.KeepTriggerWhenHold;
			if (CurrentPlayer.CharacterState == CharacterState.GamePlay && (attDown || attHolding)) {
				if (CurrentPlayer.IsAttackAllowedByMovement()) {
					if (attack.CheckReady(!attDown)) {
						attack.Attack();
					} else if (attDown) {
						AttackRequiringFrame = GlobalFrame;
					}
				}
				return;
			}

			// Perform Required Attack
			if (attack.CheckReady(false) && GlobalFrame < AttackRequiringFrame + ATTACK_REQUIRE_GAP) {
				AttackRequiringFrame = int.MinValue;
				attack.Attack();
			}
		}


		private void UpdatePlayer_Sleep () {
			if (FrameStep.HasStep<sOpening>()) return;
			if (FrameInput.KeyDown(GameKey.Action) || FrameInput.KeyDown(GameKey.Jump)) {
				CurrentPlayer.SetCharacterState(CharacterState.GamePlay);
				CurrentPlayer.Y -= 2;
			}
		}


		private void UpdatePlayer_View () {

			if (FrameStep.HasStep<sOpening>()) return;

			const int LINGER_RATE = 32;
			const int LERP_RATE = 96;
			var viewRect = ViewRect;
			if (!CurrentPlayer.InAir) LastGroundedY = CurrentPlayer.Y;
			int linger = viewRect.width * LINGER_RATE / 1000;
			int centerX = viewRect.x + viewRect.width / 2;
			if (CurrentPlayer.X < centerX - linger) {
				AimX = CurrentPlayer.X + linger - viewRect.width / 2;
			} else if (CurrentPlayer.X > centerX + linger) {
				AimX = CurrentPlayer.X - linger - viewRect.width / 2;
			}
			AimY = !CurrentPlayer.InAir || CurrentPlayer.Y < LastGroundedY ? GetAimY(CurrentPlayer.Y, viewRect.height) : AimY;
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
			if (CurrentPlayer == null && TryGetEntityInStage(PlayerTypeID, out var result)) {
				CurrentPlayer = result as ePlayer;
			}

			// Try Add Player
			if (CurrentPlayer == null && TryAddEntity(PlayerTypeID, pos.x, pos.y, out var entity) && entity is ePlayer player) {
				CurrentPlayer = player;
			}

			if (CurrentPlayer == null) return CurrentPlayer;

			// Init Player
			LastGroundedY = CurrentPlayer.Y;
			AimX = CurrentPlayer.X - ViewRect.width / 2;
			AimY = GetAimY(CurrentPlayer.Y, ViewRect.height);
			return CurrentPlayer;
		}


		#endregion




		#region --- LGC ---


		private int GetAimY (int playerY, int viewHeight) => playerY - viewHeight * 382 / 1000;


		#endregion




	}
}
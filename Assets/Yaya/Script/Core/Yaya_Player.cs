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

		}


		public void Update_Player () {
			if (State != GameState.Play) {
				if (CurrentPlayer != null) {
					CurrentPlayer.Movement.Move(Direction3.None, Direction3.None);
				}
				return;
			}
			UpdatePlayer_Respawn();
			if (CurrentPlayer == null) return;
			bool hasRoute = FrameStep.HasStep(YayaConst.STEP_ROUTE);
			switch (CurrentPlayer.CharacterState) {
				case CharacterState.GamePlay:
					if (!hasRoute) {
						UpdatePlayer_Move();
						UpdatePlayer_JumpDashPound();
						UpdatePlayer_Action_Attack();
					} else {
						CurrentPlayer.Movement.Move(Direction3.None, Direction3.None);
					}
					break;
				case CharacterState.Sleep:
					if (!hasRoute) {
						UpdatePlayer_Sleep();
					}
					break;
			}
		}


		private void UpdatePlayer_Respawn () {

			// Spawn Player when No Player Entity
			if (CurrentPlayer != null && !CurrentPlayer.Active) CurrentPlayer = null;
			if (CurrentPlayer == null && !FrameStep.HasStep(YayaConst.STEP_ROUTE)) {
				var center = CellRenderer.CameraRect.CenterInt();
				SpawnPlayer(center.x, center.y);
			}

			// Reload Game and Player After Passout
			if (
				CurrentPlayer != null && CurrentPlayer.Active &&
				CurrentPlayer.CharacterState == CharacterState.Passout &&
				GlobalFrame > CurrentPlayer.PassoutFrame + YayaConst.PASSOUT_WAIT &&
				FrameInput.GetKeyDown(GameKey.Action) &&
				!FrameStep.HasStep(YayaConst.STEP_ROUTE)
			) {
				FrameStep.AddToLast(YayaConst.FADEOUT_ID, YayaConst.STEP_ROUTE);
				if (FrameStep.TryAddToLast<sOpening>(YayaConst.OPENING_ID, YayaConst.STEP_ROUTE, out var step)) {
					step.ViewX = VIEW_X;
					step.ViewYStart = VIEW_Y_START;
					step.ViewYEnd = VIEW_Y_END;
					step.RemovePlayerAtStart = true;
					step.SpawnPlayerAtStart = true;
				}
			}
		}


		private void UpdatePlayer_Move () {

			int frame = GlobalFrame;
			var x = Direction3.None;
			var y = Direction3.None;

			// Left
			if (FrameInput.GetKey(GameKey.Left)) {
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
			if (FrameInput.GetKey(GameKey.Right)) {
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
				if (FrameInput.GetKey(GameKey.Down)) {
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
				if (FrameInput.GetKey(GameKey.Up)) {
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
			movement.HoldJump(FrameInput.GetKey(GameKey.Jump));
			if (FrameInput.GetKeyDown(GameKey.Jump)) {
				// Movement Jump
				movement.Jump();
				if (FrameInput.GetKey(GameKey.Down)) {
					movement.Dash();
				}
				AttackRequiringFrame = int.MinValue;
				if (attack.CancelAttackOnJump) {
					attack.CancelAttack();
				}
			}
			if (FrameInput.GetKeyDown(GameKey.Down)) {
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
			if (action.CurrentTarget != null && FrameInput.GetKeyDown(GameKey.Jump)) {
				action.CancelInvoke();
				return;
			}

			// Lock Input Check
			if (action.LockingInput) return;

			// Try Perform Attack
			if (CurrentPlayer.CharacterState == CharacterState.GamePlay) {
				bool attDown = FrameInput.GetKeyDown(GameKey.Action);
				bool attHolding = FrameInput.GetKey(GameKey.Action) && attack.KeepTriggerWhenHold;
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
			if (FrameInput.GetKeyDown(GameKey.Action) || FrameInput.GetKeyDown(GameKey.Jump)) {
				CurrentPlayer.SetCharacterState(CharacterState.GamePlay);
				CurrentPlayer.Y -= 2;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System.Reflection;

namespace Yaya {
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.Bounds(-Const.HALF, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.MapEditorGroup("Player")]
	public abstract class ePlayer : eCharacter, IGlobalPosition {




		#region --- VAR ---


		// Const
		private const int RUSH_TAPPING_GAP = 16;

		// Api
		public static ePlayer Selecting { get; private set; } = null;
		public override bool IsChargingAttack => MinimalChargeAttackDuration != int.MaxValue && !IsSafe && AttackCooldownReady(false) && FrameInput.GameKeyHolding(Gamekey.Action);
		public override int Team => YayaConst.TEAM_PLAYER;
		public int AimViewX { get; private set; } = 0;
		public int AimViewY { get; private set; } = 0;

		// Data
		private static readonly PhysicsCell[] Collects = new PhysicsCell[8];
		private int AttackRequiringFrame = int.MinValue;
		private int LastLeftKeyDown = int.MinValue;
		private int LastRightKeyDown = int.MinValue;
		private int LastGroundedY = 0;


		#endregion




		#region --- MSG ---


		public ePlayer () {
			// Select First Player
			if (Selecting == null) {
				// First Player
				int firstSelectID = 0;
				foreach (var type in typeof(ePlayer).AllChildClass()) {
					firstSelectID = type.AngeHash();
					if (type.GetCustomAttribute<FirstSelectedPlayerAttribute>(true) != null) break;
				}
				if (firstSelectID == GetType().AngeHash()) {
					Selecting = this;
				}
			}
		}


		public override void FillPhysics () {
			if (FrameTask.HasTask(YayaConst.TASK_ROUTE)) return;
			base.FillPhysics();
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();

			// Stop when Not Playing
			if (Selecting != this || Game.Current.State != GameState.Play) {
				Stop();
				return;
			}

			// Update Player
			switch (CharacterState) {
				case CharacterState.GamePlay:
					if (!FrameTask.HasTask(YayaConst.TASK_ROUTE)) {
						Move(FrameInput.DirectionX, FrameInput.DirectionY);
						Update_JumpDashPoundRush();
						Update_Action_Attack();
						eControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, WORD.HINT_MOVE);
					} else {
						Stop();
					}
					break;
				case CharacterState.Sleep:
					if (!FrameTask.HasTask(YayaConst.TASK_ROUTE)) {
						Update_Sleep();
					}
					break;
				case CharacterState.Passout:
					// Passout Hint
					if (IsFullPassout) {
						int x = X - Const.HALF;
						int y = Y + Const.CEL * 3 / 2;
						eControlHintUI.DrawGlobalHint(x, y, Gamekey.Action, WORD.UI_CONTINUE, true, true);
					}
					break;
			}

			// View
			Update_View();

		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			PhysicsUpdate_Collect();
		}


		private void PhysicsUpdate_Collect () {
			if (Selecting != this) return;
			int count = CellPhysics.OverlapAll(
				Collects, YayaConst.MASK_ENTITY, Rect, this, OperationMode.TriggerOnly
			);
			for (int i = 0; i < count; i++) {
				var hit = Collects[i];
				if (hit.Entity is not eCollectable col) continue;
				bool success = col.OnCollect(this);
				if (success) {
					hit.Entity.Active = false;
				}
			}
		}


		private void Update_JumpDashPoundRush () {

			if (LockingInput) return;

			eControlHintUI.AddHint(Gamekey.Jump, WORD.HINT_JUMP);

			// Jump/Dash
			HoldJump(FrameInput.GameKeyHolding(Gamekey.Jump));
			if (FrameInput.GameKeyDown(Gamekey.Jump)) {
				// Movement Jump
				if (FrameInput.GameKeyHolding(Gamekey.Down)) {
					Dash();
				} else {
					Jump();
					AttackRequiringFrame = int.MinValue;
				}
			}

			// Pound
			if (FrameInput.GameKeyDown(Gamekey.Down)) {
				Pound();
			}

			// Rush
			if (FrameInput.GameKeyDown(Gamekey.Left)) {
				if (Game.GlobalFrame < LastLeftKeyDown + RUSH_TAPPING_GAP) {
					Rush();
				}
				LastLeftKeyDown = Game.GlobalFrame;
				LastRightKeyDown = int.MinValue;
			}
			if (FrameInput.GameKeyDown(Gamekey.Right)) {
				if (Game.GlobalFrame < LastRightKeyDown + RUSH_TAPPING_GAP) {
					Rush();
				}
				LastRightKeyDown = Game.GlobalFrame;
				LastLeftKeyDown = int.MinValue;
			}

		}


		private void Update_Action_Attack () {

			// Try Perform Action
			if (CurrentActionTarget != null) {
				CurrentActionTarget.Highlight();
				eControlHintUI.AddHint(
					Gamekey.Action,
					eActionEntity.GetHintLanguageCode(CurrentActionTarget.TypeID)
				);
				if (FrameInput.GameKeyDown(Gamekey.Action)) {
					bool performed = InvokeAction();
					if (performed) return;
				}
			} else if (!IsSafe) {
				eControlHintUI.AddHint(Gamekey.Action, WORD.HINT_ATTACK);
			}

			// Try Cancel Action
			if (CurrentActionTarget != null && FrameInput.GameKeyDown(Gamekey.Jump)) {
				CancelInvokeAction();
				return;
			}

			// Lock Input Check
			if (LockingInput) return;

			// Try Perform Attack
			if (CharacterState == CharacterState.GamePlay) {
				bool attDown = FrameInput.GameKeyDown(Gamekey.Action);
				bool attHolding = FrameInput.GameKeyHolding(Gamekey.Action) && KeepAttackWhenHold;
				if (attDown || attHolding) {
					if (IsAttackAllowedByMovement()) {
						if (AttackCooldownReady(!attDown)) {
							Attack();
						} else if (attDown) {
							AttackRequiringFrame = Game.GlobalFrame;
						}
					}
					return;
				}
			}
			if (
				FrameInput.GameKeyDown(Gamekey.Left) || FrameInput.GameKeyDown(Gamekey.Right) ||
				FrameInput.GameKeyDown(Gamekey.Down) || FrameInput.GameKeyDown(Gamekey.Up)
			) {
				AttackRequiringFrame = int.MinValue;
			}

			// Perform Required Attack
			const int ATTACK_REQUIRE_GAP = 12;
			if (IsAttackAllowedByMovement() && AttackCooldownReady(false) && Game.GlobalFrame < AttackRequiringFrame + ATTACK_REQUIRE_GAP) {
				AttackRequiringFrame = int.MinValue;
				Attack();
			}

		}


		private void Update_Sleep () {
			if (IsExactlyFullSleeped) Game.Current.ClearAntiSpawn(true);
			// Wake up on Press Action
			if (FrameInput.GameKeyDown(Gamekey.Action) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				SetCharacterState(CharacterState.GamePlay);
				Y -= 4;
				MakeSafe(6);
			}
			// Ctrl Hint
			int x = X - Const.HALF;
			int y = Y + Const.CEL * 3 / 2;
			eControlHintUI.DrawGlobalHint(x, y, Gamekey.Action, WORD.HINT_WAKE, true, true);
			eControlHintUI.AddHint(Gamekey.Action, WORD.HINT_WAKE);
		}


		private void Update_View () {

			const int LINGER_RATE = 32;
			bool notInGameplay = FrameTask.IsTasking<OpeningTask>(YayaConst.TASK_ROUTE) || CharacterState != CharacterState.GamePlay;
			bool notInAir =
				notInGameplay ||
				IsGrounded || InWater || InSand || IsSliding ||
				IsClimbing || IsGrabingSide || IsGrabingTop;

			if (notInAir || IsFlying) LastGroundedY = Y;
			var game = Game.Current;

			// Aim X
			int linger = game.ViewRect.width * LINGER_RATE / 1000;
			int centerX = game.ViewRect.x + game.ViewRect.width / 2;
			if (notInGameplay) {
				AimViewX = X - game.ViewRect.width / 2;
			} else if (X < centerX - linger) {
				AimViewX = X + linger - game.ViewRect.width / 2;
			} else if (X > centerX + linger) {
				AimViewX = X - linger - game.ViewRect.width / 2;
			}

			// Aim Y
			AimViewY = Y <= LastGroundedY ? Y - GetCameraShiftOffset(game.ViewRect.height) : AimViewY;

			game.SetViewPositionDelay(AimViewX, AimViewY, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_PLAYER);

			// Clamp
			if (!game.ViewRect.Contains(X, Y)) {
				if (X >= game.ViewRect.xMax) AimViewX = X - game.ViewRect.width + 1;
				if (X <= game.ViewRect.xMin) AimViewX = X - 1;
				if (Y >= game.ViewRect.yMax) AimViewY = Y - game.ViewRect.height + 1;
				if (Y <= game.ViewRect.yMin) AimViewY = Y - 1;
				game.SetViewPositionDelay(AimViewX, AimViewY, 1000, YayaConst.VIEW_PRIORITY_PLAYER + 1);
			}

		}


		#endregion




		#region --- API ---


		public static ePlayer TrySpawnSelectingPlayer (int x, int y) {
			if (Selecting == null) return null;
			if (!Selecting.Active) {
				return Game.Current.SpawnEntity(Selecting.TypeID, x, y) as ePlayer;
			}
			Selecting.X = x;
			Selecting.Y = y;
			return Selecting;
		}


		public static void SelectPlayer (ePlayer newPlayer) {
			if (newPlayer == null || !newPlayer.Active || newPlayer == Selecting) return;
			Selecting = newPlayer;
		}


		public static int GetCameraShiftOffset (int cameraHeight) => cameraHeight * 382 / 1000;


		public Vector3Int GetHomePosition () => GlobalPosition.TryGetFirstGlobalUnitPosition(TypeID, out var pos) ? pos : new Vector3Int(X, Y, Game.Current.ViewZ);


		#endregion




	}
}
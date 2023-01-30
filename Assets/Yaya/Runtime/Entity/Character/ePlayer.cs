using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System.Reflection;

namespace Yaya {
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.Bounds(-Const.CEL / 2, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.MapEditorGroup("Player")]
	public abstract class ePlayer : eCharacter, IGlobalPosition {




		#region --- VAR ---


		// Api
		public static ePlayer Selecting { get; private set; } = null;
		public override bool IsChargingAttack => MinimalChargeAttackDuration != int.MaxValue && !IsSafe && AttackCooldownReady(false) && FrameInput.GameKeyPress(GameKey.Action);
		protected abstract System.Type MascotType { get; }
		public eMascot Mascot => _Mascot ??= Game.Current.PeekOrGetEntity(MascotType.AngeHash()) as eMascot;
		public int AimViewX { get; private set; } = 0;
		public int AimViewY { get; private set; } = 0;

		// Data
		private static readonly PhysicsCell[] Collects = new PhysicsCell[8];
		private eMascot _Mascot = null;
		private int AttackRequiringFrame = int.MinValue;
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


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			PhysicsUpdate_Collect();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();

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
						FrameUpdate_JumpDashPound();
						FrameUpdate_Action_Attack();
						eControlHintUI.DrawHint(GameKey.Left, GameKey.Right, WORD.HINT_MOVE);
					} else {
						Stop();
					}
					break;
				case CharacterState.Sleep:
					if (!FrameTask.HasTask(YayaConst.TASK_ROUTE)) {
						FrameUpdate_Sleep();
					}
					break;
				case CharacterState.Passout:
					// Passout Hint
					if (Game.GlobalFrame >= PassoutFrame + YayaConst.PASSOUT_WAIT) {
						int x = X - Const.CEL / 2;
						int y = Y + Const.CEL * 3 / 2;
						eControlHintUI.DrawGlobalHint(x, y, GameKey.Action, WORD.UI_CONTINUE, true, true);
					}
					break;
			}

			// View
			FrameUpdate_View();

			// Mascot
			if (Mascot != null && !Mascot.Active && IsGrounded && Mascot.FollowOwner) {
				Mascot.Summon();
			}

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


		private void FrameUpdate_JumpDashPound () {

			if (LockingInput) return;

			eControlHintUI.DrawHint(GameKey.Jump, WORD.HINT_JUMP);

			HoldJump(FrameInput.GameKeyPress(GameKey.Jump));
			if (FrameInput.GameKeyDown(GameKey.Jump)) {
				// Movement Jump
				Jump();
				if (FrameInput.GameKeyPress(GameKey.Down)) {
					Dash();
				}
				AttackRequiringFrame = int.MinValue;
				if (CancelAttackOnJump) {
					CancelAttack();
				}
			}
			if (FrameInput.GameKeyDown(GameKey.Down)) {
				Pound();
			}

		}


		private void FrameUpdate_Action_Attack () {

			// Try Perform Action
			if (CurrentActionTarget != null) {
				eControlHintUI.DrawEntityHint(CurrentActionTarget as Entity, GameKey.Action, WORD.HINT_USE);
				if (FrameInput.AnyGameKeyDown()) {
					bool performed = InvokeAction();
					if (performed) return;
				}
			} else if (!IsSafe) {
				eControlHintUI.DrawHint(GameKey.Action, WORD.HINT_ATTACK);
			}

			// Try Cancel Action
			if (CurrentActionTarget != null && FrameInput.GameKeyDown(GameKey.Jump)) {
				CancelInvokeAction();
				return;
			}

			// Lock Input Check
			if (LockingInput) return;

			// Try Perform Attack
			if (CharacterState == CharacterState.GamePlay) {
				bool attDown = FrameInput.GameKeyDown(GameKey.Action);
				bool attHolding = FrameInput.GameKeyPress(GameKey.Action) && KeepAttackWhenHold;
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
				FrameInput.GameKeyDown(GameKey.Left) || FrameInput.GameKeyDown(GameKey.Right) ||
				FrameInput.GameKeyDown(GameKey.Down) || FrameInput.GameKeyDown(GameKey.Up)
			) {
				AttackRequiringFrame = int.MinValue;
			}

			// Perform Required Attack
			const int ATTACK_REQUIRE_GAP = 12;
			if (AttackCooldownReady(false) && Game.GlobalFrame < AttackRequiringFrame + ATTACK_REQUIRE_GAP) {
				AttackRequiringFrame = int.MinValue;
				Attack();
			}

		}


		private void FrameUpdate_Sleep () {
			if (SleepAmount >= 1000) Game.Current.ClearAntiSpawn(true);
			// Wake up on Press Action
			if (FrameInput.GameKeyDown(GameKey.Action) || FrameInput.GameKeyDown(GameKey.Jump)) {
				SetCharacterState(CharacterState.GamePlay);
				Y -= 4;
				MakeSafe(6);
			}
			// Ctrl Hint
			int x = X - Const.CEL / 2;
			int y = Y + Const.CEL * 3 / 2;
			eControlHintUI.DrawGlobalHint(x, y, GameKey.Action, WORD.HINT_WAKE, true, true);
		}


		private void FrameUpdate_View () {

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
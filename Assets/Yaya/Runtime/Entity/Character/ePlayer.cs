using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System.Reflection;

namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(1, 1)]
	[EntityAttribute.Bounds(-Const.CEL / 2, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.UpdateOutOfRange]
	public abstract class ePlayer : eCharacter {




		#region --- VAR ---


		// Api
		public static ePlayer Current { get; private set; } = null;
		public virtual eMascot Mascot => null;
		public override bool IsChargingAttack => MinimalChargeAttackDuration != int.MaxValue && !AntiAttack && AttackCooldownReady(false) && FrameInput.GameKeyPress(GameKey.Action);
		public int AimViewX { get; private set; } = 0;
		public int AimViewY { get; private set; } = 0;

		// Data
		private static readonly PhysicsCell[] Collects = new PhysicsCell[8];
		private int AttackRequiringFrame = int.MinValue;
		private int LastGroundedY = 0;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			Current ??= this;
		}


		public override void FillPhysics () {
			if (FrameTask.HasTask(Const.TASK_ROUTE)) return;
			base.FillPhysics();
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			PhysicsUpdate_Collect();
		}


		public override void FrameUpdate () {
			base.FrameUpdate();

			// Stop when Not Playing
			if (Current != this || Game.Current.State != GameState.Play) {
				Stop();
				return;
			}

			// Update Player
			switch (CharacterState) {
				case CharacterState.GamePlay:
					if (!FrameTask.HasTask(Const.TASK_ROUTE)) {
						Move(FrameInput.DirectionX, FrameInput.DirectionY);
						FrameUpdate_JumpDashPound();
						FrameUpdate_Action_Attack();
					} else {
						Stop();
					}
					break;
				case CharacterState.Sleep:
					if (!FrameTask.HasTask(Const.TASK_ROUTE)) {
						FrameUpdate_Sleep();
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
			if (Current != this) return;
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
			if (CurrentActionTarget != null && FrameInput.AnyGameKeyDown()) {
				bool performed = InvokeAction();
				if (performed) return;
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
				IgnoreAttack(6);
			}
		}


		private void FrameUpdate_View () {

			const int LINGER_RATE = 32;
			bool notInGameplay = FrameTask.IsTasking<OpeningTask>(Const.TASK_ROUTE) || CharacterState != CharacterState.GamePlay;
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
			AimViewY = Y <= LastGroundedY ?
				Y - game.ViewRect.height * 382 / 1000 : AimViewY;

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


		public static ePlayer TrySpawnPlayer (int x, int y) {
			if (Game.Current.TryGetEntity<ePlayer>(out var player)) {
				return player;
			} else {
				int firstPlayerID = 0;
				foreach (var type in typeof(ePlayer).AllChildClass()) {
					firstPlayerID = type.AngeHash();
					if (type.GetCustomAttribute<FirstSelectedPlayerAttribute>(true) != null) break;
				}
				return Game.Current.SpawnEntity(firstPlayerID, x, y) as ePlayer;
			}
		}


		public static ePlayer TrySpawnPlayerToBed (int x, int y) {
			var player = TrySpawnPlayer(x, y);
			if (player == null) return null;
			// Go to Bed
			if (Game.Current.TryGetEntityNearby<eBed>(new(x, y), out var bed)) {
				bed.Invoke(player);
			}
			player.SleepAmount = 1000;
			return player;
		}


		#endregion




	}
}
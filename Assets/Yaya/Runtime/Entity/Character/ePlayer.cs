using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using System.Reflection;

namespace Yaya {
	[EntityAttribute.ExcludeInMapEditor]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.Bounds(-Const.CEL / 2, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.ForceSpawn]
	[EntityAttribute.ForceUpdate]
	public abstract class ePlayer : eCharacter {




		#region --- VAR ---


		// Api
		public static ePlayer Current { get; private set; } = null;
		public virtual eMascot Mascot => null;
		public override bool IsChargingAttack => MinimalChargeAttackDuration != int.MaxValue && !AntiAttack && AttackCooldownReady(false) && FrameInput.GameKeyPress(GameKey.Action);

		// Data
		private static readonly PhysicsCell[] Collects = new PhysicsCell[8];
		private int LeftDownFrame = int.MinValue;
		private int RightDownFrame = int.MinValue;
		private int DownDownFrame = int.MinValue;
		private int UpDownFrame = int.MinValue;
		private int AttackRequiringFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			Current = this;
		}


		public override void FillPhysics () {
			if (FrameTask.HasTask(Const.TASK_ROUTE)) return;
			base.FillPhysics();
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (Current != this) return;
			// Collect
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


		public override void FrameUpdate () {
			base.FrameUpdate();

			// Stop when Not Playing
			if (Game.Current.State != GameState.Play) {
				Stop();
				return;
			}

			if (Current != this) return;

			// Update Player
			switch (CharacterState) {
				case CharacterState.GamePlay:
					if (!FrameTask.HasTask(Const.TASK_ROUTE)) {
						FrameUpdate_Move();
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

			// Mascot
			if (Mascot != null && !Mascot.Active && IsGrounded && Mascot.FollowOwner) {
				Mascot.Summon();
				var spawnRect = Game.Current.SpawnRect;
				Mascot.X = Mascot.X.Clamp(spawnRect.xMin + Const.CEL, spawnRect.xMax - Const.CEL);
				Mascot.Y = Mascot.Y.Clamp(spawnRect.yMin + Const.CEL, spawnRect.yMax - Const.CEL);
			}

		}


		private void FrameUpdate_Move () {

			int frame = Game.GlobalFrame;
			var x = Direction3.None;
			var y = Direction3.None;

			// Left
			if (FrameInput.GameKeyPress(GameKey.Left)) {
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
			if (FrameInput.GameKeyPress(GameKey.Right)) {
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

			if (!LockingInput) {
				// Down
				if (FrameInput.GameKeyPress(GameKey.Down)) {
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
				if (FrameInput.GameKeyPress(GameKey.Up)) {
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
			Move(x, y);
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

			// Perform Required Attack
			const int ATTACK_REQUIRE_GAP = 12;
			if (AttackCooldownReady(false) && Game.GlobalFrame < AttackRequiringFrame + ATTACK_REQUIRE_GAP) {
				AttackRequiringFrame = int.MinValue;
				Attack();
			}
		}


		private void FrameUpdate_Sleep () {
			if (FrameInput.GameKeyDown(GameKey.Action) || FrameInput.GameKeyDown(GameKey.Jump)) {
				// Wake up
				SetCharacterState(CharacterState.GamePlay);
				Y -= 4;
				IgnoreAttack(6);
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
				return Game.Current.AddEntity(firstPlayerID, x, y) as ePlayer;
			}
		}


		#endregion




	}
}
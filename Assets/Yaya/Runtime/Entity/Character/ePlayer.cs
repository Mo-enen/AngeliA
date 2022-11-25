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
		public int AimViewX { get; private set; } = 0;
		public int AimViewY { get; private set; } = 0;

		// Data
		private static readonly HitInfo[] Collects = new HitInfo[8];
		private int LeftDownFrame = int.MinValue;
		private int RightDownFrame = int.MinValue;
		private int DownDownFrame = int.MinValue;
		private int UpDownFrame = int.MinValue;
		private int AttackRequiringFrame = int.MinValue;
		private int PlayerLastGroundedY = 0;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			if (Current == null || !Current.Active) Current = this;
		}


		public override void FillPhysics () {
			if (FrameTask.HasTask(Const.TASK_ROUTE)) return;
			base.FillPhysics();
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
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
				if (Current != null) {
					Current.Movement.Stop();
				}
				return;
			}

			// Respawn
			FrameUpdate_Respawn();

			// Update Player
			if (Current == null) return;
			bool hasRoute = FrameTask.HasTask(Const.TASK_ROUTE);
			switch (Current.CharacterState) {
				case CharacterState.GamePlay:
					if (!hasRoute) {
						FrameUpdate_Move();
						FrameUpdate_JumpDashPound();
						FrameUpdate_Action_Attack();
					} else {
						Current.Movement.Stop();
					}
					break;
				case CharacterState.Sleep:
					if (!hasRoute) {
						FrameUpdate_Sleep();
					}
					break;
			}
			if (!hasRoute) {
				FrameUpdate_View();
			}
		}


		private void FrameUpdate_Respawn () {

			// Spawn Player when No Player Entity
			if (Current != null && !Current.Active) Current = null;
			if (Current == null && !FrameTask.HasTask(Const.TASK_ROUTE)) {
				var center = CellRenderer.CameraRect.CenterInt();
				Current = TrySpawnPlayer(center.x, center.y);
			}

			// Reload Game and Player After Passout
			if (
				Current != null && Current.Active &&
				Current.CharacterState == CharacterState.Passout &&
				Game.GlobalFrame > Current.PassoutFrame + YayaConst.PASSOUT_WAIT &&
				FrameInput.GetGameKeyDown(GameKey.Action) &&
				!FrameTask.HasTask(Const.TASK_ROUTE)
			) {
				FrameTask.AddToLast(tFadeOut.TYPE_ID, Const.TASK_ROUTE);
				if (FrameTask.TryAddToLast(tOpening.TYPE_ID, Const.TASK_ROUTE, out var task) && task is tOpening oTask) {
					oTask.ViewX = YayaConst.OPENING_X;
					oTask.ViewYStart = YayaConst.OPENING_Y;
					oTask.ViewYEnd = YayaConst.OPENING_END_Y;
					oTask.RemovePlayerAtStart = true;
					oTask.SpawnPlayerAtStart = true;
				}
			}
		}


		private void FrameUpdate_Move () {

			int frame = Game.GlobalFrame;
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

			if (!Current.Action.LockingInput) {
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
			Current.Movement.Move(x, y);
		}


		private void FrameUpdate_JumpDashPound () {

			if (Current.Action.LockingInput) return;

			var movement = Current.Movement;
			var attack = Current.Attackness;
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


		private void FrameUpdate_Action_Attack () {

			var action = Current.Action;
			var attack = Current.Attackness;

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
			if (Current.CharacterState == CharacterState.GamePlay) {
				bool attDown = FrameInput.GetGameKeyDown(GameKey.Action);
				bool attHolding = FrameInput.GetGameKey(GameKey.Action) && attack.KeepTriggerWhenHold;
				if (attDown || attHolding) {
					if (Current.IsAttackAllowedByMovement()) {
						if (attack.CheckReady(!attDown)) {
							attack.Attack();
						} else if (attDown) {
							AttackRequiringFrame = Game.GlobalFrame;
						}
					}
					return;
				}
			}

			// Perform Required Attack
			const int ATTACK_REQUIRE_GAP = 12;
			if (attack.CheckReady(false) && Game.GlobalFrame < AttackRequiringFrame + ATTACK_REQUIRE_GAP) {
				AttackRequiringFrame = int.MinValue;
				attack.Attack();
			}
		}


		private void FrameUpdate_Sleep () {
			if (FrameInput.GetGameKeyDown(GameKey.Action) || FrameInput.GetGameKeyDown(GameKey.Jump)) {
				Current.SetCharacterState(CharacterState.GamePlay);
				Current.Y -= 2;
			}
		}


		private void FrameUpdate_View () {

			const int LINGER_RATE = 32;
			var viewRect = Game.Current.ViewRect;
			bool flying = Current.Movement.IsFlying;
			int playerX = Current.X;
			int playerY = Current.Y;
			bool inAir = Current.InAir;

			if (!inAir || flying) PlayerLastGroundedY = playerY;
			int linger = viewRect.width * LINGER_RATE / 1000;
			int centerX = viewRect.x + viewRect.width / 2;
			if (playerX < centerX - linger) {
				AimViewX = playerX + linger - viewRect.width / 2;
			} else if (playerX > centerX + linger) {
				AimViewX = playerX - linger - viewRect.width / 2;
			}
			AimViewY = !inAir || flying || playerY < PlayerLastGroundedY ? GetCameraY(playerY, viewRect.height) : AimViewY;
			Game.Current.SetViewPositionDely(AimViewX, AimViewY, YayaConst.PLAYER_VIEW_LERP_RATE, YayaConst.VIEW_PRIORITY_PLAYER);

			// Clamp
			if (!viewRect.Contains(playerX, playerY)) {
				if (playerX >= viewRect.xMax) AimViewX = playerX - viewRect.width + 1;
				if (playerX <= viewRect.xMin) AimViewX = playerX - 1;
				if (playerY >= viewRect.yMax) AimViewY = playerY - viewRect.height + 1;
				if (playerY <= viewRect.yMin) AimViewY = playerY - 1;
				Game.Current.SetViewPositionDely(AimViewX, AimViewY, 1000, YayaConst.VIEW_PRIORITY_PLAYER + 1);
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




		#region --- LGC ---


		private int GetCameraY (int playerY, int viewHeight) => playerY - viewHeight * 382 / 1000;


		#endregion





	}
}
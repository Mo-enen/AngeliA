using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public abstract partial class Character {




		#region --- SUB ---


		protected enum CharacterNavigationState { Idle, Operation, Fly, }


		#endregion




		#region --- VAR ---


		// Api
		protected CharacterNavigationState NavigationState { get; set; } = CharacterNavigationState.Idle;
		protected Int2 NavigationAim { get; private set; } = default;
		protected bool NavigationAimGrounded { get; private set; } = default;
		protected bool HasPerformableOperation => CurrentNavOperationIndex < CurrentNavOperationCount || CurrentNavOperationCount == 0;

		// Override
		protected virtual bool NavigationEnable => false;
		protected virtual bool ClampInSpawnRect => false;
		protected virtual int NavigationStartMoveDistance => Const.CEL * 4;
		protected virtual int NavigationEndMoveDistance => Const.CEL * 1;
		protected virtual int NavigationStartFlyDistance => Const.CEL * 18;
		protected virtual int NavigationEndFlyDistance => Const.CEL * 3;
		protected virtual int NavigationMinimumFlyDuration => 60;

		// Data
		private readonly CellNavigation.Operation[] NavOperations = new CellNavigation.Operation[64];
		private int CurrentNavOperationIndex = 0;
		private int CurrentNavOperationCount = 0;
		private int NavFlyStartFrame = int.MinValue;
		private int NavJumpFrame = 0;
		private int NavJumpDuration = 0;
		private bool NavMoveDoneX = false;
		private bool NavMoveDoneY = false;
		private Int2 NavJumpFromPosition = default;


		#endregion




		#region --- MSG ---


		private void OnActivated_Navigation () => ResetNavigation();


		private void PhysicsUpdate_Navigation () {

			if (!NavigationEnable) return;

			// Clamp In Range
			if (ClampInSpawnRect) {
				var range = Stage.SpawnRect;
				if (!range.Overlaps(Rect)) {
					X = X.Clamp(range.xMin, range.xMax);
					Y = Y.Clamp(range.yMin, range.yMax);
					NavigationState = CharacterNavigationState.Fly;
					NavFlyStartFrame = Game.GlobalFrame;
				}
			}

			// Refresh
			NavUpdate_State();

			// Perform Navigate
			if (CurrentNavOperationIndex >= CurrentNavOperationCount) {
				CurrentNavOperationCount = 0;
				CurrentNavOperationIndex = 0;
				if (NavigationState == CharacterNavigationState.Operation) {
					CurrentNavOperationCount = CellNavigation.NavigateTo(
						NavOperations, Game.GlobalFrame, Stage.ViewRect, X, Y, NavigationAim.x, NavigationAim.y
					);
					if (CurrentNavOperationCount == 0) {
						NavigationState = CharacterNavigationState.Idle;
					}
					NavMoveDoneX = false;
					NavMoveDoneY = false;
				}
			}

			// Move to Target
			switch (NavigationState) {
				case CharacterNavigationState.Idle:
					NavUpdate_Movement_Idle();
					break;
				case CharacterNavigationState.Operation:
					NavUpdate_Movement_Operating();
					break;
				case CharacterNavigationState.Fly:
					NavUpdate_Movement_Flying();
					break;
			}

		}


		private void NavUpdate_State () {

			// Don't Refresh State when Jumping
			if (
				NavigationState == CharacterNavigationState.Operation &&
				CurrentNavOperationIndex >= 0 &&
				CurrentNavOperationIndex < CurrentNavOperationCount &&
				NavOperations[CurrentNavOperationIndex].Motion == NavigationOperateMotion.Jump
			) return;

			int START_MOVE_DISTANCE_SQ = NavigationStartMoveDistance * NavigationStartMoveDistance;
			int END_MOVE_DISTANCE_SQ = NavigationEndMoveDistance * NavigationEndMoveDistance;
			int START_FLY_DISTANCE_SQ = NavigationStartFlyDistance * NavigationStartFlyDistance;
			int END_FLY_DISTANCE_SQ = NavigationEndFlyDistance * NavigationEndFlyDistance;

			// Nav Logic Start
			var navAim = GetNavigationAim(out bool _navAimGrounded);
			if (navAim.HasValue) {
				NavigationAim = navAim.Value;
				NavigationAimGrounded = _navAimGrounded;
				CurrentNavOperationIndex = 0;
				CurrentNavOperationCount = 0;
				NavJumpDuration = 0;
			}

			// Fly When No Grounded Aim Position
			if (!NavigationAimGrounded) {
				NavigationState = CharacterNavigationState.Fly;
				NavFlyStartFrame = Game.GlobalFrame;
				return;
			}

			int aimSqrtDis = Util.SquareDistance(NavigationAim.x, NavigationAim.y, X, Y);
			switch (NavigationState) {

				case CharacterNavigationState.Idle:
					if (aimSqrtDis > START_FLY_DISTANCE_SQ) {
						// Idle >> Fly
						NavigationState = CharacterNavigationState.Fly;
						NavFlyStartFrame = Game.GlobalFrame;
					} else if (aimSqrtDis > START_MOVE_DISTANCE_SQ) {
						// Idle >> Nav
						NavigationState = CharacterNavigationState.Operation;
					}
					break;

				case CharacterNavigationState.Operation:
					if (aimSqrtDis > START_FLY_DISTANCE_SQ) {
						// Nav >> Fly
						NavigationState = CharacterNavigationState.Fly;
						NavFlyStartFrame = Game.GlobalFrame;
					} else if (CurrentNavOperationIndex >= CurrentNavOperationCount && aimSqrtDis < END_MOVE_DISTANCE_SQ) {
						// Nav >> Idle
						NavigationState = CharacterNavigationState.Idle;
					}
					break;

				case CharacterNavigationState.Fly:
					if (
						Game.GlobalFrame > NavFlyStartFrame + NavigationMinimumFlyDuration &&
						aimSqrtDis < END_FLY_DISTANCE_SQ &&
						Y > NavigationAim.y - Const.HALF
					) {
						// Fly >> ??
						NavigationState = aimSqrtDis > START_MOVE_DISTANCE_SQ ?
							CharacterNavigationState.Operation :
							CharacterNavigationState.Idle;
					}
					break;

			}

		}


		private void NavUpdate_Movement_Idle () {
			VelocityX = 0;
			if (!InWater && !InSand && !IsInsideGround) {
				if (CellNavigation.IsGround(Game.GlobalFrame, Stage.ViewRect, X, Y + Const.HALF / 2, out int groundY)) {
					// Move to Ground
					VelocityY = groundY - Y;
					MakeGrounded(1);
				} else {
					// Fall Down
					VelocityY = (VelocityY - Gravity).Clamp(-MaxGravitySpeed, int.MaxValue);
				}
			} else {
				VelocityY = 0;
			}
			Y += VelocityY;
			MovementState = InWater ? CharacterMovementState.SwimIdle : CharacterMovementState.Idle;
		}


		private void NavUpdate_Movement_Operating () {

			if (CurrentNavOperationIndex >= CurrentNavOperationCount) return;

			var operation = NavOperations[CurrentNavOperationIndex];
			int targetX = operation.TargetGlobalX;
			int targetY = operation.TargetGlobalY;
			var motion = operation.Motion;

			switch (motion) {

				// Move
				case NavigationOperateMotion.Move:

					int speed = InWater ? SwimSpeed * InWaterSpeedLoseRate / 1000 : RunSpeed;

					if (targetX == X) NavMoveDoneX = true;
					if (targetY == Y) NavMoveDoneY = true;

					bool moveRight = targetX - X > 0;
					bool moveUp = targetY - Y > 0;

					VelocityX = NavMoveDoneX ? 0 : moveRight ? speed : -speed;
					VelocityY = NavMoveDoneY ? 0 : moveUp ? speed : -speed;

					X += VelocityX;
					Y += VelocityY;

					// Done Check
					NavMoveDoneX = NavMoveDoneX || (moveRight != targetX - X > 0);
					NavMoveDoneY = NavMoveDoneY || (moveUp != targetY - Y > 0);

					// Goto Next Operation
					if (NavMoveDoneX && NavMoveDoneY) {
						if (!CellNavigation.IsGround(Game.GlobalFrame, Stage.ViewRect, targetX, targetY, out _)) {
							NavigationState = CharacterNavigationState.Fly;
							NavFlyStartFrame = Game.GlobalFrame;
							CurrentNavOperationIndex = 0;
							CurrentNavOperationCount = 0;
						} else {
							GotoNextOperation();
						}
					}

					break;


				// Jump
				case NavigationOperateMotion.Jump:

					const int JUMP_SPEED = 52;

					if (NavJumpDuration == 0) {
						// Jump Start
						int dis = Util.DistanceInt(X, Y, targetX, targetY);
						NavJumpFrame = 0;
						NavJumpDuration = (dis / JUMP_SPEED).Clamp(dis < Const.HALF ? 3 : 24, 120);
						NavJumpFromPosition.x = X;
						NavJumpFromPosition.y = Y;
					}

					if (NavJumpDuration > 0 && NavJumpFrame <= NavJumpDuration) {
						// Jumping
						int newX = Util.Remap(
							0, NavJumpDuration,
							NavJumpFromPosition.x, targetX,
							NavJumpFrame
						);
						int newY = Util.Remap(
							0, NavJumpDuration,
							NavJumpFromPosition.y, targetY,
							NavJumpFrame
						);
						if (NavJumpDuration > 3) {
							int deltaY = Mathf.Abs(NavJumpFrame - NavJumpDuration / 2);
							int arc = (Util.DistanceInt(
								NavJumpFromPosition.x,
								NavJumpFromPosition.y,
								targetX,
								targetY
							) / 2).Clamp(Const.HALF, Const.CEL * 3);
							newY += Util.Remap(
								NavJumpDuration * NavJumpDuration / 4, 0, 0, arc,
								deltaY * deltaY
							);
						}
						VelocityX = newX - X;
						VelocityY = newY - Y;
						X = newX;
						Y = newY;
						NavJumpFrame++;
					} else {
						// Jump End
						VelocityX = targetX - X;
						VelocityY = targetY - Y;
						X = targetX;
						Y = targetY;
						GotoNextOperation();
					}
					break;

				// None
				case NavigationOperateMotion.None:
					GotoNextOperation();
					break;
			}

			// Move State
			if (VelocityX != 0) {
				Move(VelocityX > 0 ? Direction3.Right : Direction3.Left, 0);
			}
			if (!InWater) {
				// Move
				MovementState =
					motion == NavigationOperateMotion.Move ? CharacterMovementState.Run :
					VelocityY > 0 ? CharacterMovementState.JumpUp : CharacterMovementState.JumpDown;
			} else {
				// Swim
				MovementState = VelocityX == 0 ? CharacterMovementState.SwimIdle : CharacterMovementState.SwimMove;
			}

			// Func
			void GotoNextOperation () {
				CurrentNavOperationIndex++;
				NavJumpFrame = 0;
				NavJumpDuration = 0;
				NavMoveDoneX = false;
				NavMoveDoneY = false;
				// Skip Too Tiny Jump
				while (
					CurrentNavOperationIndex < CurrentNavOperationCount
				) {
					var motion = NavOperations[CurrentNavOperationIndex];
					if (
						motion.Motion == NavigationOperateMotion.Jump &&
						Util.SquareDistance(X, Y, motion.TargetGlobalX, motion.TargetGlobalY) <= Const.HALF * Const.HALF
					) {
						CurrentNavOperationIndex++;
					} else break;
				}
			}
		}


		private void NavUpdate_Movement_Flying () {
			int speed = FlyMoveSpeed;
			if (InWater) speed = speed * InWaterSpeedLoseRate / 1000;
			var flyAim = NavigationAim;
			if (FlyAvailable) {
				// Can Fly
				flyAim.x = X.LerpTo(flyAim.x, 100);
				flyAim.y = Y.LerpTo(flyAim.y, 100);
				VelocityX = (flyAim.x - X).Clamp(-speed, speed);
				VelocityY = (flyAim.y - Y).Clamp(-speed, speed);
				X += VelocityX;
				Y += VelocityY;
				// Move State
				MovementState = CharacterMovementState.Fly;
				if ((X - NavigationAim.x).Abs() > 96) {
					Move(X < NavigationAim.x ? Direction3.Right : Direction3.Left, 0);
				}
			} else {
				// Can't Fly
				X = flyAim.x;
				Y = flyAim.y;
				OnTeleport?.Invoke(this);
				NavigationState = CharacterNavigationState.Idle;
				MovementState = CharacterMovementState.Idle;
			}
		}


		#endregion




		#region --- API ---


		public void ResetNavigation () {
			NavigationState = CharacterNavigationState.Idle;
			NavFlyStartFrame = int.MinValue;
			CurrentNavOperationIndex = 0;
			CurrentNavOperationCount = 0;
			NavigationAim = new Int2(X, Y);
			NavigationAimGrounded = IsGrounded;
			NavJumpFrame = 0;
			NavJumpDuration = 0;
			NavMoveDoneX = false;
			NavMoveDoneY = false;
		}


		protected virtual Int2? GetNavigationAim (out bool grounded) {
			grounded = true;
			return null;
		}


		#endregion




	}
}
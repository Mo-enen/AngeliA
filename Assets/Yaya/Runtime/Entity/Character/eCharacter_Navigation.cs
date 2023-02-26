using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract partial class eCharacter {




		#region --- SUB ---


		protected enum CharacterNavigationState { Idle, Navigate, Fly, }


		#endregion




		#region --- VAR ---


		// Api
		protected CharacterNavigationState NavigationState { get; set; } = CharacterNavigationState.Idle;
		protected Vector2Int NavigationAim { get; private set; } = default;
		protected bool NavigationAimGrounded { get; private set; } = default;
		protected bool HasPerformableOperation => CurrentNavOperationIndex < CurrentNavOperationCount || CurrentNavOperationCount == 0;

		// Override
		public virtual bool NavigationEnable => false;
		protected virtual int NavigationStartMoveDistance => Const.CEL * 4;
		protected virtual int NavigationEndMoveDistance => Const.CEL * 1;
		protected virtual int NavigationStartFlyDistance => Const.CEL * 18;
		protected virtual int NavigationEndFlyDistance => Const.CEL * 3;
		protected virtual int NavigationMinimumFlyDuration => 120;

		// Data
		private readonly CellNavigation.Operation[] NavOperations = new CellNavigation.Operation[8];
		private int CurrentNavOperationIndex = 0;
		private int CurrentNavOperationCount = 0;
		private int NavFlyStartFrame = int.MinValue;
		private int NavJumpFrame = 0;
		private int NavJumpDuration = 0;
		private bool NavUseJumpArc = true;
		private bool NavMoveDoneX = false;
		private bool NavMoveDoneY = false;
		private Vector2Int NavJumpFromPosition = default;


		#endregion




		#region --- MSG ---


		private void OnActived_Navigation () => ResetNavigation();


		private void PhysicsUpdate_Navigation () {

			if (!NavigationEnable) return;

			// Clamp In Range
			var range = Game.Current.SpawnRect;
			X = X.Clamp(range.xMin, range.xMax);
			Y = Y.Clamp(range.yMin, range.yMax);

			// Refresh
			NavUpdate_State();

			// Perform Navigate
			if (CurrentNavOperationIndex >= CurrentNavOperationCount) {
				const int JUMP_DISTANCE_X = Const.CEL * 6;
				const int JUMP_DISTANCE_Y = Const.CEL * 6;
				CurrentNavOperationCount = 0;
				CurrentNavOperationIndex = 0;
				if (NavigationState == CharacterNavigationState.Navigate) {
					CurrentNavOperationCount = CellNavigation.NavigateTo(
						NavOperations, X, Y, NavigationAim.x, NavigationAim.y,
						JUMP_DISTANCE_X, JUMP_DISTANCE_Y
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
				case CharacterNavigationState.Navigate:
					NavUpdate_Movement_Navigating();
					break;
				case CharacterNavigationState.Fly:
					NavUpdate_Movement_Flying();
					break;
			}

		}


		private void NavUpdate_State () {

			// Don't Refresh State when Jumping
			if (
				NavigationState == CharacterNavigationState.Navigate &&
				CurrentNavOperationIndex >= 0 &&
				CurrentNavOperationIndex < CurrentNavOperationCount &&
				NavOperations[CurrentNavOperationIndex].Motion == NavigationMotion.Jump
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
			int aimSqrtDis = Util.SquareDistance(NavigationAim.x, NavigationAim.y, X, Y);

			if (!NavigationAimGrounded) {
				// Fly When No Grounded Aim Position
				NavigationState = CharacterNavigationState.Fly;
				NavFlyStartFrame = Game.GlobalFrame;
				return;
			}

			switch (NavigationState) {

				case CharacterNavigationState.Idle:
					if (aimSqrtDis > START_FLY_DISTANCE_SQ) {
						// Idle >> Fly
						NavigationState = CharacterNavigationState.Fly;
						NavFlyStartFrame = Game.GlobalFrame;
					} else if (aimSqrtDis > START_MOVE_DISTANCE_SQ) {
						// Idle >> Nav
						NavigationState = CharacterNavigationState.Navigate;
					}
					break;

				case CharacterNavigationState.Navigate:
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
						NavigationState == CharacterNavigationState.Fly &&
						Game.GlobalFrame > NavFlyStartFrame + NavigationMinimumFlyDuration &&
						aimSqrtDis < END_FLY_DISTANCE_SQ
					) {
						// Fly >> ??
						NavigationState = aimSqrtDis > START_MOVE_DISTANCE_SQ ?
							CharacterNavigationState.Navigate :
							CharacterNavigationState.Idle;
					}
					break;

				default: throw new System.NotImplementedException();
			}

		}


		private void NavUpdate_Movement_Idle () {
			VelocityX = 0;
			if (!InWater && !InSand) {
				if (CellNavigation.IsGround(X, Y + Const.HALF, out int groundY)) {
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
		}


		private void NavUpdate_Movement_Navigating () {

			if (CurrentNavOperationIndex >= CurrentNavOperationCount) return;

			var operation = NavOperations[CurrentNavOperationIndex];
			int targetX = operation.TargetGlobalX;
			int targetY = operation.TargetGlobalY;

			switch (operation.Motion) {

				// Move
				case NavigationMotion.Move:

					int speed = RunSpeed.Value;

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
						GotoNextOperation();
					}

					break;


				// Jump
				case NavigationMotion.Jump:

					const int JUMP_SPEED = 52;
					const int JUMP_ARCH = Const.CEL * 3 / 2;

					if (NavJumpDuration == 0) {
						// Jump Start
						int dis = Util.DistanceInt(X, Y, targetX, targetY);
						NavJumpFrame = 0;
						NavJumpDuration = (dis / JUMP_SPEED).Clamp(3, 120);
						NavJumpFromPosition.x = X;
						NavJumpFromPosition.y = Y;
						NavUseJumpArc = IsGrounded || targetY > Y;
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
						if (NavUseJumpArc) {
							int deltaY = Mathf.Abs(NavJumpFrame - NavJumpDuration / 2);
							newY += Util.Remap(
								NavJumpDuration * NavJumpDuration / 4, 0, 0, JUMP_ARCH,
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


				// Fly
				case NavigationMotion.Fly:
					NavigationState = CharacterNavigationState.Fly;
					NavFlyStartFrame = Game.GlobalFrame;
					CurrentNavOperationIndex = 0;
					CurrentNavOperationCount = 0;
					break;


				// None
				case NavigationMotion.None:
					GotoNextOperation();
					break;
			}

			// Func
			void GotoNextOperation () {
				CurrentNavOperationIndex++;
				NavJumpFrame = 0;
				NavJumpDuration = 0;
				NavMoveDoneX = false;
				NavMoveDoneY = false;
			}
		}


		private void NavUpdate_Movement_Flying () {
			var flyAim = NavigationAim;
			flyAim.x = X.LerpTo(flyAim.x, 100);
			flyAim.y = Y.LerpTo(flyAim.y, 100);
			VelocityX = (flyAim.x - X).Clamp(-FlyMoveSpeed, FlyMoveSpeed);
			VelocityY = (flyAim.y - Y).Clamp(-FlyMoveSpeed, FlyMoveSpeed);
			X += VelocityX;
			Y += VelocityY;
		}


		#endregion




		#region --- API ---


		public void ResetNavigation () {
			NavigationState = CharacterNavigationState.Idle;
			NavFlyStartFrame = int.MinValue;
			CurrentNavOperationIndex = 0;
			CurrentNavOperationCount = 0;
			NavigationAim = new Vector2Int(X, Y);
			NavigationAimGrounded = IsGrounded;
			NavJumpFrame = 0;
			NavJumpDuration = 0;
			NavMoveDoneX = false;
			NavMoveDoneY = false;
		}


		protected virtual Vector2Int? GetNavigationAim (out bool grounded) {
			grounded = true;
			return null;
		}


		#endregion




		#region --- LGC ---






		#endregion




	}
}
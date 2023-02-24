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
		public virtual bool NavigationEnable => false;
		protected bool NavOperationDone => CurrentNavOperationIndex >= CurrentNavOperationCount;
		protected bool HasNavOperation => CurrentNavOperationCount > 0;
		protected CharacterNavigationState NavigationState { get; set; } = CharacterNavigationState.Idle;
		protected Vector2Int NavigationAim { get; private set; } = default;
		protected bool NavigationAimGrounded { get; private set; } = default;

		// Data
		private readonly CellNavigation.Operation[] NavOperation = new CellNavigation.Operation[8];
		private int CurrentNavOperationIndex = 0;
		private int CurrentNavOperationCount = 0;
		private int NavigationFlyStartFrame = int.MinValue;
		private int NavJumpFrame = 0;
		private int NavJumpDuration = 0;
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
			if (NavOperationDone) {
				const int JUMP_DISTANCE_X = Const.CEL * 6;
				const int JUMP_DISTANCE_Y = Const.CEL * 6;
				CurrentNavOperationCount = 0;
				CurrentNavOperationIndex = 0;
				if (NavigationState == CharacterNavigationState.Navigate) {
					CurrentNavOperationCount = CellNavigation.NavigateTo(
						NavOperation, X, Y, NavigationAim.x, NavigationAim.y,
						JUMP_DISTANCE_X, JUMP_DISTANCE_Y, 32
					);
					if (CurrentNavOperationCount == 0) {
						NavigationState = CharacterNavigationState.Idle;
					}
					NavJumpDuration = 0;
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

			const int START_MOVE_DISTANCE = Const.CEL * 4;
			const int END_MOVE_DISTANCE = Const.CEL * 1;
			const int START_FLY_DISTANCE = Const.CEL * 18;
			const int END_FLY_DISTANCE = Const.CEL * 3;
			const int MINIMUM_FLY_DURATION = 120;

			// Nav Logic Start
			const int START_MOVE_DISTANCE_SQ = START_MOVE_DISTANCE * START_MOVE_DISTANCE;
			const int END_MOVE_DISTANCE_SQ = END_MOVE_DISTANCE * END_MOVE_DISTANCE;
			const int START_FLY_DISTANCE_SQ = START_FLY_DISTANCE * START_FLY_DISTANCE;
			const int END_FLY_DISTANCE_SQ = END_FLY_DISTANCE * END_FLY_DISTANCE;
			var navAim = GetNavigationAim(out bool _navAimGrounded);
			if (navAim.HasValue) {
				NavigationAim = navAim.Value;
				NavigationAimGrounded = _navAimGrounded;
			}
			int aimSqrtDis = Util.SquareDistance(NavigationAim.x, NavigationAim.y, X, Y);
			bool operationDone = NavOperationDone;

			if (!NavigationAimGrounded) {
				// Fly When No Grounded Aim Position
				NavigationState = CharacterNavigationState.Fly;
				NavigationFlyStartFrame = Game.GlobalFrame;
				return;
			}

			switch (NavigationState) {

				case CharacterNavigationState.Idle:
					if (aimSqrtDis > START_FLY_DISTANCE_SQ) {
						// Idle >> Fly
						NavigationState = CharacterNavigationState.Fly;
						NavigationFlyStartFrame = Game.GlobalFrame;
					} else if (aimSqrtDis > START_MOVE_DISTANCE_SQ) {
						// Idle >> Nav
						NavigationState = CharacterNavigationState.Navigate;
					}
					break;

				case CharacterNavigationState.Navigate:
					if (aimSqrtDis > START_FLY_DISTANCE_SQ) {
						// Nav >> Fly
						NavigationState = CharacterNavigationState.Fly;
						NavigationFlyStartFrame = Game.GlobalFrame;
					} else if (operationDone && aimSqrtDis < END_MOVE_DISTANCE_SQ) {
						// Nav >> Idle
						NavigationState = CharacterNavigationState.Idle;
					}
					break;

				case CharacterNavigationState.Fly:
					if (
						NavigationState == CharacterNavigationState.Fly &&
						Game.GlobalFrame > NavigationFlyStartFrame + MINIMUM_FLY_DURATION &&
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

			var operation = NavOperation[CurrentNavOperationIndex];
			if (operation.Motion != NavigationMotion.Jump) {
				NavJumpDuration = 0;
			}

			int targetX = operation.TargetGlobalX;
			int targetY = operation.TargetGlobalY;

			switch (operation.Motion) {

				// Move
				case NavigationMotion.Move:

					bool rightSide = X > targetX;
					int moveSpeed = RunSpeed.Value;

					VelocityX = (targetX - X).Clamp(-moveSpeed, moveSpeed);
					VelocityY = (targetY - Y).Clamp(-moveSpeed, moveSpeed);

					X += VelocityX;
					Y += VelocityY;

					if (X > targetX != rightSide) {
						CurrentNavOperationIndex++;
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
						NavJumpDuration = (dis / JUMP_SPEED).Clamp(24, 120);
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
						int deltaY = Mathf.Abs(NavJumpFrame - NavJumpDuration / 2);
						int newY = Util.Remap(
							0, NavJumpDuration,
							NavJumpFromPosition.y, targetY,
							NavJumpFrame
						) + Util.Remap(
							NavJumpDuration * NavJumpDuration / 4, 0, 0, JUMP_ARCH,
							deltaY * deltaY
						);
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
						NavJumpFrame = 0;
						NavJumpDuration = 0;
						CurrentNavOperationIndex++;
					}
					break;


				// Fly
				case NavigationMotion.Fly:
					NavigationState = CharacterNavigationState.Fly;
					NavigationFlyStartFrame = Game.GlobalFrame;
					CurrentNavOperationIndex = 0;
					CurrentNavOperationCount = 0;
					break;


				// None
				case NavigationMotion.None:
					CurrentNavOperationIndex++;
					break;
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
			NavigationFlyStartFrame = int.MinValue;
			CurrentNavOperationIndex = 0;
			CurrentNavOperationCount = 0;
			NavigationAim = new Vector2Int(X, Y);
			NavigationAimGrounded = IsGrounded;
			NavJumpFrame = 0;
			NavJumpDuration = 0;
		}


		public void ResetNavigationOperation () {
			CurrentNavOperationIndex = 0;
			CurrentNavOperationCount = 0;
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
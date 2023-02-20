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
		protected virtual bool NavigationEnable => false;
		protected bool NavOperationDone => CurrentNavOperationIndex >= CurrentNavOperationCount;
		protected bool HasNavOperation => CurrentNavOperationCount > 0;
		protected CharacterNavigationState NavigationState { get; private set; } = CharacterNavigationState.Idle;
		protected Vector2Int NavigationAim { get; private set; } = default;

		// Data
		private readonly CellNavigation.Operation[] NavOperation = new CellNavigation.Operation[8];
		private int CurrentNavOperationIndex = 0;
		private int CurrentNavOperationCount = 0;
		private int NavigationFlyStartFrame = int.MinValue;


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
			NavUpdate_NavigationRefresh();





			// =============== Test =====================
			NavigationState = CharacterNavigationState.Fly;
			// =============== Test =====================



			// Move to Target
			switch (NavigationState) {
				case CharacterNavigationState.Idle:
					NavUpdate_MovementIdle();
					break;
				case CharacterNavigationState.Navigate:
					NavUpdate_MovementNavigate();
					break;
				case CharacterNavigationState.Fly:
					NavUpdate_MovementFly();
					break;
			}

		}


		// Navigation
		private void NavUpdate_NavigationRefresh () {

			const int START_MOVE_DISTANCE = Const.CEL * 4;
			const int END_MOVE_DISTANCE = Const.CEL * 1;
			const int START_FLY_DISTANCE = Const.CEL * 18;
			const int END_FLY_DISTANCE = Const.CEL * 3;
			const int JUMP_DISTANCE_X = Const.CEL * 6;
			const int JUMP_DISTANCE_Y = Const.CEL * 6;
			const int MINIMUM_FLY_DURATION = 120;

			// Nav Logic Start
			int startMoveSqrtDistance = START_MOVE_DISTANCE * START_MOVE_DISTANCE;
			int endMoveSqrtDistance = END_MOVE_DISTANCE * END_MOVE_DISTANCE;
			int startFlySqrtDistance = START_FLY_DISTANCE * START_FLY_DISTANCE;
			int endFlySqrtDistance = END_FLY_DISTANCE * END_FLY_DISTANCE;
			int minimumFlyDuration = MINIMUM_FLY_DURATION;
			NavigationAim = GetNavigationAim();
			int aimSqrtDis = Util.SquareDistance(NavigationAim.x, NavigationAim.y, X, Y);
			bool operationDone = NavOperationDone;

			switch (NavigationState) {

				case CharacterNavigationState.Idle:
					if (aimSqrtDis > startFlySqrtDistance) {
						// Idle >> Fly
						StartFly();
					} else if (aimSqrtDis > startMoveSqrtDistance) {
						// Idle >> Nav
						NavigationState = CharacterNavigationState.Navigate;
					}
					break;

				case CharacterNavigationState.Navigate:
					if (aimSqrtDis > startFlySqrtDistance) {
						// Nav >> Fly
						StartFly();
					} else if (operationDone && aimSqrtDis < endMoveSqrtDistance) {
						// Nav >> Idle
						NavigationState = CharacterNavigationState.Idle;
					}
					break;

				case CharacterNavigationState.Fly:
					// Fly >> ??
					if (
						NavigationState == CharacterNavigationState.Fly &&
						Game.GlobalFrame > NavigationFlyStartFrame + minimumFlyDuration &&
						aimSqrtDis < endFlySqrtDistance
					) {
						NavigationState = aimSqrtDis > startMoveSqrtDistance ?
							CharacterNavigationState.Navigate :
							CharacterNavigationState.Idle;
					}
					break;

				default: throw new System.NotImplementedException();
			}

			// Perform Navigate
			if (operationDone) {
				CurrentNavOperationCount = 0;
				CurrentNavOperationIndex = 0;
				if (NavigationState == CharacterNavigationState.Navigate) {
					CurrentNavOperationCount = CellNavigation.NavigateTo(
						NavOperation, X, Y, NavigationAim.x, NavigationAim.y,
						JUMP_DISTANCE_X, JUMP_DISTANCE_Y
					);
					if (CurrentNavOperationCount == 0) {
						NavigationState = CharacterNavigationState.Idle;
					}
				}
			}

		}


		private void NavUpdate_MovementIdle () {
			VelocityX = 0;
			VelocityY = (
				InWater || InSand || IsInsideGround ? 0 :
				CellNavigation.IsGround(X, Y, out int groundY) ? groundY - Y :
				VelocityY - Gravity
			).Clamp(-MaxGravitySpeed, int.MaxValue);
			X += VelocityX;
			Y += VelocityY;
		}


		private void NavUpdate_MovementNavigate () {





		}


		private void NavUpdate_MovementFly () {
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
		}


		public void ClearNavigation () {
			CurrentNavOperationIndex = 0;
			CurrentNavOperationCount = 0;
		}


		protected virtual Vector2Int GetNavigationAim () => NavigationAim;


		#endregion




		#region --- LGC ---


		private void StartFly () {
			NavigationState = CharacterNavigationState.Fly;
			NavigationFlyStartFrame = Game.GlobalFrame;
			VelocityX = 0;
			VelocityY = 0;
		}


		#endregion




	}
}
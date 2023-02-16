using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract partial class eCharacter {




		#region --- SUB ---


		private enum CharacterNavigationState { Idle, Navigate, Fly, }


		#endregion




		#region --- VAR ---


		// Api
		protected virtual int NavigationGroundNearbyIteration => 6;
		protected virtual int NavigationStartMoveDistance => Const.CEL * 4;
		protected virtual int NavigationEndMoveDistance => Const.CEL * 1;
		protected virtual int NavigationStartFlyDistance => Const.CEL * 18;
		protected virtual int NavigationEndFlyDistance => Const.CEL * 3;
		protected virtual int NavigationJumpDistanceX => Const.CEL * 6;
		protected virtual int NavigationJumpDistanceY => Const.CEL * 6;
		protected virtual int NavigationMinimumFlyDuration => 120;
		protected virtual int NavigationTargetRefreshFrequency => 30;
		protected virtual bool NavigationEnable => false;

		// Data
		private CharacterNavigationState NavigationState = CharacterNavigationState.Idle;
		private readonly CellNavigation.Operation[] NavOperation = new CellNavigation.Operation[8];
		private int CurrentNavOperationIndex = 0;
		private int CurrentNavOperationCount = 0;
		private int NavigationFlyStartFrame = int.MinValue;
		private int LastNavStateRefreshFrame = int.MaxValue;


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
					MoveState = MovementState.Fly;
					break;
			}
			X += VelocityX;
			Y += VelocityY;

		}


		// Navigation
		private void NavUpdate_NavigationRefresh () {

			bool operationDone =
				NavigationState == CharacterNavigationState.Navigate &&
				CurrentNavOperationCount > 0 &&
				CurrentNavOperationIndex >= CurrentNavOperationCount;

			// Scan Frequency Gate
			if (
				!operationDone &&
				Game.GlobalFrame < LastNavStateRefreshFrame + NavigationTargetRefreshFrequency
			) return;
			LastNavStateRefreshFrame = Game.GlobalFrame;

			// Nav Logic Start
			int startMoveSqrtDistance = NavigationStartMoveDistance * NavigationStartMoveDistance;
			int endMoveSqrtDistance = NavigationEndMoveDistance * NavigationEndMoveDistance;
			int startFlySqrtDistance = NavigationStartFlyDistance * NavigationStartFlyDistance;
			int endFlySqrtDistance = NavigationEndFlyDistance * NavigationEndFlyDistance;
			int minimumFlyDuration = NavigationMinimumFlyDuration;
			var aim = GetNavigationAim();
			int aimSqrtDis = Util.SqrtDistance(aim.x, aim.y, X, Y);

			switch (NavigationState) {

				case CharacterNavigationState.Idle:
					if (aimSqrtDis > startFlySqrtDistance) {
						// Idle >> Fly
						NavigationState = CharacterNavigationState.Fly;
						NavigationFlyStartFrame = Game.GlobalFrame;
					} else if (aimSqrtDis > startMoveSqrtDistance) {
						// Idle >> Nav
						NavigationState = CharacterNavigationState.Navigate;
					}
					break;

				case CharacterNavigationState.Navigate:
					if (aimSqrtDis > startFlySqrtDistance) {
						// Nav >> Fly
						NavigationState = CharacterNavigationState.Fly;
						NavigationFlyStartFrame = Game.GlobalFrame;
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
			CurrentNavOperationCount = 0;
			CurrentNavOperationIndex = 0;
			if (NavigationState == CharacterNavigationState.Navigate) {
				CurrentNavOperationCount = CellNavigation.Navigate(
					NavOperation, X, Y, aim.x, aim.y,
					NavigationJumpDistanceX, NavigationJumpDistanceY
				);
				if (CurrentNavOperationCount == 0) {
					NavigationState = CharacterNavigationState.Idle;
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
		}


		private void NavUpdate_MovementNavigate () {





		}


		private void NavUpdate_MovementFly () {




		}


		#endregion




		#region --- API ---


		public void ResetNavigation () {
			NavigationState = CharacterNavigationState.Idle;
			NavigationFlyStartFrame = int.MinValue;
			LastNavStateRefreshFrame = int.MaxValue;
			CurrentNavOperationIndex = 0;
			CurrentNavOperationCount = 0;
		}


		protected virtual Vector2Int GetNavigationAim () => new(X, Y);


		#endregion




		#region --- LGC ---





		#endregion




	}
}
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
		protected virtual int NavigationGroundSnapDistance => Const.CEL * 6;
		protected virtual int NavigationStartMoveRange => Const.CEL * 6;
		protected virtual int NavigationMinimumFlyDuration => 120;
		protected virtual int NavigationTargetScanFrequency => 30;

		// Data
		private CharacterNavigationState NavigationState = CharacterNavigationState.Idle;
		private readonly CellNavigation.Operation[] NavOperation = new CellNavigation.Operation[8];
		private int CurrentNavOperationIndex = 0;
		private int CurrentNavOperationCount = 0;
		private int NavigationAimX = 0;
		private int NavigationAimY = 0;
		private int NavigationTargetX = 0;
		private int NavigationTargetY = 0;
		private int NavigationFlyStartFrame = int.MinValue;
		private int LastNavStateRefreshFrame = int.MaxValue;
		private int LastNavigateFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		private void OnActived_Navigation () => ResetNavigation();


		private void PhysicsUpdate_Navigation () {

			if (Game.GlobalFrame > LastNavigateFrame || CharacterState != CharacterState.GamePlay) return;

			// Find Target
			NavUpdate_NavigationScan();





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
		private void NavUpdate_NavigationScan () {

			if (Game.GlobalFrame < LastNavStateRefreshFrame + NavigationTargetScanFrequency) return;
			LastNavStateRefreshFrame = Game.GlobalFrame;

			int snapDistance = NavigationGroundSnapDistance;
			int startMoveDistance = NavigationStartMoveRange;
			int minimumFlyDuration = NavigationMinimumFlyDuration;
			int aimX = NavigationAimX;
			int aimY = NavigationAimY;

			// Refresh Target Nav Pos
			bool hasGroundedTarget = CellNavigation.SnapToGroundNearby(
				aimX, aimY, snapDistance,
				out NavigationTargetX, out NavigationTargetY
			);

			// ?? >> Fly
			if (!hasGroundedTarget) {
				NavigationState = CharacterNavigationState.Fly;
				NavigationFlyStartFrame = Game.GlobalFrame;
			}

			// Fly >> ??
			if (
				hasGroundedTarget &&
				NavigationState == CharacterNavigationState.Fly &&
				Game.GlobalFrame > NavigationFlyStartFrame + minimumFlyDuration
			) {
				NavigationState = CharacterNavigationState.Idle;
			}

			// Idle >> Nav
			if (NavigationState == CharacterNavigationState.Idle) {
				if (Util.SqrtDistance(NavigationTargetX, NavigationTargetY, X, Y) > startMoveDistance * startMoveDistance) {
					NavigationState = CharacterNavigationState.Navigate;
				}
			}

			// Navigate
			if (NavigationState == CharacterNavigationState.Navigate) {
				CurrentNavOperationCount = CellNavigation.Navigate(
					NavOperation, this, NavigationTargetX, NavigationTargetY, 6, 6
				);
				CurrentNavOperationIndex = 0;
			}

		}


		private void NavUpdate_MovementIdle () {
			VelocityX = 0;
			VelocityY = (
				InWater || InSand || IsInsideGround ? 0 :
				CellNavigation.TryGetGroundPosition(X, Y, out int groundY) ? groundY - Y :
				VelocityY - Gravity
			).Clamp(-MaxGravitySpeed, int.MaxValue);
		}


		private void NavUpdate_MovementNavigate () {





		}


		private void NavUpdate_MovementFly () {




		}


		#endregion




		#region --- API ---


		public void Navigate (int x, int y) {
			NavigationAimX = x;
			NavigationAimY = y;
			LastNavigateFrame = Game.GlobalFrame;
		}


		public void ResetNavigation () {
			NavigationState = CharacterNavigationState.Idle;
			NavigationAimX = 0;
			NavigationAimY = 0;
			NavigationTargetX = X;
			NavigationTargetY = Y;
			NavigationFlyStartFrame = int.MinValue;
			LastNavStateRefreshFrame = int.MaxValue;
			CurrentNavOperationIndex = 0;
			CurrentNavOperationCount = 0;
		}


		#endregion




		#region --- LGC ---





		#endregion




	}
}
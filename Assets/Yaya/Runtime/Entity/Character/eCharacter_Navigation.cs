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
		protected virtual int NavigationGroundSearchRangeX => Const.CEL * 10;
		protected virtual int NavigationGroundSearchRangeY => Const.CEL * 8;
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
			int toX = X;
			int toY = Y;
			switch (NavigationState) {
				case CharacterNavigationState.Idle:
					NavUpdate_MovementIdle(ref toY);
					break;
				case CharacterNavigationState.Navigate:
					NavUpdate_MovementMove(ref toX, ref toY);
					break;
				case CharacterNavigationState.Fly:
					NavUpdate_MovementFly(ref toX, ref toY);
					MoveState = MovementState.Fly;
					break;
			}
			X = toX;
			Y = toY;

		}


		// Navigation
		private void NavUpdate_NavigationScan () {

			if (Game.GlobalFrame < LastNavStateRefreshFrame + NavigationTargetScanFrequency) return;
			LastNavStateRefreshFrame = Game.GlobalFrame;

			int rangeX = NavigationGroundSearchRangeX;
			int rangeY = NavigationGroundSearchRangeY;
			int startMoveDistance = NavigationStartMoveRange;
			int minimumFlyDuration = NavigationMinimumFlyDuration;
			int aimX = NavigationAimX;
			int aimY = NavigationAimY;

			// Refresh Target Nav Pos
			bool hasGroundedTarget = CellNavigation.TryGetGroundNearby(
				new RectInt(aimX - rangeX, aimY - rangeY, rangeX * 2, rangeY * 2),
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


		private void NavUpdate_MovementIdle (ref int toY) {
			VelocityY = CellNavigation.TryGetGroundPosition(X, Y, out int groundY) ?
				groundY - Y : IsInsideGround ? 0 :
				VelocityY - Gravity;
			VelocityY = VelocityY.Clamp(-MaxGravitySpeed, int.MaxValue);
			toY = Y + VelocityY;
		}


		private void NavUpdate_MovementMove (ref int toX, ref int toY) {





		}


		private void NavUpdate_MovementFly (ref int toX, ref int toY) {




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
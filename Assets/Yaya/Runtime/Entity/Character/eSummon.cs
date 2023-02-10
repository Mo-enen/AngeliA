using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.MapEditorGroup("Summon")]
	[EntityAttribute.Capacity(1)]
	public abstract class eSummon : eCharacter {




		#region --- SUB ---


		private enum SummonNavigationState {
			Idle, Move, Fly,
		}


		#endregion




		#region --- VAR ---


		// Api
		public eCharacter Owner { get; set; } = null;
		public override int Team => Owner != null ? Owner.Team : YayaConst.TEAM_NEUTRAL;
		protected override bool PhysicsEnable => base.PhysicsEnable && CharacterState != CharacterState.GamePlay;

		// Data
		private SummonNavigationState NavigationState = SummonNavigationState.Idle;
		private bool HasGroundedTarget = true;
		private int NavigationTargetX = 0;
		private int NavigationTargetY = 0;
		private int FlyStartFrame = int.MinValue;
		private static readonly CellNavigation.Operation[] NavOperation = new CellNavigation.Operation[8];


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			NavigationState = SummonNavigationState.Idle;
			NavigationTargetX = X;
			NavigationTargetY = Y;
			HasGroundedTarget = true;
			FlyStartFrame = int.MinValue;
		}


		public override void OnInactived () {
			base.OnInactived();
			Owner = null;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (CharacterState == CharacterState.GamePlay) {
				if (Game.GlobalFrame % 30 == 0) {
					if (Owner != null && Owner.Active) {
						// Follow Owner
						Update_NavigationTarget(
							Owner.X + (InstanceIndex / 2) * Const.CEL * ((InstanceIndex % 2) == 0 ? 1 : -1),
							Owner.Y
						);
					} else {
						// Free Move
						//Update_NavigationTarget(,,, );
					}
				}
				Update_NavigationMovement();
			}
		}


		private void Update_NavigationTarget (int aimX, int aimY) {

			// Refresh Target Nav Pos
			const int MAX_RANGE_X = Const.CEL * 10;
			const int MAX_RANGE_Y = Const.CEL * 8;
			HasGroundedTarget = CellNavigation.TryGetGroundNearby(
				aimX, aimY, 
				new RectInt(aimX - MAX_RANGE_X, aimY - MAX_RANGE_Y, MAX_RANGE_X * 2, MAX_RANGE_Y * 2),
				out NavigationTargetX, out NavigationTargetY
			);

			// ?? >> Fly
			if (!HasGroundedTarget) {
				NavigationState = SummonNavigationState.Fly;
				FlyStartFrame = Game.GlobalFrame;
			}

			// Fly >> ??
			if (
				NavigationState == SummonNavigationState.Fly &&
				HasGroundedTarget &&
				Game.GlobalFrame > FlyStartFrame + 120
			) {
				NavigationState = SummonNavigationState.Idle;
			}

			// Idle >> Move
			if (NavigationState == SummonNavigationState.Idle) {
				const int TRIGGER_DIS = Const.CEL * 6;
				int disSqrt = Util.SqrtDistance(NavigationTargetX, NavigationTargetY, X, Y);
				if (disSqrt > TRIGGER_DIS * TRIGGER_DIS) {
					NavigationState = SummonNavigationState.Move;
				}
			}

		}


		private void Update_NavigationMovement () {
			int toX = X;
			int toY = Y;
			switch (NavigationState) {

				// Idle
				case SummonNavigationState.Idle:



					break;

				// Moving
				case SummonNavigationState.Move:


					break;

				// Flying
				case SummonNavigationState.Fly:


					break;

			}
			X = toX;
			Y = Y.MoveTowards(toY, Const.CEL / 8);
		}


		public virtual void OnSummoned (bool create) => RenderBounce();


		#endregion




		#region --- LGC ---




		#endregion




	}
}
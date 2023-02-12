using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.ExcludeInMapEditor]
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
		public override bool AllowDamageFromLevel => false;
		protected override bool PhysicsEnable => base.PhysicsEnable && CharacterState != CharacterState.GamePlay;

		// Data
		private readonly CellNavigation.Operation[] NavOperation = new CellNavigation.Operation[8];
		private int CurrentNavOperationIndex = 0;
		private SummonNavigationState NavigationState = SummonNavigationState.Idle;
		private bool HasGroundedTarget = true;
		private int NavigationTargetX = 0;
		private int NavigationTargetY = 0;
		private int FlyStartFrame = int.MinValue;
		private int LastNavTargetRefreshFrame = int.MaxValue;
		private int PrevZ = int.MinValue;
		private int SummonFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			ResetNavigation();
		}


		public override void OnInactived () {
			base.OnInactived();
			Owner = null;
		}


		public override void PhysicsUpdate () {

			if (Owner == null || !Owner.Active) {
				Active = false;
				return;
			}

			base.PhysicsUpdate();

			if (CharacterState == CharacterState.GamePlay) {

				// Find Target
				if (Game.GlobalFrame > LastNavTargetRefreshFrame + 30) {
					// Follow Owner
					Update_NavigationTarget(
						Owner.X + (InstanceIndex / 2) * Const.CEL * ((InstanceIndex % 2) == 0 ? 1 : -1),
						Owner.Y
					);
				}

				// Move to Target
				int toX = X;
				int toY = Y;
				switch (NavigationState) {
					case SummonNavigationState.Idle:
						Update_MovementIdle(ref toY);
						break;
					case SummonNavigationState.Move:
						Update_MovementMove(ref toX, ref toY);
						break;
					case SummonNavigationState.Fly:
						Update_MovementFly(ref toX, ref toY);
						break;
				}
				X = X.MoveTowards(toX, RunSpeed);
				Y = Y.MoveTowards(toY, MaxGravitySpeed);
			}

			// Gether when Z Changed
			if (PrevZ != Game.Current.ViewZ) {
				PrevZ = Game.Current.ViewZ;
				X = Owner.X;
				Y = Owner.Y;
			}
		}


		// Navigation
		private void Update_NavigationTarget (int aimX, int aimY) {

			LastNavTargetRefreshFrame = Game.GlobalFrame;

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


		private void Update_MovementIdle (ref int toY) {
			if (CellNavigation.TryGetGroundPosition(X, Y, out int groundY)) {
				toY = groundY;
				VelocityY = 0;
			} else {
				VelocityY -= IsInsideGround ? 0 : Gravity;
				toY = Y + VelocityY;
			}
		}


		private void Update_MovementMove (ref int toX, ref int toY) {





		}


		private void Update_MovementFly (ref int toX, ref int toY) {




		}


		// Misc
		public virtual void OnSummoned (bool create) {
			RenderBounce();
			ResetNavigation();
			SummonFrame = Game.GlobalFrame;
		}


		// Summon
		public static eSummon CreateSummon<T> (eCharacter owner, int x, int y) where T : eSummon => CreateSummon(owner, typeof(T).AngeHash(), x, y);
		public static eSummon CreateSummon (eCharacter owner, int typeID, int x, int y) {
			if (owner == null) return null;
			var game = Game.Current;
			if (game.SpawnEntity(typeID, x, y) is eSummon summon) {
				// Create New
				summon.Owner = owner;
				summon.OnSummoned(true);
				return summon;
			} else {
				// Find Old
				var entities = game.Entities;
				int eLen = game.EntityLen;
				int minSpawnFrame = int.MaxValue;
				eSummon old = null;
				for (int i = 0; i < eLen; i++) {
					var e = entities[i];
					if (
						e.TypeID == typeID &&
						e is eSummon sum &&
						sum.Owner == owner &&
						sum.SummonFrame < minSpawnFrame
					) {
						minSpawnFrame = sum.SummonFrame;
						old = sum;
					}
				}
				// Swape Old
				if (old != null) {
					old.X = x;
					old.Y = y;
					old.Owner = owner;
					old.OnSummoned(false);
					return old;
				}
			}
			return null;
		}


		#endregion




		#region --- LGC ---


		private void ResetNavigation () {
			NavigationState = SummonNavigationState.Idle;
			NavigationTargetX = X;
			NavigationTargetY = Y;
			HasGroundedTarget = true;
			FlyStartFrame = int.MinValue;
			LastNavTargetRefreshFrame = int.MaxValue;
			CurrentNavOperationIndex = 0;
		}


		#endregion




	}
}
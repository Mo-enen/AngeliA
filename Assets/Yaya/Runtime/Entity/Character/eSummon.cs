using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.ExcludeInMapEditor]
	public abstract class eSummon : eCharacter {




		#region --- VAR ---


		// Const
		private const int TARGET_REFRESH_FREQUENCY = 30;

		// Api
		public eCharacter Owner { get; set; } = null;
		public sealed override int Team => Owner != null ? Owner.Team : YayaConst.TEAM_NEUTRAL;
		public override bool AllowDamageFromLevel => false;
		protected override bool PhysicsEnable => base.PhysicsEnable && CharacterState != CharacterState.GamePlay;
		protected override bool NavigationEnable => CharacterState == CharacterState.GamePlay && Owner != null && Owner.Active;

		// Data
		private readonly Vector2Int[] OwnerPosTrail = new Vector2Int[30];
		private int OwnerPosTrailIndex = -1;
		private int SummonFrame = int.MinValue;
		private int PrevZ = int.MinValue;
		private int LastNavStateReloadFrame = int.MaxValue;
		private int LastTrailUpdateFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			OwnerPosTrailIndex = -1;
			Owner = null;
			LastNavStateReloadFrame = int.MaxValue;
		}


		public virtual void OnSummoned (bool create) {
			RenderBounce();
			ResetNavigation();
			LastNavStateReloadFrame = int.MaxValue;
			SummonFrame = Game.GlobalFrame;
		}


		public override void PhysicsUpdate () {

			// Inactive when Owner not Ready
			if (Owner == null || !Owner.Active) {
				Active = false;
				return;
			}

			// Gether when Z Changed
			if (PrevZ != Game.Current.ViewZ) {
				PrevZ = Game.Current.ViewZ;
				X = Owner.X;
				Y = Owner.Y;
				LastNavStateReloadFrame = int.MaxValue;
				ResetNavigation();
			}

			PhysicsUpdate_Trail();

			base.PhysicsUpdate();

			// Motion
			switch (NavigationState) {
				case CharacterNavigationState.Fly:
					MoveState = MovementState.Fly;
					if ((X - NavigationAim.x).Abs() > 96) {
						Move(X < NavigationAim.x ? Direction3.Right : Direction3.Left, 0);
					}
					break;

				case CharacterNavigationState.Navigate:
					if (VelocityX != 0) {
						Move(VelocityX > 0 ? Direction3.Right : Direction3.Left, 0);
					}
					break;
			}

		}


		private void PhysicsUpdate_Trail () {

			if (NavigationState != CharacterNavigationState.Fly) return;

			// Init Check
			if (OwnerPosTrailIndex < 0 || LastTrailUpdateFrame != Game.GlobalFrame) {
				if (Owner != null && Owner.Active) {
					for (int i = 0; i < OwnerPosTrail.Length; i++) {
						OwnerPosTrail[i] = new Vector2Int(Owner.X, Owner.Y);
					}
				} else {
					System.Array.Clear(OwnerPosTrail, 0, OwnerPosTrail.Length);
				}
				OwnerPosTrailIndex = 0;
			}

			// Update
			OwnerPosTrail[OwnerPosTrailIndex] = new(Owner.X, Owner.Y);
			OwnerPosTrailIndex = (OwnerPosTrailIndex + 1).UMod(OwnerPosTrail.Length);
			LastTrailUpdateFrame = Game.GlobalFrame;
		}


		#endregion




		#region --- API ---


		protected override Vector2Int GetNavigationAim () {

			if (Owner == null || !Owner.Active) return base.GetNavigationAim();
			var result = new Vector2Int(Owner.X, Owner.Y);

			switch (NavigationState) {

				case CharacterNavigationState.Navigate:

					if (
						Game.GlobalFrame < LastNavStateReloadFrame + TARGET_REFRESH_FREQUENCY &&
						(!NavOperationDone || !HasNavOperation)
					) break;

					LastNavStateReloadFrame = Game.GlobalFrame;
					ClearNavigation();



					// Test
					if (CellNavigation.ExpandTo(
						Owner.X, Owner.Y + Const.HALF, Owner.X, Owner.Y + Const.HALF, 12, out int groundX, out int groundY
					)) {
						result.x = groundX;
						result.y = groundY;
					}
					// Test



					break;

				case CharacterNavigationState.Fly:

					const int COLUMN_COUNT = 4;

					int sign = InstanceIndex % 2 == 0 ? -1 : 1;
					int row = InstanceIndex / 2 / COLUMN_COUNT;
					int column = (InstanceIndex / 2 % COLUMN_COUNT + 1) * sign;
					int rowSign = (row % 2 == 0) == (sign == 1) ? 1 : -1;

					int instanceOffsetX = column * Const.CEL * 3 / 2 + rowSign * Const.HALF / 2;
					int instanceOffsetY = row * Const.CEL + Const.CEL - column.Abs() * Const.HALF / 3;

					// Result
					var pos = OwnerPosTrail[(OwnerPosTrailIndex + 1).UMod(OwnerPosTrail.Length)];
					result.x = pos.x + instanceOffsetX;
					result.y = pos.y + instanceOffsetY;
					break;

			}
			return result;
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




	}
}
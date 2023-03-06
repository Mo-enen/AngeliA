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
		private const int AIM_REFRESH_FREQUENCY = 120;

		// Api
		public eCharacter Owner { get; set; } = null;
		public sealed override int Team => Owner != null ? Owner.Team : YayaConst.TEAM_NEUTRAL;
		public override bool AllowDamageFromLevel => false;
		public override bool PhysicsEnable => base.PhysicsEnable && CharacterState != CharacterState.GamePlay;
		public override bool NavigationEnable => CharacterState == CharacterState.GamePlay && Owner != null && Owner.Active;

		// Data
		private readonly Vector2Int[] OwnerPosTrail = new Vector2Int[30];
		private int OwnerPosTrailIndex = -1;
		private int SummonFrame = int.MinValue;
		private int PrevZ = int.MinValue;
		private int LastTrailUpdateFrame = int.MinValue;
		private bool RequireAimRefresh = true;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			OwnerPosTrailIndex = -1;
			Owner = null;
			NavigationState = CharacterNavigationState.Operation;
			RequireAimRefresh = true;
		}


		public virtual void OnSummoned (bool create) {
			RenderBounce();
			ResetNavigation();
			SummonFrame = Game.GlobalFrame;
			NavigationState = CharacterNavigationState.Operation;
			RequireAimRefresh = true;
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
				ResetNavigation();
				NavigationState = CharacterNavigationState.Operation;
				RequireAimRefresh = true;
			}

			PhysicsUpdate_Trail();

			base.PhysicsUpdate();

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


		protected override Vector2Int? GetNavigationAim (out bool grounded) {

			if (Owner == null || !Owner.Active) return base.GetNavigationAim(out grounded);
			grounded = false;

			// Scan Frequency Gate
			if (
				!RequireAimRefresh &&
				(Game.GlobalFrame + InstanceIndex) % AIM_REFRESH_FREQUENCY != 0 &&
				HasPerformableOperation
			) return null;
			RequireAimRefresh = false;

			// Get Aim at Ground
			var result = new Vector2Int(Owner.X, Owner.Y);
			int offsetX = Const.CEL * ((InstanceIndex % 12) / 2 + 2) * (InstanceIndex % 2 == 0 ? -1 : 1);
			int offsetY = NavigationState == CharacterNavigationState.Fly ? Const.CEL : Const.HALF;
			if (CellNavigation.ExpandTo(
				Owner.X,
				Owner.Y,
				Owner.X + offsetX,
				Owner.Y + offsetY,
				maxIteration: 12,
				out int groundX, out int groundY
			)) {
				result.x = groundX;
				result.y = groundY;
				grounded = true;
			} else {
				result.x += offsetX;
				grounded = false;
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
				int eLen = game.EntityCount;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.ExcludeInMapEditor]
	public abstract class eSummon : eCharacter {




		#region --- VAR ---


		// Const
		//private const int NavigationGroundNearbyIteration = 6;

		// Api
		public eCharacter Owner { get; set; } = null;
		public sealed override int Team => Owner != null ? Owner.Team : YayaConst.TEAM_NEUTRAL;
		public override bool AllowDamageFromLevel => false;
		protected override bool PhysicsEnable => base.PhysicsEnable && CharacterState != CharacterState.GamePlay;
		protected override bool NavigationEnable => CharacterState == CharacterState.GamePlay && Owner != null && Owner.Active;

		// Data
		private readonly Vector2Int[] OwnerPosTrail = new Vector2Int[24];
		private int OwnerPosTrailIndex = -1;
		private int SummonFrame = int.MinValue;
		private int PrevZ = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			OwnerPosTrailIndex = -1;
		}


		public override void OnInactived () {
			base.OnInactived();
			Owner = null;
		}


		public override void FillPhysics () {
			if (NavigationEnable && MoveState == MovementState.Fly) return;
			base.FillPhysics();
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
			}

			// Update Trail
			if (OwnerPosTrailIndex < 0) {
				// Init Check
				if (Owner != null && Owner.Active) {
					for (int i = 0; i < OwnerPosTrail.Length; i++) {
						OwnerPosTrail[i] = new Vector2Int(Owner.X, Owner.Y);
					}
				} else {
					System.Array.Clear(OwnerPosTrail, 0, OwnerPosTrail.Length);
				}
				OwnerPosTrailIndex = 0;
			}
			// Update Pos
			OwnerPosTrail[OwnerPosTrailIndex] = new(Owner.X, Owner.Y);
			OwnerPosTrailIndex = (OwnerPosTrailIndex + 1).UMod(OwnerPosTrail.Length);

			base.PhysicsUpdate();

		}


		// Misc
		public virtual void OnSummoned (bool create) {
			RenderBounce();
			ResetNavigation();
			SummonFrame = Game.GlobalFrame;
		}


		#endregion




		#region --- API ---


		protected override Vector2Int GetNavigationMoveAim () {

			if (Owner == null || !Owner.Active) return base.GetNavigationMoveAim();

			var result = new Vector2Int(Owner.X, Owner.Y);

			if (CellNavigation.ExpandToGroundNearby(
				result.x, result.y, 6, out int groundX, out int groundY
			)) {
				result.x = groundX;
				result.y = groundY;
			}



			return result;
		}


		protected override Vector2Int GetNavigationFlyAim () {
			if (Owner == null || !Owner.Active) return base.GetNavigationFlyAim();
			var pos = OwnerPosTrail[(OwnerPosTrailIndex + 1).UMod(OwnerPosTrail.Length)];
			return new Vector2Int(
				pos.x + Const.CEL * (InstanceIndex % 2 == 0 ? -2 : 2),
				pos.y + Const.CEL
			);
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
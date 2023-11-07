using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Capacity(1)]
	[EntityAttribute.ExcludeInMapEditor]
	public abstract class Summon : Character, IDamageReceiver {




		#region --- VAR ---


		// Const
		private const int AIM_REFRESH_FREQUENCY = 120;

		// Api
		public Character Owner { get; set; } = null;
		int IDamageReceiver.Team => Owner != null ? (Owner as IDamageReceiver).Team : Const.TEAM_NEUTRAL;
		bool IDamageReceiver.TakeDamageFromEnvironment => false;
		protected override bool NavigationEnable => CharacterState == CharacterState.GamePlay && Owner != null && Owner.Active;
		protected override bool PhysicsEnable => base.PhysicsEnable && CharacterState != CharacterState.GamePlay;
		protected override bool ClampInSpawnRect => Owner == Player.Selecting;
		public override int AttackTargetTeam => Owner != null ? Owner.AttackTargetTeam : 0;
		public int OriginItemID { get; set; } = 0;
		public int InventoryUpdatedFrame { get; set; } = -1;

		// Data
		private int SummonFrame = int.MinValue;
		private int PrevZ = int.MinValue;
		private bool RequireAimRefresh = true;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			Owner = null;
			NavigationState = CharacterNavigationState.Operation;
			RequireAimRefresh = true;
			OriginItemID = 0;
		}


		public virtual void OnSummoned (bool create) {
			Bounce();
			ResetNavigation();
			SummonFrame = Game.GlobalFrame;
			NavigationState = CharacterNavigationState.Operation;
			RequireAimRefresh = true;
		}


		public override void PhysicsUpdate () {

			if (!Active) return;

			// Inactive when Owner not Ready
			if (Owner == null || !Owner.Active) {
				Active = false;
				return;
			}

			// when Z Changed
			if (PrevZ != Stage.ViewZ) {
				PrevZ = Stage.ViewZ;
				X = Owner.X;
				Y = Owner.Y;
				ResetNavigation();
				NavigationState = CharacterNavigationState.Operation;
				RequireAimRefresh = true;
			}

			base.PhysicsUpdate();

		}


		public override void FrameUpdate () {
			if (!Active) return;
			// Check Item Exists
			if (
				Owner != null &&
				OriginItemID != 0 &&
				Game.GlobalFrame > InventoryUpdatedFrame + 1 &&
				Inventory.ItemTotalCount(Owner.TypeID, OriginItemID, true) == 0
			) {
				Active = false;
				return;
			}
			base.FrameUpdate();
		}


		#endregion




		#region --- API ---


		protected override Vector2Int? GetNavigationAim (out bool grounded) {

			if (Owner == null || !Owner.Active) return base.GetNavigationAim(out grounded);
			grounded = false;

			// Scan Frequency Gate
			int insIndex = InstanceOrder;
			if (
				!RequireAimRefresh &&
				(Game.GlobalFrame + insIndex) % AIM_REFRESH_FREQUENCY != 0 &&
				HasPerformableOperation
			) return null;
			RequireAimRefresh = false;

			// Get Aim at Ground
			var result = new Vector2Int(Owner.X, Owner.Y);
			int offsetX = Const.CEL * ((insIndex % 12) / 2 + 2) * (insIndex % 2 == 0 ? -1 : 1);
			int offsetY = NavigationState == CharacterNavigationState.Fly ? Const.CEL : Const.HALF;
			if (CellNavigation.ExpandTo(
				Game.GlobalFrame, Stage.ViewRect,
				Owner.X, Owner.Y,
				Owner.X + offsetX, Owner.Y + offsetY,
				maxIteration: 12, out int groundX, out int groundY
			)) {
				result.x = groundX;
				result.y = groundY;
				grounded = true;
			} else {
				result.x += offsetX;
				grounded = false;
			}

			// Shift
			result = new Vector2Int(
				result.x + (InstanceOrder % 2 == 0 ? 8 : -8) * (InstanceOrder / 2),
				result.y
			);

			return result;
		}


		// Summon
		public static T CreateSummon<T> (Character owner, int x, int y) where T : Summon => CreateSummon(owner, typeof(T).AngeHash(), x, y) as T;
		public static Summon CreateSummon (Character owner, int typeID, int x, int y) {
			if (owner == null) return null;
			if (Stage.SpawnEntity(typeID, x, y) is Summon summon) {
				// Create New
				summon.Owner = owner;
				summon.OnSummoned(true);
				return summon;
			} else {
				// Find Old
				var entities = Stage.Entities[Const.ENTITY_LAYER_GAME];
				int eLen = Stage.EntityCounts[Const.ENTITY_LAYER_GAME];
				int minSpawnFrame = int.MaxValue;
				Summon old = null;
				for (int i = 0; i < eLen; i++) {
					var e = entities[i];
					if (
						e.TypeID == typeID &&
						e is Summon sum &&
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
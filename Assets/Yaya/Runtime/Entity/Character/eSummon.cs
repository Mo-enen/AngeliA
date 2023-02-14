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


		// Api
		public eCharacter Owner { get; set; } = null;
		public sealed override int Team => Owner != null ? Owner.Team : YayaConst.TEAM_NEUTRAL;
		public override bool AllowDamageFromLevel => false;
		protected override bool PhysicsEnable => base.PhysicsEnable && CharacterState != CharacterState.GamePlay;

		// Data
		private int SummonFrame = int.MinValue;
		private int PrevZ = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnInactived () {
			base.OnInactived();
			Owner = null;
		}


		public override void PhysicsUpdate () {

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

			// Nav to Owner
			//Navigate(
			//	Owner.X + (InstanceIndex / 2) * Const.CEL * ((InstanceIndex % 2) == 0 ? 1 : -1),
			//	Owner.Y
			//);





			base.PhysicsUpdate();

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





		#endregion




	}
}
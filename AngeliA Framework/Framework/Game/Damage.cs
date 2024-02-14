using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {


	public struct Damage {
		public readonly bool IsPhysical => Type == 0 || Type == SpriteTag.DAMAGE_TAG;
		public readonly bool IsExplosive => Type == SpriteTag.DAMAGE_EXPLOSIVE_TAG;
		public readonly bool IsMagical => Type == SpriteTag.DAMAGE_MAGICAL_TAG;
		public int Amount;
		public int Type;
		public Entity Sender;
		public Damage (int amount, Entity sender, int type = 0) {
			Amount = amount;
			Sender = sender;
			Type = type;
		}
	}


	public interface IDamageReceiver {

		public int Team { get; }
		public bool TakeDamageFromLevel => true;

		void TakeDamage (Damage damage);

		[OnGameUpdateLater]
		public static void DamageUpdateFromLevel () {
			PerformDamageCheck(EntityLayer.GAME);
			PerformDamageCheck(EntityLayer.CHARACTER);
			static void PerformDamageCheck (int entityLayer) {
				int len = Stage.EntityCounts[entityLayer];
				var entities = Stage.Entities[entityLayer];
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (
						entity is not IDamageReceiver receiver ||
						!receiver.TakeDamageFromLevel ||
						receiver.Team == Const.TEAM_ENVIRONMENT
					) continue;
					var hits = CellPhysics.OverlapAll(PhysicsMask.DAMAGE, entity.Rect, out int count, entity, OperationMode.ColliderAndTrigger);
					for (int j = 0; j < count; j++) {
						var hit = hits[j];
						receiver.TakeDamage(new Damage(1, null, hit.Tag));
					}
				}
			}
		}

	}
}
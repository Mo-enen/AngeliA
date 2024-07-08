using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public struct Damage {
	public int Amount;
	public Tag Type;
	public Entity Sender;
	public Entity Bullet;
	public Damage (int amount, Entity sender, Entity bullet = null, Tag type = Tag.PhysicalDamage) {
		Amount = amount;
		Sender = sender;
		Type = type;
		Bullet = bullet;
	}
}


public interface IDamageReceiver {

	public int Team { get; }
	public bool TakeDamageFromLevel => true;

	void TakeDamage (Damage damage);

	[OnGameUpdateLater]
	public static void DamageUpdateFromLevel () {
		if (!Stage.Enable) return;
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
				var hits = Physics.OverlapAll(PhysicsMask.DAMAGE, entity.Rect, out int count, entity, OperationMode.ColliderAndTrigger);
				for (int j = 0; j < count; j++) {
					var hit = hits[j];
					receiver.TakeDamage(new Damage(1, null, null, hit.Tag));
				}
			}
		}
	}

}
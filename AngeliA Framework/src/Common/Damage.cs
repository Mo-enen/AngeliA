using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Data structure for a single damage performed
/// </summary>
/// <param name="amount">How many damage it deals</param>
/// <param name="targetTeam">What team does it attacks</param>
/// <param name="bullet">Which bullet does it came from</param>
/// <param name="type">What special type does it holds</param>
public struct Damage (int amount, int targetTeam = Const.TEAM_ALL, Entity bullet = null, Tag type = Tag.PhysicalDamage) {

	public int Amount = amount;
	public int TargetTeam = targetTeam;
	public Tag Type = type;
	public Entity Bullet = bullet;
	/// <summary>
	/// Do not make target become invincible after take this damage
	/// </summary>
	public bool IgnoreInvincible = false;
	/// <summary>
	/// Do not make target stun after take this damage
	/// </summary>
	public bool IgnoreStun = false;

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
					receiver.TakeDamage(new Damage(1, type: hits[j].Tag));
				}
			}
		}
	}

}

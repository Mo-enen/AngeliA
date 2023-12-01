using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public interface IDamageReceiver {

		public int Team { get; }
		public bool TakeDamageFromEnvironment => true;

		void TakeDamage (int damage, Entity sender);

		[OnGameUpdateLater]
		public static void OnGameUpdateLater () {
			PerformDamageCheck(EntityLayer.GAME);
			PerformDamageCheck(EntityLayer.CHARACTER);
		}

		private static void PerformDamageCheck (int entityLayer) {
			int len = Stage.EntityCounts[entityLayer];
			var entities = Stage.Entities[entityLayer];
			for (int i = 0; i < len; i++) {
				var entity = entities[i];
				if (entity is not IDamageReceiver receiver) continue;
				var hits = CellPhysics.OverlapAll(PhysicsMask.DAMAGE, entity.Rect, out int count, entity, OperationMode.ColliderAndTrigger);
				for (int j = 0; j < count; j++) {
					var hit = hits[j];
					if (receiver.TakeDamageFromEnvironment && receiver.Team != Const.TEAM_ENVIRONMENT) {
						receiver.TakeDamage(hit.Tag, null);
					}
				}
			}
		}

	}
}
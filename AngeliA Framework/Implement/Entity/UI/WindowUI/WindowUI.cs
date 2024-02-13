using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Capacity(1, 0)]
	public abstract class WindowUI : EntityUI {

		[OnGameQuitting]
		public static void OnGameQuitting () {
			int len = Stage.EntityCounts[EntityLayer.UI];
			var entities = Stage.Entities[EntityLayer.UI];
			for (int i = 0; i < len; i++) {
				var e = entities[i];
				if (e is WindowUI window && e.Active) {
					e.OnInactivated();
				}
			}
		}

		public static void OpenWindow (int typeID) => Stage.SpawnEntity(typeID, 0, 0);

		public static void CloseWindow (int typeID) {
			int len = Stage.EntityCounts[EntityLayer.UI];
			var entities = Stage.Entities[EntityLayer.UI];
			for (int i = 0; i < len; i++) {
				var e = entities[i];
				if (e.TypeID == typeID) e.Active = false;
			}
		}

	}
}
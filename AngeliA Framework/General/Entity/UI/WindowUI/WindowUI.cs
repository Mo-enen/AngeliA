using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Capacity(1, 0)]
	public abstract class WindowUI : EntityUI {

		public static WindowUI Instance { get; private set; } = null;
		public static bool MouseOutside { get; set; } = false;
		public static IRect MainWindowRect { get; set; } = default;

		[OnGameQuitting]
		public static void OnGameQuitting () {
			if (Game.AllowMakerFeaures && Instance != null && Instance.Active) Instance.OnInactivated();
		}

		public override void OnActivated () {
			base.OnActivated();
			if (Instance != null && Instance != this) Instance.Active = false;
			Instance = this;
		}

		public static void OpenWindow (int typeID) {
			if (!Game.AllowMakerFeaures) return;
			Stage.ClearStagedEntities();
			Stage.SpawnEntity(typeID, 0, 0);
		}

	}
}
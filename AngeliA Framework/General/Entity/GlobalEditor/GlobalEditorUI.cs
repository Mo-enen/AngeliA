using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Capacity(1, 0)]
	public abstract class GlobalEditorUI : EntityUI {


		// Api
		public static GlobalEditorUI Instance { get; private set; } = null;
		private static System.Action RestartGameImmediately;


		// MSG
		[OnGameInitialize]
		internal static void OnGameInitialize () => RestartGameImmediately += () => Game.RestartGame(immediately: true);


		[OnGameQuitting]
		public static void OnGameQuitting () {
			if (Instance != null && Instance.Active) Instance.OnInactivated();
		}


		public override void OnActivated () {
			base.OnActivated();
			if (Instance != null && Instance != this) Instance.Active = false;
			Instance = this;
		}


		public static void OpenEditorSmoothly (int typeID, bool fade = true) {
			FrameTask.EndAllTask();
			if (fade) {
				// During Gameplay
				FrameTask.AddToLast(FadeOutTask.TYPE_ID, 50);
				if (FrameTask.AddToLast(SpawnEntityTask.TYPE_ID) is SpawnEntityTask task) {
					task.EntityID = typeID;
					task.X = 0;
					task.Y = 0;
				}
				FrameTask.AddToLast(FadeInTask.TYPE_ID, 50);
			} else {
				// On Game Start
				CellRenderer.DrawBlackCurtain(1000);
				if (FrameTask.AddToLast(SpawnEntityTask.TYPE_ID) is SpawnEntityTask task) {
					task.EntityID = typeID;
					task.X = 0;
					task.Y = 0;
				}
			}
		}


		public static void CloseEditorSmoothly () {
			if (Instance == null || !Instance.Active) return;
			FrameTask.EndAllTask();
			FrameTask.AddToLast(FadeOutTask.TYPE_ID, 50);
			FrameTask.AddToLast(DespawnEntityTask.TYPE_ID, Instance);
			FrameTask.AddToLast(MethodTask.TYPE_ID, RestartGameImmediately);
		}




	}
}
using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public partial class SheetEditor : GlobalEditorUI {




		#region --- VAR ---


		// Api
		public static bool IsEditing => Instance != null && Instance.Active;
		public new static SheetEditor Instance => GlobalEditorUI.Instance as SheetEditor;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			WorldSquad.Enable = false;
			Stage.DespawnAllEntitiesFromWorld();
			if (Player.Selecting != null) {
				Player.Selecting.Active = false;
			}
		}


		public override void OnInactivated () {
			base.OnInactivated();
			WorldSquad.Enable = true;
		}


		public override void UpdateUI () {
			base.UpdateUI();

			// Skybox
			Game.SetSkyboxTint(Const.BLACK, Const.BLACK);





		}


		#endregion




		#region --- API ---


		public static void OpenSheetEditorSmoothly (bool fade = true) {
			FrameTask.EndAllTask();
			if (fade) {
				FrameTask.AddToLast(FadeOutTask.TYPE_ID, 50);
				if (FrameTask.AddToLast(SpawnEntityTask.TYPE_ID) is SpawnEntityTask task) {
					task.EntityID = typeof(SheetEditor).AngeHash();
					task.X = 0;
					task.Y = 0;
				}
				FrameTask.AddToLast(FadeInTask.TYPE_ID, 50);
			} else {
				CellRenderer.DrawBlackCurtain(1000);
				if (FrameTask.AddToLast(SpawnEntityTask.TYPE_ID) is SpawnEntityTask task) {
					task.EntityID = typeof(SheetEditor).AngeHash();
					task.X = 0;
					task.Y = 0;
				}
			}
		}


		public static void CloseSheetEditorSmoothly () {
			FrameTask.EndAllTask();
			FrameTask.AddToLast(FadeOutTask.TYPE_ID, 50);
			FrameTask.AddToLast(DespawnEntityTask.TYPE_ID, Instance);
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}
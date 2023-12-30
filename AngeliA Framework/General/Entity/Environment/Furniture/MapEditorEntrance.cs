namespace AngeliaFramework {


	public class MapEditorEntranceWood : MapEditorEntrance { }


	[EntityAttribute.Capacity(1, 0)]
	public abstract class MapEditorEntrance : Furniture, IActionTarget {

		private static readonly int HINT_START = "CtrlHint.EditMap".AngeHash();
		private static readonly int HINT_QUIT = "CtrlHint.QuitEditing".AngeHash();

		public override void FrameUpdate () {
			base.FrameUpdate();
			// Draw Hint
			if ((this as IActionTarget).IsHighlighted) {
				bool editing = MapEditor.Instance != null && MapEditor.Instance.Active;
				ControlHintUI.DrawGlobalHint(
					X, Y + Height + Const.CEL * 2, Gamekey.Action,
					editing ? Language.Get(HINT_QUIT, "Quit Editing Map") : Language.Get(HINT_START, "Edit Map"), true
				);
			}

		}

		void IActionTarget.Invoke () {
			if (FrameTask.HasTask()) return;
			bool editing = MapEditor.Instance != null && MapEditor.Instance.Active;
			if (editing) {
				GlobalEditorUI.CloseEditorSmoothly();
			} else {
				GlobalEditorUI.OpenEditorSmoothly(MapEditor.TYPE_ID);
			}
		}

		bool IActionTarget.AllowInvoke () => !FrameTask.HasTask();

	}
}
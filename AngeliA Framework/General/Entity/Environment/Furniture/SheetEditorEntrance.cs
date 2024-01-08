namespace AngeliaFramework {


	public class SheetEditorEntranceWood : MapEditorEntrance { }


	[EntityAttribute.Capacity(1, 0)]
	public abstract class SheetEditorEntrance : Furniture, IActionTarget {

		private static readonly int HINT_START = "CtrlHint.EditSheet".AngeHash();
		private static readonly int HINT_QUIT = "CtrlHint.QuitEditing".AngeHash();

		public override void FrameUpdate () {
			base.FrameUpdate();
			// Draw Hint
			if ((this as IActionTarget).IsHighlighted) {
				bool editing = SheetEditor.Instance != null && SheetEditor.Instance.Active;
				ControlHintUI.DrawGlobalHint(
					X, Y + Height + Const.CEL * 2, Gamekey.Action,
					editing ? Language.Get(HINT_QUIT, "Quit Editing") : Language.Get(HINT_START, "Edit Sheet"), true
				);
			}

		}

		void IActionTarget.Invoke () {
			if (FrameTask.HasTask()) return;
			bool editing = MapEditor.Instance != null && MapEditor.Instance.Active;
			if (editing) {
				GlobalEditorUI.CloseEditorSmoothly();
			} else {
				GlobalEditorUI.OpenEditorSmoothly(SheetEditor.TYPE_ID);
			}
		}

		bool IActionTarget.AllowInvoke () => !FrameTask.HasTask();

	}
}
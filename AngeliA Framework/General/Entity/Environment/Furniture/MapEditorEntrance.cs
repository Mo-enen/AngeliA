namespace AngeliaFramework {



	[RequireLanguageFromField]
	public class LanguageEditorEntranceWood : GlobalEditorEntrance<LanguageEditor> {
		private static readonly LanguageCode HINT_START = "CtrlHint.EditLanguage";
		protected override int StartHint => HINT_START;
		protected override bool IsEditing => LanguageEditor.IsActived;
	}


	[RequireLanguageFromField]
	public class SheetEditorEntranceWood : GlobalEditorEntrance<SheetEditor> {
		private static readonly LanguageCode HINT_START = "CtrlHint.EditSheet";
		protected override int StartHint => HINT_START;
		protected override bool IsEditing => SheetEditor.IsActived;
	}


	[RequireLanguageFromField]
	public class MapEditorEntranceWood : GlobalEditorEntrance<MapEditor> {
		private static readonly LanguageCode HINT_START = "CtrlHint.EditMap";
		protected override int StartHint => HINT_START.ID;
		protected override bool IsEditing => MapEditor.IsActived;
	}


	// Entrance
	[EntityAttribute.Capacity(1, 0)]
	[RequireLanguageFromField]
	public abstract class GlobalEditorEntrance<E> : Furniture, IActionTarget where E : GlobalEditorUI {

		protected abstract int StartHint { get; }
		protected abstract bool IsEditing { get; }
		private static readonly LanguageCode HINT_QUIT = "CtrlHint.QuitEditing";
		private readonly int TargetTypeID = 0;

		public GlobalEditorEntrance () => TargetTypeID = typeof(E).AngeHash();

		public override void FrameUpdate () {
			base.FrameUpdate();
			// Draw Hint
			if ((this as IActionTarget).IsHighlighted) {
				ControlHintUI.DrawGlobalHint(
					X, Y + Height + Const.CEL * 2, Gamekey.Action,
					IsEditing ? Language.Get(HINT_QUIT, "Quit Editing") : Language.Get(StartHint, "Edit Map"), true
				);
			}

		}

		void IActionTarget.Invoke () {
			if (FrameTask.HasTask()) return;
			if (IsEditing) {
				GlobalEditorUI.CloseEditorSmoothly();
			} else {
				GlobalEditorUI.OpenEditorSmoothly(TargetTypeID);
			}
		}

		bool IActionTarget.AllowInvoke () => !FrameTask.HasTask();

	}
}
namespace AngeliaFramework {


	public class SheetEditorEntranceWood : SheetEditorEntrance, ICombustible {
		int ICombustible.BurnStartFrame { get; set; }
	}


	[EntityAttribute.Capacity(1, 0)]
	public abstract class SheetEditorEntrance : Furniture, IActionTarget {

		private static readonly int HINT_START = "CtrlHint.EditSheet".AngeHash();

		public override void FrameUpdate () {
			base.FrameUpdate();
			// Draw Hint
			if ((this as IActionTarget).IsHighlighted) {
				ControlHintUI.DrawGlobalHint(
					X, Y + Height + Const.CEL * 2, Gamekey.Action,
					Language.Get(HINT_START, "Edit Sheet"), true
				);
			}

		}

		void IActionTarget.Invoke () {
			if (FrameTask.HasTask()) return;
			GlobalEditorUI.OpenEditorSmoothly(SheetEditor.TYPE_ID);
		}

		bool IActionTarget.AllowInvoke () => !FrameTask.HasTask() && !SheetEditor.IsActived;

	}
}
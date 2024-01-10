using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public partial class SheetEditor : GlobalEditorUI {




		#region --- VAR ---


		// Const 
		public static readonly int TYPE_ID = typeof(SheetEditor).AngeHash();
		private const int PANEL_WIDTH = 300;

		// Api
		public new static SheetEditor Instance => GlobalEditorUI.Instance as SheetEditor;
		public static bool IsActived => Instance != null && Instance.Active;

		// Data


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
		}


		public override void OnInactivated () {
			base.OnInactivated();
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
			ControlHintUI.ForceOffset(Unify(PANEL_WIDTH), 0);
		}


		public override void UpdateUI () {
			base.UpdateUI();



		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}
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
		public new static bool IsActived => Instance != null && Instance.Active;

		// Data


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			WorldSquad.Enable = false;
			Stage.ClearStagedEntities();
			if (Player.Selecting != null) Player.Selecting.Active = false;
			Player.Selecting = null;
		}


		public override void OnInactivated () {
			base.OnInactivated();
			WorldSquad.Enable = true;
			Game.RestartGame(immediately: true);
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
			ControlHintUI.ForceShowHint();
			ControlHintUI.ForceHideGamepad();
			ControlHintUI.ForceOffset(Unify(PANEL_WIDTH), 0);
			Skybox.ForceSkyboxTint(new Byte4(32, 33, 37, 255), new Byte4(32, 33, 37, 255));
		}


		public override void UpdateUI () {
			base.UpdateUI();



		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}
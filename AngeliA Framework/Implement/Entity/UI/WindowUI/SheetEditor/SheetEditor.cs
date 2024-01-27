using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace AngeliaFramework {
	//[RequireSpriteFromField]
	//[RequireLanguageFromField]
	public partial class SheetEditor : WindowUI {




		#region --- SUB ---


		private class RequirementData {
			public string SpriteName;
			public string AtlasName;
		}


		#endregion




		#region --- VAR ---


		// Const 
		private const int PANEL_WIDTH = 300;
		public static readonly int TYPE_ID = typeof(SheetEditor).AngeHash();

		// Api
		public new static SheetEditor Instance => WindowUI.Instance as SheetEditor;
		public static bool IsActived => Instance != null && Instance.Active;

		// Data
		private static readonly Dictionary<int, RequirementData> RequirementPool = new();
		private readonly EditableSheet Sheet = new();
		private string CurrentAtlasName = "";


		#endregion




		#region --- MSG ---


		[OnGameInitialize]
		public static void OnGameInitialize () {
			// Init Requirement
			RequirementPool.Clear();
			foreach (var (name, atlasName) in AngeUtil.ForAllSpriteNameRequirements()) {
				int id = name.AngeHash();
				if (RequirementPool.ContainsKey(id)) continue;
				RequirementPool.Add(id, new RequirementData() {
					SpriteName = name,
					AtlasName = atlasName,
				});
			}
		}


		public override void OnActivated () {
			base.OnActivated();
			Sheet.LoadFromDisk(ProjectSystem.CurrentProject.SheetPath);
			CurrentAtlasName = "";
			Game.StopGame();
		}


		public override void OnInactivated () {
			base.OnInactivated();
			Sheet.Clear();
			CurrentAtlasName = "";
		}


		// Update
		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
			ControlHintUI.ForceOffset(Unify(PANEL_WIDTH), 0);
			ControlHintUI.ForceHideGamepad();
			Skybox.ForceSkyboxTint(new Byte4(32, 33, 37, 255), new Byte4(32, 33, 37, 255));
		}


		public override void UpdateUI () {
			base.UpdateUI();

			int leftPanelWidth = Unify(PANEL_WIDTH);

			// Panel Rect
			var panelRect = MainWindowRect.EdgeInside(Direction4.Left, leftPanelWidth);
			if (string.IsNullOrEmpty(CurrentAtlasName)) {
				Update_FilePanel(panelRect);
			} else {
				Update_InspectorPanel(panelRect);
			}

			// Content
			if (Sheet.NotEmpty) {
				Update_ContentPanel(MainWindowRect.EdgeInside(Direction4.Right, MainWindowRect.width - leftPanelWidth));
			} else {
				Update_EmptySheetPanel(MainWindowRect.EdgeInside(Direction4.Right, MainWindowRect.width - leftPanelWidth));
			}

		}


		private void Update_FilePanel (IRect panelRect) {

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.GREY_12, z: 0);

			// Atlas List




		}


		private void Update_InspectorPanel (IRect panelRect) {

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.GREY_12, z: 0);


		}


		private void Update_ContentPanel (IRect panelRect) {




		}


		private void Update_EmptySheetPanel (IRect panelRect) {




		}


		#endregion




		#region --- LGC ---





		#endregion




	}
}
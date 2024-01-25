using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[RequireLanguageFromField]
	public class HomeScreen : WindowUI {




		#region --- SUB ---


		private enum PanelType { MyLevel, Downloaded, Hot, New, }


		#endregion




		#region --- VAR ---


		// Const
		public static readonly int TYPE_ID = typeof(HomeScreen).AngeHash();
		private static readonly LanguageCode PANEL_MY_LEVEL = ("HomeScreen.MyLevel", "My Level");
		private static readonly LanguageCode PANEL_DOWNLOAD = ("HomeScreen.Downloaded", "Downloaded");
		private static readonly LanguageCode PANEL_HOT = ("HomeScreen.Hot", "Hot");
		private static readonly LanguageCode PANEL_NEW = ("HomeScreen.New", "New");

		// Api
		public new static HomeScreen Instance => WindowUI.Instance as HomeScreen;
		public static bool IsActived => Instance != null && Instance.Active;

		// Data
		private PanelType CurrentPanel = PanelType.Hot;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			Game.StopGame();
			LoadPanel(CurrentPanel);
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
			ControlHintUI.ForceHideGamepad();
			Skybox.ForceSkyboxTint(new Byte4(32, 33, 37, 255), new Byte4(32, 33, 37, 255));
		}


		public override void UpdateUI () {
			base.UpdateUI();

			Update_Panel(MainWindowRect.EdgeInside(Direction4.Left, Unify(300)));




		}


		private void Update_Panel (IRect panelRect) {

			int panelPadding = Unify(6);
			int itemPadding = Unify(6);
			int markWidth = Unify(6);

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK, z: 0);

			// Content
			panelRect = panelRect.Shrink(panelPadding, panelPadding, panelPadding, panelPadding * 4);
			var rect = panelRect.EdgeInside(Direction4.Up, Unify(48));

			DrawButton(rect, PANEL_MY_LEVEL, PanelType.MyLevel);
			rect.y -= rect.height + itemPadding;

			DrawButton(rect, PANEL_DOWNLOAD, PanelType.Downloaded);
			rect.y -= rect.height + itemPadding;

			DrawButton(rect, PANEL_HOT, PanelType.Hot);
			rect.y -= rect.height + itemPadding;

			DrawButton(rect, PANEL_NEW, PanelType.New);
			rect.y -= rect.height + itemPadding;

			// Func
			void DrawButton (IRect rect, string label, PanelType panel) {

				bool selecting = CurrentPanel == panel;

				// Button
				if (CellGUI.Button(rect, label, z: 1, labelTint: Const.GREY_230, enable: !selecting)) {
					LoadPanel(panel);
				}

				// Mark
				if (selecting) {
					CellRenderer.Draw(Const.PIXEL, rect.EdgeInside(Direction4.Right, markWidth), Const.GREEN, z: 3);
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_20, z: 2);
				}

			}
		}


		#endregion




		#region --- LGC ---


		private void LoadPanel (PanelType panel) {
			if (panel == CurrentPanel) return;
			CurrentPanel = panel;




		}


		#endregion




	}
}
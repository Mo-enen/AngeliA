using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[RequireLanguageFromField]
	public class HomeScreen : WindowUI {




		#region --- SUB ---


		private enum ContentType { MyLevel, Downloaded, Hot, New, Sub, }


		#endregion




		#region --- VAR ---


		// Const
		public static readonly int TYPE_ID = typeof(HomeScreen).AngeHash();
		private static readonly LanguageCode PANEL_MY_LEVEL = ("HomeScreen.MyLevel", "My Level");
		private static readonly LanguageCode PANEL_DOWNLOAD = ("HomeScreen.Downloaded", "Downloaded");
		private static readonly LanguageCode PANEL_HOT = ("HomeScreen.Hot", "Hot");
		private static readonly LanguageCode PANEL_NEW = ("HomeScreen.New", "New");
		private static readonly LanguageCode PANEL_SUB = ("HomeScreen.Sub", "Subscribe");

		// Api
		public static HomeScreen Instance { get; private set; }
		public static bool IsActived => Instance != null && Instance.Active;

		// Data
		private ContentType CurrentContent = ContentType.Hot;


		#endregion




		#region --- MSG ---


		public HomeScreen () => Instance = this;


		public override void OnActivated () {
			base.OnActivated();
			Game.StopGame();
			LoadContent(CurrentContent);
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
			ControlHintUI.ForceHideGamepad();
			var mainRect = CellRenderer.CameraRect;
			X = mainRect.x;
			Y = mainRect.y;
			Width = mainRect.width;
			Height = mainRect.height;
		}


		public override void UpdateWindowUI () {
			int panelWidth = Unify(300);
			var mainRect = CellRenderer.CameraRect;
			Update_Panel(mainRect.EdgeInside(Direction4.Left, panelWidth));
			Update_Content(mainRect.EdgeInside(Direction4.Right, mainRect.width - panelWidth));
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

			DrawButton(rect, PANEL_MY_LEVEL, ContentType.MyLevel);
			rect.y -= rect.height + itemPadding;

			DrawButton(rect, PANEL_DOWNLOAD, ContentType.Downloaded);
			rect.y -= rect.height + itemPadding;

			DrawButton(rect, PANEL_HOT, ContentType.Hot);
			rect.y -= rect.height + itemPadding;

			DrawButton(rect, PANEL_NEW, ContentType.New);
			rect.y -= rect.height + itemPadding;

			DrawButton(rect, PANEL_SUB, ContentType.Sub);
			rect.y -= rect.height + itemPadding;

			// Func
			void DrawButton (IRect rect, string label, ContentType type) {

				bool selecting = CurrentContent == type;

				// Button
				if (CellGUI.Button(rect, label, z: 1, labelTint: Const.GREY_230, enable: !selecting)) {
					LoadContent(type);
				}

				// Mark
				if (selecting) {
					CellRenderer.Draw(Const.PIXEL, rect.EdgeInside(Direction4.Right, markWidth), Const.GREEN, z: 3);
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_20, z: 2);
				}

			}
		}


		private void Update_Content (IRect panelRect) {







		}


		#endregion




		#region --- LGC ---


		private void LoadContent (ContentType type) {
			if (type == CurrentContent) return;
			CurrentContent = type;




		}


		#endregion




	}
}
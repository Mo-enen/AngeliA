using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[RequireSpriteFromField]
	[RequireLanguageFromField]
	public static class GlobalTabs {

		private static readonly SpriteCode BUTTON_ICON_MAP_EDITOR = "Window.MapEditor";
		private static readonly SpriteCode BUTTON_ICON_SHEET_EDITOR = "Window.SheetEditor";
		private static readonly SpriteCode BUTTON_ICON_LANGUAGE_EDITOR = "Window.LanguageEditor";
		private static readonly SpriteCode BUTTON_ICON_HOME_SCREEN = "Window.HomeScreen";
		private static readonly LanguageCode LABEL_MAP_EDITOR = ("Window.Label.MapEditor", "Map");
		private static readonly LanguageCode LABEL_SHEET_EDITOR = ("Window.Label.SheetEditor", "Sheet");
		private static readonly LanguageCode LABEL_LANGUAGE_EDITOR = ("Window.Label.LanguageEditor", "Language");
		private static readonly LanguageCode LABEL_HOME_SCREEN = ("Window.Label.HomeScreen", "Home");

		[OnGameUpdate]
		public static void DrawPanelTabsUI () {

			if (!Game.AllowMakerFeaures) return;
			if (Player.HasActivePlayer) {
				WindowUI.MainWindowRect = CellRenderer.CameraRect;
				return;
			}

			CellRenderer.SetLayerToUI();

			int tabWidth = CellGUI.Unify(164);
			int tabHeight = CellGUI.Unify(36);
			const int padding = 0;
			WindowUI.MainWindowRect = CellRenderer.CameraRect.EdgeInside(Direction4.Down, CellRenderer.CameraRect.height - tabHeight);
			var barRect = CellRenderer.CameraRect.EdgeInside(Direction4.Up, tabHeight);
			var rect = new IRect(barRect.x, barRect.y, tabWidth, tabHeight);

			// BG
			CellRenderer.Draw(Const.PIXEL, barRect, Const.GREY_12, z: int.MaxValue - 2);

			// Home Editor
			if (DrawTab(rect, HomeScreen.IsActived, LABEL_HOME_SCREEN, BUTTON_ICON_HOME_SCREEN)) {
				WindowUI.OpenWindow(HomeScreen.TYPE_ID);
			}
			rect.x += tabWidth + padding;

			if (!ProjectSystem.CurrentProject.Readonly) {
				// Map Editor
				if (DrawTab(rect, MapEditor.IsActived, LABEL_MAP_EDITOR, BUTTON_ICON_MAP_EDITOR)) {
					WindowUI.OpenWindow(MapEditor.TYPE_ID);
				}
				rect.x += tabWidth + padding;

				// Sheet Editor
				if (DrawTab(rect, SheetEditor.IsActived, LABEL_SHEET_EDITOR, BUTTON_ICON_SHEET_EDITOR)) {
					WindowUI.OpenWindow(SheetEditor.TYPE_ID);
				}
				rect.x += tabWidth + padding;

				if (Game.IsEdittime) {
					// Language Editor
					if (DrawTab(rect, LanguageEditor.IsActived, LABEL_LANGUAGE_EDITOR, BUTTON_ICON_LANGUAGE_EDITOR)) {
						WindowUI.OpenWindow(LanguageEditor.TYPE_ID);
					}
					rect.x += tabWidth + padding;
				}
			}

			// Block Event
			WindowUI.MouseOutside = WindowUI.MouseOutside || barRect.MouseInside();
			if (FrameInput.MouseLeftButton && WindowUI.MouseOutside) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}

			CellRenderer.SetLayerToDefault();
		}

		[OnGameUpdatePauseless]
		public static void OnGameUpdatePauseless () => WindowUI.MouseOutside = false;

		private static bool DrawTab (IRect rect, bool actived, string label, int iconID) {
			if (actived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_96, int.MaxValue - 1);
			}
			if (CellGUI.Button(rect, label, out var bounds, z: int.MaxValue) && !actived) {
				return true;
			}
			CellRenderer.Draw(
				iconID,
				new IRect(bounds.x - rect.height, rect.y, rect.height, rect.height),
				z: int.MaxValue - 1
			);
			return false;
		}

	}
}
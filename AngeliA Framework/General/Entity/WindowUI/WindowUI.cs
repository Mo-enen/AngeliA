using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Capacity(1, 0)]
	[RequireSpriteFromField]
	[RequireLanguageFromField]
	public abstract class WindowUI : EntityUI {




		#region --- VAR ---


		// Const
		private static readonly SpriteCode BUTTON_ICON_MAP_EDITOR = "Window.MapEditor";
		private static readonly SpriteCode BUTTON_ICON_SHEET_EDITOR = "Window.SheetEditor";
		private static readonly SpriteCode BUTTON_ICON_LANGUAGE_EDITOR = "Window.LanguageEditor";
		private static readonly SpriteCode BUTTON_ICON_HOME_SCREEN = "Window.HomeScreen";
		private static readonly LanguageCode LABEL_MAP_EDITOR = "Window.Label.MapEditor";
		private static readonly LanguageCode LABEL_SHEET_EDITOR = "Window.Label.SheetEditor";
		private static readonly LanguageCode LABEL_LANGUAGE_EDITOR = "Window.Label.LanguageEditor";
		private static readonly LanguageCode LABEL_HOME_SCREEN = "Window.Label.HomeScreen";

		// Api
		public static WindowUI Instance { get; private set; } = null;
		public static bool MouseOutside { get; set; } = false;
		protected static IRect MainWindowRect { get; private set; } = default;


		#endregion




		#region --- MSG ---


		[OnGameQuitting]
		public static void OnGameQuitting () {
			if (Game.AllowMakerFeaures && Instance != null && Instance.Active) Instance.OnInactivated();
		}


		[OnGameUpdate]
		protected static void DrawPanelTabsUI () {

			if (!Game.AllowMakerFeaures) return;
			if (Player.HasActivePlayer) {
				MainWindowRect = CellRenderer.CameraRect;
				return;
			}

			CellRenderer.SetLayerToUI();

			int tabWidth = Unify(164);
			int tabHeight = Unify(36);
			const int padding = 0;
			MainWindowRect = CellRenderer.CameraRect.EdgeInside(Direction4.Down, CellRenderer.CameraRect.height - tabHeight);
			var barRect = CellRenderer.CameraRect.EdgeInside(Direction4.Up, tabHeight);
			var rect = new IRect(barRect.x, barRect.y, tabWidth, tabHeight);

			// BG
			CellRenderer.Draw(Const.PIXEL, barRect, Const.GREY_12, z: int.MaxValue - 2);

			// Home Editor
			if (DrawTab(rect, HomeScreen.IsActived, LABEL_HOME_SCREEN.Get("Home"), BUTTON_ICON_HOME_SCREEN)) {
				OpenWindow(HomeScreen.TYPE_ID);
			}
			rect.x += tabWidth + padding;

			if (!ProjectSystem.CurrentProject.Readonly) {
				// Map Editor
				if (DrawTab(rect, MapEditor.IsActived, LABEL_MAP_EDITOR.Get("Map"), BUTTON_ICON_MAP_EDITOR)) {
					OpenWindow(MapEditor.TYPE_ID);
				}
				rect.x += tabWidth + padding;

				// Sheet Editor
				if (DrawTab(rect, SheetEditor.IsActived, LABEL_SHEET_EDITOR.Get("Sheet"), BUTTON_ICON_SHEET_EDITOR)) {
					OpenWindow(SheetEditor.TYPE_ID);
				}
				rect.x += tabWidth + padding;

				// Language Editor
				if (DrawTab(rect, LanguageEditor.IsActived, LABEL_LANGUAGE_EDITOR.Get("Language"), BUTTON_ICON_LANGUAGE_EDITOR)) {
					OpenWindow(LanguageEditor.TYPE_ID);
				}
				rect.x += tabWidth + padding;
			}

			// Block Event
			MouseOutside = MouseOutside || barRect.MouseInside();
			if (FrameInput.MouseLeftButton && MouseOutside) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}

			CellRenderer.SetLayerToDefault();
		}


		[OnGameUpdatePauseless]
		public static void OnGameUpdatePauseless () => MouseOutside = false;


		public override void OnActivated () {
			base.OnActivated();
			if (Instance != null && Instance != this) Instance.Active = false;
			Instance = this;
		}


		#endregion




		#region --- API ---


		public static void OpenWindow (int typeID) {
			if (!Game.AllowMakerFeaures) return;
			Stage.ClearStagedEntities();
			Stage.SpawnEntity(typeID, 0, 0);
		}


		#endregion




		#region --- LGC ---


		private static bool DrawTab (IRect rect, bool actived, string label, int iconID) {
			if (actived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_96, int.MaxValue - 1);
			}
			if (CellRendererGUI.Button(rect, label, out var bounds, z: int.MaxValue) && !actived) {
				return true;
			}
			CellRenderer.Draw(
				iconID,
				new IRect(bounds.x - rect.height, rect.y, rect.height, rect.height),
				z: int.MaxValue - 1
			);
			return false;
		}


		#endregion




	}
}
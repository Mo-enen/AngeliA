using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Capacity(1, 0)]
	[RequireSpriteFromField]
	[RequireLanguageFromField]
	public abstract class GlobalEditorUI : EntityUI {


		// Const
		private static readonly SpriteCode BUTTON_ICON_MAP_EDITOR = "GlobalEditor.MapEditor";
		private static readonly SpriteCode BUTTON_ICON_SHEET_EDITOR = "GlobalEditor.SheetEditor";
		private static readonly SpriteCode BUTTON_ICON_LANGUAGE_EDITOR = "GlobalEditor.LanguageEditor";
		private static readonly LanguageCode LABEL_MAP_EDITOR = "GlobalEditor.Label.MapEditor";
		private static readonly LanguageCode LABEL_SHEET_EDITOR = "GlobalEditor.Label.SheetEditor";
		private static readonly LanguageCode LABEL_LANGUAGE_EDITOR = "GlobalEditor.Label.LanguageEditor";

		// Api
		public static GlobalEditorUI Instance { get; private set; } = null;
		public static bool HasActiveInstance => Instance != null && Instance.Active;
		protected static bool MouseInToolbar { get; private set; } = false;
		protected static IRect MainWindowRect { get; private set; } = default;


		[OnGameQuitting]
		public static void OnGameQuitting () {
			if (Game.AllowMakerFeaures && HasActiveInstance) Instance.OnInactivated();
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
			var panelRect = CellRenderer.CameraRect.EdgeInside(Direction4.Up, tabHeight);
			var rect = new IRect(panelRect.x, panelRect.y, tabWidth, tabHeight);

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.GREY_12, z: int.MaxValue - 2);

			// Map Editor
			if (MapEditor.IsActived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_128, int.MaxValue - 1);
			}
			if (CellRendererGUI.Button(
				rect, LABEL_MAP_EDITOR.Get("map"), out var bounds, z: int.MaxValue
			) && !MapEditor.IsActived) {
				OpenEditor(MapEditor.TYPE_ID);
			}
			CellRenderer.Draw(
				BUTTON_ICON_MAP_EDITOR,
				new IRect(bounds.x - tabHeight, rect.y, tabHeight, tabHeight),
				z: int.MaxValue - 1
			);
			rect.x += tabWidth + padding;

			// Sheet Editor
			if (SheetEditor.IsActived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_128, int.MaxValue - 1);
			}
			if (CellRendererGUI.Button(
				rect, LABEL_SHEET_EDITOR.Get("Sheet"), out bounds, z: int.MaxValue
			) && !SheetEditor.IsActived) {
				OpenEditor(SheetEditor.TYPE_ID);
			}
			CellRenderer.Draw(
				BUTTON_ICON_SHEET_EDITOR,
				new IRect(bounds.x - tabHeight, rect.y, tabHeight, tabHeight),
				z: int.MaxValue - 1
			);
			rect.x += tabWidth + padding;

			// Language Editor
			if (LanguageEditor.IsActived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREY_128, int.MaxValue - 1);
			}
			if (CellRendererGUI.Button(
				rect, LABEL_LANGUAGE_EDITOR.Get("Language"), out bounds, z: int.MaxValue
			) && !LanguageEditor.IsActived) {
				OpenEditor(LanguageEditor.TYPE_ID);
			}
			CellRenderer.Draw(
				BUTTON_ICON_LANGUAGE_EDITOR,
				new IRect(bounds.x - tabHeight, rect.y, tabHeight, tabHeight),
				z: int.MaxValue - 1
			);
			rect.x += tabWidth + padding;

			// Block Event
			MouseInToolbar = panelRect.MouseInside();
			if (FrameInput.MouseLeftButton && MouseInToolbar) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}

			CellRenderer.SetLayerToDefault();
		}


		public override void OnActivated () {
			base.OnActivated();
			if (Instance != null && Instance != this) Instance.Active = false;
			Instance = this;
		}


		// API
		public static void OpenEditor (int typeID) {
			if (!Game.AllowMakerFeaures) return;
			Stage.SpawnEntity(typeID, 0, 0);
		}


		public static void OpenEditorSmoothly (int typeID, bool fadeOutAndIn = true) {
			if (!Game.AllowMakerFeaures) return;
			FrameTask.EndAllTask();
			if (fadeOutAndIn) {
				// Fade
				FrameTask.AddToLast(FadeOutTask.TYPE_ID, 50);
				if (FrameTask.AddToLast(SpawnEntityTask.TYPE_ID) is SpawnEntityTask task) {
					task.EntityID = typeID;
					task.X = 0;
					task.Y = 0;
				}
				FrameTask.AddToLast(FadeInTask.TYPE_ID, 50);
			} else {
				// Imme
				Game.PassEffect_RetroDarken(1f);
				if (FrameTask.AddToLast(SpawnEntityTask.TYPE_ID) is SpawnEntityTask task) {
					task.EntityID = typeID;
					task.X = 0;
					task.Y = 0;
				}
			}
		}


	}
}
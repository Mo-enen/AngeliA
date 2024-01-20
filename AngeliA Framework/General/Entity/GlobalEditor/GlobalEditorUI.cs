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
		public static bool ShowingToolbar => (Game.IsEdittime || !Project.OpeningBuiltInProject) && (!HasActiveInstance || Instance.ShowGlobalToolbar);
		protected static bool MouseInToolbar { get; private set; } = false;
		protected virtual bool ShowGlobalToolbar => true;

		// Data
		private static System.Action RestartGameImmediately;
		private static readonly CellContent TooltipLabel = new() {
			Alignment = Alignment.TopRight,
			CharSize = 18,
			Clip = false,
			Wrap = false,
			Tint = Const.GREY_230,
			BackgroundTint = Const.BLACK,
			BackgroundPadding = 6,
		};

		// MSG
		[OnGameInitializeLater]
		internal static void OnGameInitialize () => RestartGameImmediately += () => Game.RestartGame(immediately: true);


		[OnGameQuitting]
		public static void OnGameQuitting () {
			if (HasActiveInstance) Instance.OnInactivated();
		}


		[OnGameUpdate]
		protected static void DrawToolbarUI () {

			if (!ShowingToolbar) return;

			int buttonSize = Unify(36);
			var panelRect = CellRenderer.CameraRect.EdgeInside(Direction4.Up, buttonSize);
			int padding = Unify(6);
			var rect = new IRect(panelRect.xMax, panelRect.y, buttonSize, buttonSize);

			// Map Editor
			rect.x -= buttonSize + padding;
			if (MapEditor.IsActived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue);
			}
			if (CellRendererGUI.Button(
				rect, BUTTON_ICON_MAP_EDITOR, BUTTON_ICON_MAP_EDITOR, BUTTON_ICON_MAP_EDITOR,
				0, 0, 0, z: int.MaxValue
			) && !MapEditor.IsActived) {
				OpenEditorSmoothly(MapEditor.TYPE_ID);
			}
			if (rect.MouseInside()) {
				CellRendererGUI.Label(
					TooltipLabel.SetText(LABEL_MAP_EDITOR.Get("Map Editor")),
					rect.Shift(0, -rect.height - Unify(12))
				);
			}

			// Sheet Editor
			rect.x -= buttonSize + padding;
			if (SheetEditor.IsActived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue);
			}
			if (CellRendererGUI.Button(
				rect, BUTTON_ICON_SHEET_EDITOR, BUTTON_ICON_SHEET_EDITOR, BUTTON_ICON_SHEET_EDITOR,
				0, 0, 0, z: int.MaxValue
			) && !SheetEditor.IsActived) {
				OpenEditorSmoothly(SheetEditor.TYPE_ID);
			}
			if (rect.MouseInside()) {
				CellRendererGUI.Label(
					TooltipLabel.SetText(LABEL_SHEET_EDITOR.Get("Sheet Editor")),
					rect.Shift(0, -rect.height - Unify(12))
				);
			}

			// Language Editor
			rect.x -= buttonSize + padding;
			if (LanguageEditor.IsActived) {
				CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue);
			}
			if (CellRendererGUI.Button(
				rect, BUTTON_ICON_LANGUAGE_EDITOR, BUTTON_ICON_LANGUAGE_EDITOR, BUTTON_ICON_LANGUAGE_EDITOR,
				0, 0, 0, z: int.MaxValue
			) && !LanguageEditor.IsActived) {
				OpenEditorSmoothly(LanguageEditor.TYPE_ID);
			}
			if (rect.MouseInside()) {
				CellRendererGUI.Label(
					TooltipLabel.SetText(LABEL_LANGUAGE_EDITOR.Get("Language Editor")),
					rect.Shift(0, -rect.height - Unify(12))
				);
			}

			// BG
			panelRect.width = panelRect.xMax - rect.x;
			panelRect.x = rect.x;
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.BLACK, z: int.MaxValue - 1);

			// Block Event
			MouseInToolbar = panelRect.MouseInside();
			if (FrameInput.MouseLeftButton && MouseInToolbar) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}
		}


		public override void OnActivated () {
			base.OnActivated();
			if (Instance != null && Instance != this) Instance.Active = false;
			Instance = this;
		}


		// API
		public static void OpenEditorSmoothly (int typeID, bool fadeOutAndIn = true) {
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


		public static void CloseEditorSmoothly () {
			if (Instance == null || !Instance.Active) return;
			FrameTask.EndAllTask();
			FrameTask.AddToLast(FadeOutTask.TYPE_ID, 50);
			FrameTask.AddToLast(DespawnEntityTask.TYPE_ID, Instance);
			FrameTask.AddToLast(MethodTask.TYPE_ID, RestartGameImmediately);
		}


	}
}
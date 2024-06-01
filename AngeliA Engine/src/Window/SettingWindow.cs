using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[RequireSpriteFromField]
[RequireLanguageFromField]
public class SettingWindow : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly SpriteCode UI_PANEL_SETTING = "UI.Panel.Setting";

	private static readonly LanguageCode LABEL_ENGINE = ("Setting.Engine", "Engine");
	private static readonly LanguageCode LABEL_MAP_EDITOR = ("Setting.MapEditorLabel", "Map Editor");
	private static readonly LanguageCode LABEL_PIXEL_EDITOR = ("Setting.PixelEditorLabel", "Artwork");
	private static readonly LanguageCode LABEL_CONSOLE = ("Setting.ConsoleLabel", "Console");
	private static readonly LanguageCode LABEL_PE_BG_COLOR = ("Setting.PE.BgColor", "Background Color");
	private static readonly LanguageCode LABEL_PE_SOLID_PAINTING = ("Setting.PE.SolidPaintingPreview", "Solid Painting Preview");
	private static readonly LanguageCode LABEL_OPEN_LAST_PROJECT_ON_START = ("Setting.OpenLastProjectOnStart", "Open Last Project on Start");
	private static readonly LanguageCode LABEL_MEDT_QUICK_DROP = ("Setting.MEDT.QuickDrop", "Drop Player when Release Space Key");
	private static readonly LanguageCode LABEL_MEDT_SHOW_STATE = ("Setting.MEDT.ShowState", "Show State Info");
	private static readonly LanguageCode LABEL_MEDT_SHOW_BEHIND = ("Setting.MEDT.ShowBehind", "Show Map Behind");
	private static readonly LanguageCode LABEL_MEDT_AUTO_ZOOM = ("Setting.MEDT.AutoZoom", "Auto Zoom when Editing");
	private static readonly LanguageCode LABEL_USE_TOOLTIP = ("Setting.UseTooltip", "Show Tooltip");
	private static readonly LanguageCode LABEL_USE_NOTI = ("Setting.UseNotification", "Show Notification");
	private static readonly LanguageCode LABEL_SHOW_LOG_TIME = ("Setting.ShowLogTime", "Show Log Time");
	private static readonly LanguageCode LABEL_THEME_BUILT_IN = ("Menu.BuiltInTheme", "Built-in");
	private static readonly LanguageCode LABEL_THEME = ("Setting.Theme", "Theme");
	private static readonly LanguageCode LABEL_AUTO_RECOMPILE = ("Setting.AutoRecompile", "Auto Recompile when Script Changed");

	// Api
	public static SettingWindow Instance { get; private set; }
	public string RequireChangeThemePath { get; set; } = null;
	public bool RigSettingChanged { get; set; } = false;
	public override string DefaultName => "Setting";

	// Data
	private readonly List<(string path, string name)> ThemePaths = new();
	private bool RequiringReloadThemePath = true;
	private int MasterScroll = 0;
	private int UIHeight = 0;
	private ColorF PixEditorBackgroundColor;
	private Color32 BackgroundColorDefault;


	#endregion




	#region --- MSG ---


	public SettingWindow (ColorF pixEditorBackgroundColor, Color32 backgroundColorDefault) {
		Instance = this;
		PixEditorBackgroundColor = pixEditorBackgroundColor;
		BackgroundColorDefault = backgroundColorDefault;
	}


	[OnGameFocused]
	internal static void OnGameFocused () => Instance?.RequireReloadThemePath();


	public override void UpdateWindowUI () {
		int extendedUISize = 1;
		using (var scroll = Scope.GUIScroll(WindowRect, MasterScroll, 0, UIHeight)) {
			MasterScroll = scroll.ScrollPosition;

			var panelRect = WindowRect.Shrink(Unify(12), Unify(12), Unify(42), Unify(42));
			int maxPanelWidth = Unify(612);
			if (panelRect.width > maxPanelWidth) {
				panelRect.x += (panelRect.width - maxPanelWidth) / 2;
				panelRect.width = maxPanelWidth;
			}

			var rect = panelRect.EdgeInside(Direction4.Up, GUI.FieldHeight);

			using var _ = Scope.GUILabelWidth(Util.Min(Unify(256), rect.width / 2));

			DrawPanel(ref rect, Update_Engine);
			DrawPanel(ref rect, Update_MapEditor);
			DrawPanel(ref rect, Update_PixelEditor);
			DrawPanel(ref rect, Update_Console);

			extendedUISize = WindowRect.yMax - rect.yMax + Unify(128);
			UIHeight = (extendedUISize - WindowRect.height).GreaterOrEquelThanZero();
		}
		MasterScroll = GUI.ScrollBar(
			92645,
			WindowRect.EdgeInside(Direction4.Right, Unify(12)),
			MasterScroll,
			extendedUISize,
			WindowRect.height
		);
	}


	private IRect Update_Engine (IRect rect) {

		int itemPadding = GUI.FieldPadding;

		// Label - Engine
		GUI.Label(rect.Shift(-Unify(32), 0), LABEL_ENGINE, Skin.SmallGreyLabel);
		rect.SlideDown(itemPadding);

		// Open Last Project on Start
		EngineSetting.OpenLastProjectOnStart.Value = GUI.Toggle(
			rect, EngineSetting.OpenLastProjectOnStart.Value, LABEL_OPEN_LAST_PROJECT_ON_START,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Use Tooltip
		EngineSetting.UseTooltip.Value = GUI.Toggle(
			rect, EngineSetting.UseTooltip.Value, LABEL_USE_TOOLTIP,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Use Notification
		EngineSetting.UseNotification.Value = GUI.Toggle(
			rect, EngineSetting.UseNotification.Value, LABEL_USE_NOTI,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Auto Recompile
		EngineSetting.AutoRecompile.Value = GUI.Toggle(
			rect, EngineSetting.AutoRecompile.Value, LABEL_AUTO_RECOMPILE,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Theme
		GUI.SmallLabel(rect, LABEL_THEME);
		var popRect = rect.ShrinkLeft(GUI.LabelWidth).LeftHalf();
		if (GUI.Button(popRect, Skin.Name, Skin.SmallDarkButton)) {
			ShowThemeMenu(popRect);
		}
		GUI.PopupTriangleIcon(popRect);
		rect.SlideDown(itemPadding);

		return rect;
	}


	private IRect Update_MapEditor (IRect rect) {

		int itemPadding = GUI.FieldPadding;

		// Label - MapEditor
		GUI.Label(rect.Shift(-Unify(32), 0), LABEL_MAP_EDITOR, Skin.SmallGreyLabel);
		rect.SlideDown(itemPadding);

		GUI.BeginChangeCheck();

		// Quick Drop
		EngineSetting.MapEditor_QuickPlayerDrop.Value = GUI.Toggle(
			rect, EngineSetting.MapEditor_QuickPlayerDrop.Value, LABEL_MEDT_QUICK_DROP,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Auto Zoom
		EngineSetting.MapEditor_AutoZoom.Value = GUI.Toggle(
			rect, EngineSetting.MapEditor_AutoZoom.Value, LABEL_MEDT_AUTO_ZOOM,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Show Behind
		EngineSetting.MapEditor_ShowBehind.Value = GUI.Toggle(
			rect, EngineSetting.MapEditor_ShowBehind.Value, LABEL_MEDT_SHOW_BEHIND,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Show State
		EngineSetting.MapEditor_ShowState.Value = GUI.Toggle(
			rect, EngineSetting.MapEditor_ShowState.Value, LABEL_MEDT_SHOW_STATE,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Final
		RigSettingChanged = RigSettingChanged || GUI.EndChangeCheck();

		return rect;

	}


	private IRect Update_PixelEditor (IRect rect) {

		int itemPadding = GUI.FieldPadding;

		// Label - PixEditor
		GUI.Label(rect.Shift(-Unify(32), 0), LABEL_PIXEL_EDITOR, Skin.SmallGreyLabel);
		rect.SlideDown(itemPadding);

		// Background Color
		PixEditorBackgroundColor = GUI.HorizontalColorField(
			PixEditorBackgroundColor,
			rect,
			label: LABEL_PE_BG_COLOR,
			labelStyle: Skin.SmallLabel,
			defaultColor: BackgroundColorDefault.ToColorF()
		);
		EngineSetting.BackgroundColor.Value = PixEditorBackgroundColor.ToColor32();
		rect.SlideDown(itemPadding);

		// Solid Painting Preview
		EngineSetting.SolidPaintingPreview.Value = GUI.Toggle(
			rect, EngineSetting.SolidPaintingPreview.Value, LABEL_PE_SOLID_PAINTING,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		return rect;

	}


	private IRect Update_Console (IRect rect) {

		int itemPadding = GUI.FieldPadding;

		// Label - Console
		GUI.Label(rect.Shift(-Unify(32), 0), LABEL_CONSOLE, Skin.SmallGreyLabel);
		rect.SlideDown(itemPadding);

		// Show Log Time
		EngineSetting.ShowLogTime.Value = GUI.Toggle(
			rect, EngineSetting.ShowLogTime.Value, LABEL_SHOW_LOG_TIME,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);
		return rect;

	}


	#endregion




	#region --- API ---


	public void RequireReloadThemePath () => RequiringReloadThemePath = true;


	#endregion




	#region --- LGC ---


	private void DrawPanel (ref IRect rect, System.Func<IRect, IRect> panelGUI) {

		int labelOffset = Unify(32);
		var boxPadding = Int4.Direction(Unify(20), Unify(20), Unify(6), Unify(12));
		int boxTop = rect.yMax;
		int boxLeft = rect.xMin;
		int boxRight = rect.xMax;

		rect = panelGUI(rect);

		if (
			Renderer.TryGetSprite(UI_PANEL_SETTING, out var sprite) ||
			Renderer.TryGetSprite(Const.PIXEL, out sprite)
		) {
			using (Scope.RendererLayer(RenderLayer.DEFAULT)) {
				var tint = sprite.ID == Const.PIXEL ? Color32.WHITE_12 : Color32.WHITE;
				Renderer.DrawSlice(sprite, new IRect(
					boxLeft - boxPadding.left - labelOffset,
					rect.yMax - boxPadding.down + MasterScroll,
					boxRight - boxLeft + boxPadding.horizontal + labelOffset,
					boxTop - rect.yMax + boxPadding.vertical
				), tint);
			}
		}
		rect.y -= boxPadding.vertical + Unify(6);
	}


	private void ShowThemeMenu (IRect rect) {

		// Reload
		if (RequiringReloadThemePath) {
			RequiringReloadThemePath = false;
			ThemePaths.Clear();
			string themeFolder = Util.CombinePaths(UniverseSystem.BuiltInUniverse.UniverseRoot, "Theme");
			if (!Util.FolderExists(themeFolder)) return;
			foreach (var path in Util.EnumerateFiles(themeFolder, true, $"*.{AngePath.SHEET_FILE_EXT}")) {
				ThemePaths.Add((path, Util.GetDisplayName(Util.GetNameWithoutExtension(path))));
			}
		}

		// Show Menu
		GenericPopupUI.BeginPopup(new Int2(rect.x + Unify(4), rect.y));
		GenericPopupUI.AddItem(LABEL_THEME_BUILT_IN, MenuInvoked, @checked: GUI.Skin.Name == "Built-in");
		int index = 0;
		foreach (var (path, name) in ThemePaths) {
			GenericPopupUI.AddItem(name, MenuInvoked, @checked: GUI.Skin.Name == name, data: index);
			index++;
		}

		// Func
		static void MenuInvoked () {
			if (GenericPopupUI.Instance.InvokingItemData is not int index) {
				// Built In
				Instance.RequireChangeThemePath = "";
			} else if (Instance.ThemePaths.Count > 0 && index >= 0) {
				// Custom
				Instance.RequireChangeThemePath = Instance.ThemePaths[index.Clamp(0, Instance.ThemePaths.Count - 1)].path;
			}
		}
	}


	#endregion




}
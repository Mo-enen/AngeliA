using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[RequireLanguageFromField]
public class SettingWindow : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly LanguageCode LABEL_ENGINE = ("Setting.Engine", "Engine");
	private static readonly LanguageCode LABEL_PIXEL_EDITOR = ("Setting.PixelEditorLabel", "Artwork");
	private static readonly LanguageCode LABEL_CONSOLE = ("Setting.ConsoleLabel", "Console");
	private static readonly LanguageCode LABEL_PE_BG_COLOR = ("Setting.PE.BgColor", "Background Color");
	private static readonly LanguageCode LABEL_PE_SOLID_PAINTING = ("Setting.PE.SolidPaintingPreview", "Solid Painting Preview");
	private static readonly LanguageCode LABEL_OPEN_LAST_PROJECT_ON_START = ("Setting.OpenLastProjectOnStart", "Open Last Project on Start");
	private static readonly LanguageCode LABEL_ONLY_SPRITE_ON_OPTION = ("Setting.ASAOOHOK", "Only Modify Spirte on Holding Ctrl");
	private static readonly LanguageCode LABEL_USE_TOOLTIP = ("Setting.UseTooltip", "Show Tooltip");
	private static readonly LanguageCode LABEL_USE_NOTI = ("Setting.UseNotification", "Show Notification");
	private static readonly LanguageCode LABEL_SHOW_LOG_TIME = ("Setting.ShowLogTime", "Show Log Time");
	private static readonly LanguageCode LABEL_THEME_BUILT_IN = ("Menu.BuiltInTheme", "Built-in");
	private static readonly LanguageCode LABEL_THEME = ("Setting.Theme", "Theme");

	// Api
	public string RequireChangeThemePath { get; set; } = null;
	public override string DefaultName => "Setting";
	public bool Changed { get; private set; } = false;
	public bool OpenLastProjectOnStart { get; set; }
	public bool UseTooltip { get; set; }
	public bool UseNotification { get; set; }
	public bool SolidPaintingPreview { get; set; }
	public bool AllowSpirteActionOnlyOnHoldingOptionKey { get; set; }
	public bool ShowLogTime { get; set; }
	public Color32 BackgroundColor { get; set; }

	// Data
	private static SettingWindow Instance;
	private readonly List<(string path, string name)> ThemePaths = new();
	private bool RequiringReloadThemePath = true;
	private int MasterScroll = 0;
	private int UIHeight = 0;
	private ColorF PixEditorBackgroundColor;
	private Color32 BackgroundColorDefault;


	#endregion




	#region --- MSG ---


	public SettingWindow () => Instance = this;


	[OnGameFocused]
	internal static void OnGameFocused () => Instance?.RequireReloadThemePath();


	public override void UpdateWindowUI () {
		int itemHeight = Unify(32);
		int extendedUISize = 1;
		using (var scroll = Scope.GUIScroll(WindowRect, MasterScroll, 0, UIHeight)) {
			MasterScroll = scroll.ScrollPosition;
			var rect = WindowRect.Shrink(
				Unify(96), Unify(96), Unify(42), Unify(42)
			).EdgeInside(Direction4.Up, itemHeight);

			using var _ = Scope.GUILabelWidth(Util.Min(Unify(384), rect.width / 2));
			GUI.BeginChangeCheck();
			DrawPanel(ref rect, Update_Engine);
			DrawPanel(ref rect, Update_PixelEditor);
			DrawPanel(ref rect, Update_Console);
			Changed = GUI.EndChangeCheck();

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

		int itemPadding = Unify(4);

		// Label - Engine
		GUI.Label(rect.Shift(-Unify(32), 0), LABEL_ENGINE, Skin.SmallGreyLabel);
		rect.SlideDown(itemPadding);

		// Open Last Project on Start
		OpenLastProjectOnStart = GUI.Toggle(
			rect, OpenLastProjectOnStart, LABEL_OPEN_LAST_PROJECT_ON_START,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Use Tooltip
		UseTooltip = GUI.Toggle(
			rect, UseTooltip, LABEL_USE_TOOLTIP,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Use Notification
		UseNotification = GUI.Toggle(
			rect, UseNotification, LABEL_USE_NOTI,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Theme
		GUI.SmallLabel(rect, LABEL_THEME);
		var popRect = rect.ShrinkLeft(GUI.LabelWidth).LeftHalf();
		if (GUI.Button(popRect, Skin.Name, Skin.SmallDarkDropdown)) {
			ShowThemeMenu(popRect);
		}
		rect.SlideDown(itemPadding);

		return rect;
	}


	private IRect Update_PixelEditor (IRect rect) {

		int itemPadding = Unify(4);

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
		BackgroundColor = PixEditorBackgroundColor.ToColor32();
		rect.SlideDown(itemPadding);

		// Solid Painting Preview
		SolidPaintingPreview = GUI.Toggle(
			rect, SolidPaintingPreview, LABEL_PE_SOLID_PAINTING,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Allow Spirte Action Only On Holding Option Key
		AllowSpirteActionOnlyOnHoldingOptionKey = GUI.Toggle(
			rect, AllowSpirteActionOnlyOnHoldingOptionKey, LABEL_ONLY_SPRITE_ON_OPTION,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);
		return rect;

	}


	private IRect Update_Console (IRect rect) {

		int itemPadding = Unify(4);

		// Label - Console
		GUI.Label(rect.Shift(-Unify(32), 0), LABEL_CONSOLE, Skin.SmallGreyLabel);
		rect.SlideDown(itemPadding);

		// Show Log Time
		ShowLogTime = GUI.Toggle(
			rect, ShowLogTime, LABEL_SHOW_LOG_TIME,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);
		return rect;

	}


	#endregion




	#region --- API ---


	public void Initialize (ColorF pixEditorBackgroundColor, Color32 backgroundColorDefault) {
		PixEditorBackgroundColor = pixEditorBackgroundColor;
		BackgroundColorDefault = backgroundColorDefault;
	}


	public void RequireReloadThemePath () => RequiringReloadThemePath = true;


	#endregion




	#region --- LGC ---


	private void DrawPanel (ref IRect rect, System.Func<IRect, IRect> panelGUI) {
		int labelOffset = Unify(32);
		int boxPadding = Unify(8);
		var box = Renderer.DrawPixel(default, Color32.WHITE_12);
		int boxTop = rect.yMax;
		int boxLeft = rect.xMin;
		int boxRight = rect.xMax;
		rect = panelGUI(rect);
		box.X = boxLeft - boxPadding - labelOffset;
		box.Y = rect.yMax - boxPadding;
		box.Width = boxRight - boxLeft + boxPadding * 2 + labelOffset;
		box.Height = boxTop - rect.yMax + boxPadding * 2;
		rect.y -= Unify(24);
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
		foreach (var (path, name) in ThemePaths) {
			GenericPopupUI.AddItem(name, MenuInvoked, @checked: GUI.Skin.Name == name);
		}

		// Func
		static void MenuInvoked () {
			int index = GenericPopupUI.Instance.InvokingItemIndex;
			if (index <= 0) {
				// Built In
				Instance.RequireChangeThemePath = "";
			} else if (Instance.ThemePaths.Count > 0) {
				// Custom
				Instance.RequireChangeThemePath = Instance.ThemePaths[(index - 1).Clamp(0, Instance.ThemePaths.Count - 1)].path;
			}
		}
	}


	#endregion




}
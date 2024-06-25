using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;



public class SettingWindow : WindowUI {




	#region --- VAR ---


	// Const
	private static readonly SpriteCode UI_PANEL_SETTING = "UI.Panel.Setting";
	private static readonly SpriteCode ICON_ENGINE = "Icon.SettingFold.Engine";
	private static readonly SpriteCode ICON_MAP_EDITOR = "Icon.SettingFold.MapEditor";
	private static readonly SpriteCode ICON_PIXEL_EDITOR = "Icon.SettingFold.PixelEditor";
	private static readonly SpriteCode ICON_CHAR_ANI_EDITOR = "Icon.SettingFold.CharAnimation";
	private static readonly SpriteCode ICON_CONSOLE = "Icon.SettingFold.Console";
	private static readonly SpriteCode ICON_HOTKEY = "Icon.SettingFold.Hotkey";

	private static readonly LanguageCode LABEL_ENGINE = ("Setting.Engine", "Engine");
	private static readonly LanguageCode LABEL_MAP_EDITOR = ("Setting.MapEditorLabel", "Map Editor");
	private static readonly LanguageCode LABEL_PIXEL_EDITOR = ("Setting.PixelEditorLabel", "Artwork");
	private static readonly LanguageCode LABEL_CHAR_ANI_EDITOR = ("Setting.CharAniEditorLabel", "Animation");
	private static readonly LanguageCode LABEL_CONSOLE = ("Setting.ConsoleLabel", "Console");
	private static readonly LanguageCode LABEL_HOTKEY = ("Setting.HotkeyLabel", "Hotkey");

	private static readonly LanguageCode LABEL_OPEN_LAST_PROJECT_ON_START = ("Setting.OpenLastProjectOnStart", "Open Last Project on Start");
	private static readonly LanguageCode LABEL_USE_TOOLTIP = ("Setting.UseTooltip", "Show Tooltip");
	private static readonly LanguageCode LABEL_USE_NOTI = ("Setting.UseNotification", "Show Notification");
	private static readonly LanguageCode LABEL_THEME = ("Setting.Theme", "Theme");
	private static readonly LanguageCode LABEL_AUTO_RECOMPILE = ("Setting.AutoRecompile", "Auto Recompile when Script Changed");
	private static readonly LanguageCode LABEL_CLEAR_CHAR_CONFIG = ("Setting.ClearCharConfig", "Clear Character Config Before Game Start");

	private static readonly LanguageCode LABEL_MEDT_ENABLE = ("Setting.MEDT.Enable", "Use Map Editor in Engine");
	private static readonly LanguageCode LABEL_MEDT_QUICK_DROP = ("Setting.MEDT.QuickDrop", "Drop Player when Release Space Key");
	private static readonly LanguageCode LABEL_MEDT_SHOW_STATE = ("Setting.MEDT.ShowState", "Show State Info on Bottom-Right");
	private static readonly LanguageCode LABEL_MEDT_SHOW_BEHIND = ("Setting.MEDT.ShowBehind", "Show Map Behind");
	private static readonly LanguageCode LABEL_MEDT_AUTO_ZOOM = ("Setting.MEDT.AutoZoom", "Auto Zoom when Editing");

	private static readonly LanguageCode LABEL_PE_BG_COLOR = ("Setting.PE.BgColor", "Background Color");
	private static readonly LanguageCode LABEL_PE_GRADIENT_BG = ("Setting.PE.GradientBG", "Gradient Background");
	private static readonly LanguageCode LABEL_PE_SOLID_PAINTING = ("Setting.PE.SolidPaintingPreview", "Solid Painting Preview");

	private static readonly LanguageCode LABEL_SHOW_LOG_TIME = ("Setting.ShowLogTime", "Show Log Time");
	private static readonly LanguageCode LABEL_BLINK_ERROR = ("Setting.BlinkWhenError", "Blink When Having Compile Error");

	private static readonly LanguageCode LABEL_CHAR_ANI_REVERSE_SCROLL = ("Setting.CharAni.ReverseScroll", "Reverse Mouse Scroll for Timeline");
	private static readonly LanguageCode LABEL_CHAR_ANI_VERTICAL_SCROLL = ("Setting.CharAni.VerticalScroll", "Mouse Scroll Vertically for Timeline");
	private static readonly LanguageCode LABEL_CHAR_ANI_DRAG_HORI_ONLY = ("Setting.CharAni.MidDragHoriOnly", "Mid Drag Only Move Timeline Horizontally");

	private static readonly LanguageCode LABEL_HOTKEY_RECOMPILE = ("Setting.Hotkey.Recompile", "Recompile");
	private static readonly LanguageCode LABEL_HOTKEY_RUN = ("Setting.Hotkey.Run", "Run");
	private static readonly LanguageCode LABEL_HOTKEY_CLEAR_CONSOLE = ("Setting.Hotkey.ClearConsole", "Clear Console");
	private static readonly LanguageCode LABEL_HOTKEY_MEDT = ("Setting.Hotkey.MEDT", "Open Map Editor");
	private static readonly LanguageCode LABEL_HOTKEY_ART = ("Setting.Hotkey.Artwork", "Open Artwork");
	private static readonly LanguageCode LABEL_HOTKEY_ANI = ("Setting.Hotkey.CharAni", "Open Character Animation");
	private static readonly LanguageCode LABEL_HOTKEY_LANGUAGE = ("Setting.Hotkey.Language", "Open Language Editor");
	private static readonly LanguageCode LABEL_HOTKEY_CONSOLE = ("Setting.Hotkey.Console", "Open Console");
	private static readonly LanguageCode LABEL_HOTKEY_PROJECT = ("Setting.Hotkey.Project", "Open Project Editor");
	private static readonly LanguageCode LABEL_HOTKEY_SETTING = ("Setting.Hotkey.Setting", "Open Setting");
	private static readonly LanguageCode LABEL_HOTKEY_PIX_TOOL_RECT = ("Setting.Hotkey.Pix.Rect", "Artwork - Rect Tool");
	private static readonly LanguageCode LABEL_HOTKEY_PIX_TOOL_LINE = ("Setting.Hotkey.Pix.Line", "Artwork - Line Tool");
	private static readonly LanguageCode LABEL_HOTKEY_PIX_TOOL_BUCKET = ("Setting.Hotkey.Pix.Bucket", "Artwork - Bucket Tool");
	private static readonly LanguageCode LABEL_HOTKEY_PIX_TOOL_SELECT = ("Setting.Hotkey.Pix.Select", "Artwork - Select Tool");
	private static readonly LanguageCode LABEL_HOTKEY_PIX_TOOL_SPRITE = ("Setting.Hotkey.Pix.Sprite", "Artwork - Sprite Tool");
	private static readonly LanguageCode LABEL_HOTKEY_PIX_PAL_PREV = ("Setting.Hotkey.Pix.PalPrev", "Artwork - Prev Palette Color");
	private static readonly LanguageCode LABEL_HOTKEY_PIX_PAL_NEXT = ("Setting.Hotkey.Pix.PalNext", "Artwork - Next Palette Color");
	private static readonly LanguageCode LABEL_HOTKEY_FD_NEXT = ("Setting.Hotkey.FrameDebug.Next", "Frame Debug - Next Frame");

	private static readonly LanguageCode LABEL_THEME_BUILT_IN = ("Menu.BuiltInTheme", "Built-in");
	private static readonly LanguageCode MENU_CATA_LETTER = ("Menu.Group.Letter", "Letter");
	private static readonly LanguageCode MENU_CATA_NUMBER = ("Menu.Group.Number", "Number");
	private static readonly LanguageCode MENU_CATA_SIGN = ("Menu.Group.Sign", "Sign");
	private static readonly LanguageCode MENU_CATA_OTHER = ("Menu.Group.Other", "Fn");

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
	private bool PanelFolding_Engine = false;
	private bool PanelFolding_MapEditor = true;
	private bool PanelFolding_PixelEditor = true;
	private bool PanelFolding_CharAniEditor = true;
	private bool PanelFolding_Console = true;
	private bool PanelFolding_Hotkey = true;
	private SavingHotkey ActivatedSetting = null;


	#endregion




	#region --- MSG ---


	public SettingWindow (ColorF pixEditorBackgroundColor, Color32 backgroundColorDefault) {
		Instance = this;
		PixEditorBackgroundColor = pixEditorBackgroundColor;
		BackgroundColorDefault = backgroundColorDefault;
	}


	[OnGameFocused]
	internal static void OnGameFocused () {
		if (Instance != null) {
			Instance.RequiringReloadThemePath = true;
		}
	}


	public override void UpdateWindowUI () {
		int extendedUISize = 1;
		using (var scroll = new GUIVerticalScrollScope(WindowRect, MasterScroll, 0, UIHeight)) {
			MasterScroll = scroll.PositionY;

			var panelRect = WindowRect.Shrink(Unify(12), Unify(12), Unify(42), Unify(42));
			int maxPanelWidth = Unify(612);
			if (panelRect.width > maxPanelWidth) {
				panelRect.x += (panelRect.width - maxPanelWidth) / 2;
				panelRect.width = maxPanelWidth;
			}

			var rect = panelRect.Edge(Direction4.Up, GUI.FieldHeight);

			using var _ = new GUILabelWidthScope(Util.Min(Unify(256), rect.width / 2));

			DrawPanel(ref rect, LABEL_ENGINE, ICON_ENGINE, Update_Engine, ref PanelFolding_Engine);
			DrawPanel(ref rect, LABEL_MAP_EDITOR, ICON_MAP_EDITOR, Update_MapEditor, ref PanelFolding_MapEditor);
			DrawPanel(ref rect, LABEL_PIXEL_EDITOR, ICON_PIXEL_EDITOR, Update_PixelEditor, ref PanelFolding_PixelEditor);
			DrawPanel(ref rect, LABEL_CHAR_ANI_EDITOR, ICON_CHAR_ANI_EDITOR, Update_CharAniEditor, ref PanelFolding_CharAniEditor);
			DrawPanel(ref rect, LABEL_CONSOLE, ICON_CONSOLE, Update_Console, ref PanelFolding_Console);
			DrawPanel(ref rect, LABEL_HOTKEY, ICON_HOTKEY, Update_Hotkey, ref PanelFolding_Hotkey);

			extendedUISize = WindowRect.yMax - rect.yMax + Unify(128);
			UIHeight = (extendedUISize - WindowRect.height).GreaterOrEquelThanZero();
		}
		MasterScroll = GUI.ScrollBar(
			92645,
			WindowRect.Edge(Direction4.Right, GUI.ScrollbarSize),
			MasterScroll,
			extendedUISize,
			WindowRect.height
		);
	}


	private IRect Update_Engine (IRect rect) {

		int itemPadding = GUI.FieldPadding;

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

		// Auto Recompile
		EngineSetting.ClearCharacterConfigBeforeGameStart.Value = GUI.Toggle(
			rect, EngineSetting.ClearCharacterConfigBeforeGameStart.Value, LABEL_CLEAR_CHAR_CONFIG,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Theme
		GUI.SmallLabel(rect, LABEL_THEME);
		var popRect = rect.ShrinkLeft(GUI.LabelWidth).LeftHalf();
		if (GUI.Button(popRect, Skin.Name, Skin.SmallDarkButton)) {
			ShowThemeMenu(popRect.Shift(Unify(4), MasterScroll).BottomLeft());
		}
		GUI.PopupTriangleIcon(popRect.Shrink(rect.height / 8));
		rect.SlideDown(itemPadding);

		return rect;
	}


	private IRect Update_MapEditor (IRect rect) {

		int itemPadding = GUI.FieldPadding;

		GUI.BeginChangeCheck();

		// Enable
		EngineSetting.MapEditor_Enable.Value = GUI.Toggle(
			rect, EngineSetting.MapEditor_Enable.Value, LABEL_MEDT_ENABLE,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		if (EngineSetting.MapEditor_Enable.Value) {

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
		}

		// Final
		RigSettingChanged = RigSettingChanged || GUI.EndChangeCheck();

		return rect;

	}


	private IRect Update_PixelEditor (IRect rect) {

		int itemPadding = GUI.FieldPadding;

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

		// Gradient Background
		EngineSetting.GradientBackground.Value = GUI.Toggle(
			rect, EngineSetting.GradientBackground.Value, LABEL_PE_GRADIENT_BG,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Solid Painting Preview
		EngineSetting.SolidPaintingPreview.Value = GUI.Toggle(
			rect, EngineSetting.SolidPaintingPreview.Value, LABEL_PE_SOLID_PAINTING,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);


		return rect;

	}


	private IRect Update_CharAniEditor (IRect rect) {

		int itemPadding = GUI.FieldPadding;

		// Reverse Mouse Scroll for Timeline
		EngineSetting.ReverseMouseScrollForTimeline.Value = GUI.Toggle(
			rect, EngineSetting.ReverseMouseScrollForTimeline.Value, LABEL_CHAR_ANI_REVERSE_SCROLL,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Mouse Scroll Vertical for Timeline
		EngineSetting.MouseScrollVerticalForTimeline.Value = GUI.Toggle(
			rect, EngineSetting.MouseScrollVerticalForTimeline.Value, LABEL_CHAR_ANI_VERTICAL_SCROLL,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Mid Drag Horizontal Only for Timeline
		EngineSetting.MidDragHorizontalOnlyForTimeline.Value = GUI.Toggle(
			rect, EngineSetting.MidDragHorizontalOnlyForTimeline.Value, LABEL_CHAR_ANI_DRAG_HORI_ONLY,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		return rect;

	}


	private IRect Update_Console (IRect rect) {

		int itemPadding = GUI.FieldPadding;

		// Show Log Time
		EngineSetting.ShowLogTime.Value = GUI.Toggle(
			rect, EngineSetting.ShowLogTime.Value, LABEL_SHOW_LOG_TIME,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		// Blink when Error
		EngineSetting.BlinkWhenError.Value = GUI.Toggle(
			rect, EngineSetting.BlinkWhenError.Value, LABEL_BLINK_ERROR,
			labelStyle: Skin.SmallLabel
		);
		rect.SlideDown(itemPadding);

		return rect;

	}


	private IRect Update_Hotkey (IRect rect) {

		int itemPadding = GUI.FieldPadding;

		HotkeyField(rect, EngineSetting.Hotkey_Recompile, LABEL_HOTKEY_RECOMPILE);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_Run, LABEL_HOTKEY_RUN);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_ClearConsole, LABEL_HOTKEY_CLEAR_CONSOLE);
		rect.SlideDown(itemPadding);

		// Window
		HotkeyField(rect, EngineSetting.Hotkey_Window_MapEditor, LABEL_HOTKEY_MEDT);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_Window_Artwork, LABEL_HOTKEY_ART);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_Window_CharAni, LABEL_HOTKEY_ANI);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_Window_Language, LABEL_HOTKEY_LANGUAGE);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_Window_Console, LABEL_HOTKEY_CONSOLE);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_Window_Project, LABEL_HOTKEY_PROJECT);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_Window_Setting, LABEL_HOTKEY_SETTING);
		rect.SlideDown(itemPadding);

		// Pix Tool
		HotkeyField(rect, EngineSetting.Hotkey_PixTool_Rect, LABEL_HOTKEY_PIX_TOOL_RECT);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_PixTool_Line, LABEL_HOTKEY_PIX_TOOL_LINE);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_PixTool_Bucket, LABEL_HOTKEY_PIX_TOOL_BUCKET);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_PixTool_Select, LABEL_HOTKEY_PIX_TOOL_SELECT);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_PixTool_Sprite, LABEL_HOTKEY_PIX_TOOL_SPRITE);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_Pix_PalettePrev, LABEL_HOTKEY_PIX_PAL_PREV);
		rect.SlideDown(itemPadding);

		HotkeyField(rect, EngineSetting.Hotkey_Pix_PaletteNext, LABEL_HOTKEY_PIX_PAL_NEXT);
		rect.SlideDown(itemPadding);

		// FDebug
		HotkeyField(rect, EngineSetting.Hotkey_FrameDebug_Next, LABEL_HOTKEY_FD_NEXT);
		rect.SlideDown(itemPadding);

		return rect;
	}


	#endregion




	#region --- LGC ---


	private void DrawPanel (ref IRect rect, string label, int icon, System.Func<IRect, IRect> panelGUI, ref bool folding) {

		var boxPadding = Int4.Direction(Unify(24), Unify(4), Unify(3), Unify(3));
		int boxTop = rect.yMax;
		int boxLeft = rect.xMin;
		int boxRight = rect.xMax;

		// Fold Icon
		GUI.Icon(rect.Edge(Direction4.Left, rect.height * 3 / 4).Shift(-boxPadding.left / 5, 0), icon);

		// Fold Label
		if (GUI.Button(rect.Expand(boxPadding.left, boxPadding.right, 0, 0), 0, Skin.WeakHighlightPixel)) folding = !folding;
		GUI.Label(rect.ShrinkLeft(rect.height), label, Skin.SmallGreyLabel);

		// Fold Triangle
		using (new GUIColorScope(Color32.GREY_128)) {
			GUI.Icon(
				rect.EdgeOutside(Direction4.Left, rect.height * 2 / 3).Shift(-boxPadding.left / 4, 0),
				folding ? BuiltInSprite.ICON_TRIANGLE_RIGHT : BuiltInSprite.ICON_TRIANGLE_DOWN
			);
		}
		rect.SlideDown(folding ? 0 : GUI.FieldPadding);

		if (!folding) {
			rect = panelGUI(rect);
		}

		if (
			Renderer.TryGetSprite(UI_PANEL_SETTING, out var sprite) ||
			Renderer.TryGetSprite(Const.PIXEL, out sprite)
		) {
			using (new DefaultLayerScope()) {
				var tint = sprite.ID == Const.PIXEL ? new Color32(23, 23, 23, 255) : Color32.WHITE;
				Renderer.DrawSlice(sprite, new IRect(
					boxLeft - boxPadding.left,
					rect.yMax - boxPadding.down + MasterScroll,
					boxRight - boxLeft + boxPadding.horizontal,
					boxTop - rect.yMax + boxPadding.vertical
				), tint);
			}
		}
		rect.y -= boxPadding.vertical + Unify(6);
	}


	private void ShowThemeMenu (Int2 pos) {

		// Reload
		if (RequiringReloadThemePath) {
			RequiringReloadThemePath = false;
			ThemePaths.Clear();
			string themeFolder = Util.CombinePaths(Universe.BuiltIn.UniverseRoot, "Theme");
			if (!Util.FolderExists(themeFolder)) return;
			foreach (var path in Util.EnumerateFiles(themeFolder, true, $"*.{AngePath.SHEET_FILE_EXT}")) {
				ThemePaths.Add((path, Util.GetDisplayName(Util.GetNameWithoutExtension(path))));
			}
		}

		// Show Menu
		GenericPopupUI.BeginPopup(pos);
		GenericPopupUI.AddItem(LABEL_THEME_BUILT_IN, MenuInvoked, @checked: GUI.Skin.Name == "Built-in");
		int index = 0;
		foreach (var (path, name) in ThemePaths) {
			GenericPopupUI.AddItem(name, MenuInvoked, @checked: GUI.Skin.Name == name, data: index);
			index++;
		}

		// Func
		static void MenuInvoked () {
			if (GenericPopupUI.InvokingItemData is not int index) {
				// Built In
				Instance.RequireChangeThemePath = "";
			} else if (Instance.ThemePaths.Count > 0 && index >= 0) {
				// Custom
				Instance.RequireChangeThemePath = Instance.ThemePaths[index.Clamp(0, Instance.ThemePaths.Count - 1)].path;
			}
		}
	}


	private void HotkeyField (IRect rect, SavingHotkey saving, string label) {

		int padding = Unify(4);

		// Label
		GUI.SmallLabel(rect, label);
		rect = rect.ShrinkLeft(GUI.LabelWidth);

		// Key
		rect.width = Unify(96);
		if (GUI.Button(rect, Util.GetKeyDisplayName(saving.Value.Key), Skin.SmallDarkButton)) {
			ActivatedSetting = saving;
			ShowKeyboardKeyPopup(rect.Shift(Unify(4), MasterScroll).BottomLeft());
		}
		GUI.PopupTriangleIcon(rect.Shrink(rect.height / 8));
		rect.SlideRight(padding * 4);

		// CSA
		rect.width = rect.height;

		GUI.BeginChangeCheck();

		// Ctrl
		rect.x += padding;
		GUI.Label(rect, "Ctrl", out var bounds, Skin.SmallGreyLabel);
		rect.x += bounds.width + padding;
		bool ctrl = GUI.Toggle(rect, saving.Value.Ctrl);
		rect.SlideRight(padding);

		// Shift
		rect.x += padding;
		GUI.Label(rect, "Shift", out bounds, Skin.SmallGreyLabel);
		rect.x += bounds.width + padding;
		bool shift = GUI.Toggle(rect, saving.Value.Shift);
		rect.SlideRight(padding);

		// Alt
		rect.x += padding;
		GUI.Label(rect, "Alt", out bounds, Skin.SmallGreyLabel);
		rect.x += bounds.width + padding;
		bool alt = GUI.Toggle(rect, saving.Value.Alt);
		rect.SlideRight(padding);

		if (GUI.EndChangeCheck()) {
			saving.Value = new Hotkey(saving.Value.Key, ctrl, shift, alt);
		}
	}


	private void ShowKeyboardKeyPopup (Int2 pos) {

		if (Instance == null || ActivatedSetting == null) return;

		GenericPopupUI.BeginPopup(pos);

		// Letter
		GenericPopupUI.AddItem(MENU_CATA_LETTER, Const.EmptyMethod);
		GenericPopupUI.BeginSubItem();
		for (int i = (int)KeyboardKey.A; i <= (int)KeyboardKey.Z; i++) {
			Add((KeyboardKey)i);
		}
		GenericPopupUI.EndSubItem();

		// Number
		GenericPopupUI.AddItem(MENU_CATA_NUMBER, Const.EmptyMethod);
		GenericPopupUI.BeginSubItem();
		for (int i = (int)KeyboardKey.Digit1; i <= (int)KeyboardKey.Digit0; i++) {
			Add((KeyboardKey)i);
		}
		for (int i = (int)KeyboardKey.Numpad0; i <= (int)KeyboardKey.Numpad9; i++) {
			Add((KeyboardKey)i);
		}
		GenericPopupUI.EndSubItem();

		// Sign
		GenericPopupUI.AddItem(MENU_CATA_SIGN, Const.EmptyMethod);
		GenericPopupUI.BeginSubItem();

		Add(KeyboardKey.Backquote);
		Add(KeyboardKey.Backslash);
		Add(KeyboardKey.Comma);
		Add(KeyboardKey.Equals);
		Add(KeyboardKey.LeftBracket);
		Add(KeyboardKey.Minus);
		Add(KeyboardKey.NumpadDivide);
		Add(KeyboardKey.NumpadEquals);
		Add(KeyboardKey.NumpadMinus);
		Add(KeyboardKey.NumpadMultiply);
		Add(KeyboardKey.NumpadPeriod);
		Add(KeyboardKey.NumpadPlus);
		Add(KeyboardKey.Period);
		Add(KeyboardKey.Quote);
		Add(KeyboardKey.RightBracket);
		Add(KeyboardKey.Semicolon);
		Add(KeyboardKey.Slash);

		GenericPopupUI.EndSubItem();

		// Other
		GenericPopupUI.AddItem(MENU_CATA_OTHER, Const.EmptyMethod);
		GenericPopupUI.BeginSubItem();

		Add(KeyboardKey.F1);
		Add(KeyboardKey.F2);
		Add(KeyboardKey.F3);
		Add(KeyboardKey.F4);
		Add(KeyboardKey.F5);
		Add(KeyboardKey.F6);
		Add(KeyboardKey.F7);
		Add(KeyboardKey.F8);
		Add(KeyboardKey.F9);
		Add(KeyboardKey.F10);
		Add(KeyboardKey.F11);
		Add(KeyboardKey.F12);

		GenericPopupUI.EndSubItem();

		// Func
		static void Add (KeyboardKey _k) => GenericPopupUI.AddItem(
			Util.GetKeyDisplayName(_k),
			Invoke,
			data: _k,
			@checked: Instance.ActivatedSetting.Value.Key == _k
		);
		static void Invoke () {
			if (Instance == null || Instance.ActivatedSetting == null) return;
			if (GenericPopupUI.InvokingItemData is not KeyboardKey newKey) return;
			var value = Instance.ActivatedSetting.Value;
			Instance.ActivatedSetting.Value = new Hotkey(newKey, value.Ctrl, value.Shift, value.Alt);
		}
	}


	#endregion




}
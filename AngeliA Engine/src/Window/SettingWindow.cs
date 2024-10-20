using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AngeliA;

namespace AngeliaEngine;



public class SettingWindow : WindowUI {




	#region --- SUB ---


	private class SettingGroup {
		public LanguageCode Name;
		public SpriteCode Icon;
		public List<SettingItem> Items;
		public bool Folding = true;
		public bool RigGroup = false;
	}


	private class SettingItem {
		public LanguageCode Name;
		public Saving Value;
		public SavingBool Requirement;
		public object CustomData;
		public bool GameOnly;
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly LanguageCode LABEL_THEME_BUILT_IN = ("Menu.BuiltInTheme", "Built-in");
	private static readonly LanguageCode MENU_CATA_LETTER = ("Menu.Group.Letter", "Letter");
	private static readonly LanguageCode MENU_CATA_NUMBER = ("Menu.Group.Number", "Number");
	private static readonly LanguageCode MENU_CATA_SIGN = ("Menu.Group.Sign", "Sign");
	private static readonly LanguageCode MENU_CATA_OTHER = ("Menu.Group.Other", "Fn");
	private static readonly LanguageCode LABEL_THEME = ("UI.EngineSetting.Theme", "Theme");
	private static readonly LanguageCode LABEL_LANGUAGE = ("UI.EngineSetting.Language", "Language");

	// Api
	public static SettingWindow Instance { get; private set; }
	public string RequireChangeThemePath { get; set; } = null;
	public bool MapSettingChanged { get; set; } = false;
	public override string DefaultWindowName => "Setting";

	// Data
	private static readonly List<SettingGroup> Groups = [];
	private readonly List<(string path, string name)> ThemePaths = [];
	private Project CurrentProject;
	private int MasterScroll = 0;
	private int UIHeight = 0;
	private SavingHotkey ActivatedSetting = null;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static TaskResult OnGameInitializeLater () {
		if (!SavingSystem.PoolReady) return TaskResult.Continue;
		// Init from Code
		Groups.Clear();
		var pool = new Dictionary<string, SettingGroup>();
		foreach (var (classType, _) in Util.AllClassWithAttribute<EngineSettingAttribute>()) {
			var fields = classType.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (var field in fields) {
				if (field.GetCustomAttribute<EngineSettingAttribute>() is not EngineSettingAttribute att) continue;
				if (field.GetValue(null) is not Saving value) continue;
				// Create or Get Group
				if (!pool.TryGetValue(att.Group, out var group)) {
					group = new SettingGroup() {
						Items = [],
						Name = ($"UI.EngineSetting.{att.Group}", att.Group),
						Icon = $"Icon.SettingFold.{att.Group}",
						Folding = att.Group != "Engine",
						RigGroup = att.Group == "MapEditor",
					};
					pool.Add(att.Group, group);
				}
				// Add Item into Group
				SavingBool req = null;
				if (!string.IsNullOrEmpty(att.RequireSettingName)) {
					var rqField = classType.GetField(att.RequireSettingName, BindingFlags.Public | BindingFlags.Static);
					if (rqField != null && rqField.FieldType == typeof(SavingBool)) {
						req = rqField.GetValue(null) as SavingBool;
					}
				}
				group.Items.Add(new SettingItem() {
					Name = ($"UI.EngineSetting.{field.Name}", att.DisplayLabel),
					Value = value,
					Requirement = req,
					GameOnly = att.GameOnly,
					CustomData =
						value is SavingColor32 sValue ? sValue.Value.ToColorF() :
						value is SavingColor32NoAlpha sValueNa ? sValueNa.Value.ToColorF() :
						null,
				});
			}
		}
		Groups.AddRange(pool.Values);
		Groups.Sort((a, b) => a.Folding.CompareTo(b.Folding));
		return TaskResult.End;
	}


	public SettingWindow () => Instance = this;


	public override void UpdateWindowUI () {

		if (CurrentProject == null) return;

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

			using var _ = new GUILabelWidthScope(Util.Min(Unify(320), rect.width / 2));

			// Group Content
			bool isGameProject = CurrentProject.Universe.Info.ProjectType == ProjectType.Game;
			for (int groupIndex = 0; groupIndex < Groups.Count; groupIndex++) {
				var group = Groups[groupIndex];
				if (!isGameProject && IsGroupIgnored(group)) continue;
				DrawGroup(ref rect, group, groupIndex == 0 ? DrawExtraContent : null, out bool changed);
				if (group.RigGroup && changed) {
					MapSettingChanged = true;
				}
			}

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


	#endregion




	#region --- API ---


	public void SetCurrentProject (Project project) => CurrentProject = project;


	#endregion




	#region --- LGC ---


	private void DrawGroup (ref IRect rect, SettingGroup groupData, Func<IRect, IRect> extraContent, out bool changed) {

		var boxPadding = Int4.Direction(Unify(24), Unify(4), Unify(3), Unify(3));
		int boxTop = rect.yMax;
		int boxLeft = rect.xMin;
		int boxRight = rect.xMax;
		var isGameProject = CurrentProject != null && CurrentProject.Universe.Info.ProjectType == ProjectType.Game;
		GUI.BeginChangeCheck();

		// Fold
		if (!GUI.ToggleFold(
			rect, ref groupData.Folding, groupData.Icon, groupData.Name, boxPadding.left, boxPadding.right
		)) {

			rect.SlideDown(GUI.FieldPadding);

			// Content
			foreach (var item in groupData.Items) {

				if (item.Requirement != null && !item.Requirement.Value) continue;
				if (!isGameProject && item.GameOnly) continue;

				switch (item.Value) {

					default:
						using (new GUIContentColorScope(Color32.RED_BETTER)) {
							GUI.SmallLabel(rect, item.Name);
						}
						break;

					case SavingBool sBool:
						sBool.Value = GUI.Toggle(
							rect, sBool.Value, item.Name,
							labelStyle: Skin.SmallLabel
						);
						break;

					case SavingColor32 sColor32: {
						var colorF = (ColorF)item.CustomData;
						int version = GUI.ContentVersion;
						colorF = GUI.HorizontalColorField(
							colorF, rect, item.Name, Skin.SmallLabel, true, true, true, false, sColor32.DefaultValue.ToColorF()
						);
						if (version != GUI.ContentVersion) {
							sColor32.Value = colorF.ToColor32();
							item.CustomData = colorF;
						}
						break;
					}

					case SavingColor32NoAlpha sColor32na: {
						var colorF = (ColorF)item.CustomData;
						GUI.BeginChangeCheck();
						colorF = GUI.HorizontalColorField(
							colorF, rect, item.Name, Skin.SmallLabel, true, false, true, false, sColor32na.DefaultValue.ToColorF()
						);
						if (GUI.EndChangeCheck()) {
							sColor32na.Value = colorF.ToColor32();
							item.CustomData = colorF;
						}
						break;
					}

					case SavingHotkey sHotkey:
						HotkeyField(rect, sHotkey, item.Name);
						break;

				}
				rect.SlideDown(GUI.FieldPadding);
			}

			// Extra Content
			if (extraContent != null) {
				rect = extraContent(rect);
			}
		} else {
			rect.SlideDown();
		}

		changed = GUI.EndChangeCheck();

		// Box BG
		if (Renderer.TryGetSprite(EngineSprite.UI_PANEL_GENERAL, out var sprite)) {
			using (new DefaultLayerScope()) {
				Renderer.DrawSlice(sprite, new IRect(
					boxLeft - boxPadding.left,
					rect.yMax - boxPadding.down + MasterScroll,
					boxRight - boxLeft + boxPadding.horizontal,
					boxTop - rect.yMax + boxPadding.vertical
				));
			}
		}
		rect.y -= boxPadding.vertical + Unify(6);
	}


	private IRect DrawExtraContent (IRect rect) {

		// Theme
		GUI.SmallLabel(rect, LABEL_THEME);
		var popRect = rect.ShrinkLeft(GUI.LabelWidth).LeftHalf();
		if (GUI.Button(popRect, Skin.Name == "Built-in" ? LABEL_THEME_BUILT_IN : Skin.Name, Skin.SmallDarkButton)) {
			ShowThemeMenu(popRect.Shift(Unify(4), MasterScroll).BottomLeft());
		}
		GUI.PopupTriangleIcon(popRect.Shrink(rect.height / 8));
		rect.SlideDown(GUI.FieldPadding);

		// Language
		GUI.SmallLabel(rect, LABEL_LANGUAGE);
		popRect = rect.ShrinkLeft(GUI.LabelWidth).LeftHalf();
		if (GUI.Button(popRect, Util.GetLanguageDisplayName(Language.CurrentLanguage), Skin.SmallDarkButton)) {
			ShowLanguageMenu(popRect.Shift(Unify(4), MasterScroll).BottomLeft());
		}
		GUI.PopupTriangleIcon(popRect.Shrink(rect.height / 8));
		rect.SlideDown(GUI.FieldPadding);

		return rect;
	}


	// Util
	private void ShowThemeMenu (Int2 pos) {

		// Reload Theme Paths
		ThemePaths.Clear();
		string themeFolder = EngineUtil.ThemeRoot;
		if (!Util.FolderExists(themeFolder)) return;
		foreach (var path in Util.EnumerateFiles(themeFolder, true, AngePath.SHEET_SEARCH_PATTERN)) {
			ThemePaths.Add((path, Util.GetDisplayName(Util.GetNameWithoutExtension(path))));
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


	private void ShowLanguageMenu (Int2 pos) {

		GenericPopupUI.BeginPopup(pos);

		int len = Language.LanguageCount;
		for (int i = 0; i < len; i++) {
			string lan = Language.GetLanguageAt(i);
			GenericPopupUI.AddItem(
				Util.GetLanguageDisplayName(lan),
				MenuInvoke,
				true,
				lan == Language.CurrentLanguage,
				data: lan
			);
		}
		static void MenuInvoke () {
			if (GenericPopupUI.InvokingItemData is not string lan) return;
			Language.SetLanguage(lan);
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


	private bool IsGroupIgnored (SettingGroup group) {
		foreach (var item in group.Items) {
			if (!item.GameOnly) return false;
		}
		return true;
	}


	#endregion




}
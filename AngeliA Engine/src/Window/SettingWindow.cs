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
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode UI_PANEL_SETTING = "UI.Panel.Setting";

	private static readonly LanguageCode LABEL_THEME_BUILT_IN = ("Menu.BuiltInTheme", "Built-in");
	private static readonly LanguageCode MENU_CATA_LETTER = ("Menu.Group.Letter", "Letter");
	private static readonly LanguageCode MENU_CATA_NUMBER = ("Menu.Group.Number", "Number");
	private static readonly LanguageCode MENU_CATA_SIGN = ("Menu.Group.Sign", "Sign");
	private static readonly LanguageCode MENU_CATA_OTHER = ("Menu.Group.Other", "Fn");
	private static readonly LanguageCode LABEL_THEME = ("UI.EngineSetting.Theme", "Theme");

	// Api
	public static SettingWindow Instance { get; private set; }
	public string RequireChangeThemePath { get; set; } = null;
	public bool RigSettingChanged { get; set; } = false;
	public override string DefaultName => "Setting";

	// Data
	private static readonly List<SettingGroup> Groups = new();
	private readonly List<(string path, string name)> ThemePaths = new();
	private bool RequiringReloadThemePath = true;
	private int MasterScroll = 0;
	private int UIHeight = 0;
	private Project CurrentProject;
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
						Items = new(),
						Name = ($"UI.EngineSetting.{att.Group}", att.Group),
						Icon = $"Icon.SettingFold.{att.Group}",
						Folding = att.Group != "Engine",
						RigGroup = att.Group == "Map Editor",
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

			using var _ = new GUILabelWidthScope(Util.Min(Unify(320), rect.width / 2));

			// Group Content
			bool useProceduralMap = CurrentProject.Universe.Info.UseProceduralMap;
			for (int groupIndex = 0; groupIndex < Groups.Count; groupIndex++) {
				var group = Groups[groupIndex];
				if (useProceduralMap && group.Name == "Map Editor") continue;
				DrawGroup(ref rect, group, groupIndex == 0 ? DrawExtraContent : null, out bool changed);
				if (group.RigGroup && changed) {
					RigSettingChanged = true;
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


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
	}


	#endregion




	#region --- LGC ---


	private void DrawGroup (ref IRect rect, SettingGroup groupData, Func<IRect, IRect> extraContent, out bool changed) {

		var boxPadding = Int4.Direction(Unify(24), Unify(4), Unify(3), Unify(3));
		int boxTop = rect.yMax;
		int boxLeft = rect.xMin;
		int boxRight = rect.xMax;
		GUI.BeginChangeCheck();

		// Fold Icon
		GUI.Icon(rect.Edge(Direction4.Left, rect.height * 3 / 4).Shift(-boxPadding.left / 5, 0), groupData.Icon);

		// Fold Label
		if (GUI.Button(rect.Expand(boxPadding.left, boxPadding.right, 0, 0), 0, Skin.WeakHighlightPixel)) {
			groupData.Folding = !groupData.Folding;
		}
		GUI.Label(rect.ShrinkLeft(rect.height), groupData.Name, Skin.SmallGreyLabel);

		// Fold Triangle
		using (new GUIColorScope(Color32.GREY_128)) {
			GUI.Icon(
				rect.EdgeOutside(Direction4.Left, rect.height * 2 / 3).Shift(-boxPadding.left / 4, 0),
				groupData.Folding ? BuiltInSprite.ICON_TRIANGLE_RIGHT : BuiltInSprite.ICON_TRIANGLE_DOWN
			);
		}
		rect.SlideDown(groupData.Folding ? 0 : GUI.FieldPadding);

		if (!groupData.Folding) {
			// Content
			foreach (var item in groupData.Items) {
				if (item.Requirement != null && !item.Requirement.Value) continue;
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
							colorF, rect, true, true, true, false, sColor32.DefaultValue.ToColorF()
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
							colorF, rect, true, false, true, false, sColor32na.DefaultValue.ToColorF()
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

		}

		changed = GUI.EndChangeCheck();

		// Box BG
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


	private IRect DrawExtraContent (IRect rect) {
		GUI.SmallLabel(rect, LABEL_THEME);
		var popRect = rect.ShrinkLeft(GUI.LabelWidth).LeftHalf();
		if (GUI.Button(popRect, Skin.Name, Skin.SmallDarkButton)) {
			ShowThemeMenu(popRect.Shift(Unify(4), MasterScroll).BottomLeft());
		}
		GUI.PopupTriangleIcon(popRect.Shrink(rect.height / 8));
		rect.SlideDown(GUI.FieldPadding);
		return rect;
	}


	// Util
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
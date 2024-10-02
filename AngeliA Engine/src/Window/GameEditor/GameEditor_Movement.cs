using System.Collections.Generic;
using System.Reflection;
using AngeliA;

namespace AngeliaEngine;

public partial class GameEditor {

	// SUB
	private enum MovementFieldType { Int, Bool, }

	private class MovementFieldData {
		public MovementFieldType Type;
		public string Key;
		public LanguageCode DisplayName;
		public string ValueString = "";
		public PropVisibilityAttribute Visible = null;
		public bool Separator = false;
		public int DefaultValue = 0;
	}

	// Api
	public int RigGameSelectingPlayerID { get; set; } = 0;

	// Data
	private readonly Dictionary<int, Dictionary<string, int>> ConfigPool = [];
	private readonly Dictionary<int, string> ConfigPath = [];
	private readonly LanguageCode[] MovementTabNames;
	private readonly IntToChars MovementTabLabelToChars;
	private readonly MovementFieldData[][] MovementFields;
	private readonly int MovementTabCount;
	private int MovementTab = 0;
	private int PrevMovementTabIndex = -1;
	private int PrevSelectingPlayerID = -1;
	private bool IsMovementEditorDirty = false;
	private Project CurrentProject = null;
	private bool ConfigPoolInitialized = false;


	// MSG
	private void DrawMovementPanel (ref IRect panelRect) {

		if (CurrentProject == null) return;

		if (!ConfigPoolInitialized) {
			ConfigPoolInitialized = true;
			InitializeMovementConfigPool();
		}

		if (
			RigGameSelectingPlayerID == 0 ||
			!ConfigPool.TryGetValue(RigGameSelectingPlayerID, out var configMap)
		) return;

		// Min Width
		int minWidth = Unify(296);
		if (panelRect.width < minWidth) {
			panelRect.xMin = panelRect.xMax - minWidth;
		}

		// Content
		int panelPadding = Unify(12);
		int padding = Unify(6);
		int toolbarSize = Unify(28);
		int top = panelRect.y;
		var rect = new IRect(panelRect.x, panelRect.y - toolbarSize, panelRect.width, toolbarSize);
		rect = rect.Shrink(panelPadding, panelPadding, 0, 0);

		// Tab Bar
		using (new GUIContentColorScope(Color32.GREY_196)) {
			if (GUI.Button(rect.EdgeLeft(rect.height), BuiltInSprite.ICON_TRIANGLE_LEFT, Skin.SmallIconButton)) {
				MovementTab = (MovementTab - 1).Clamp(0, MovementTabCount - 1);
			}
			if (GUI.Button(rect.EdgeRight(rect.height), BuiltInSprite.ICON_TRIANGLE_RIGHT, Skin.SmallIconButton)) {
				MovementTab = (MovementTab + 1).Clamp(0, MovementTabCount - 1);
			}
		}
		var fields = MovementFields[MovementTab];

		// Tab Changed
		if (PrevMovementTabIndex != MovementTab || PrevSelectingPlayerID != RigGameSelectingPlayerID) {
			PrevMovementTabIndex = MovementTab;
			PrevSelectingPlayerID = RigGameSelectingPlayerID;
			foreach (var field in fields) {
				if (field.Type != MovementFieldType.Int) continue;
				if (configMap.TryGetValue(field.Key, out int configValue)) {
					field.ValueString = configValue.ToString();
				}
			}
		}

		// Name Label
		GUI.Label(
			rect.ShrinkRight(rect.height),
			MovementTabNames[MovementTab],
			out var bounds, Skin.SmallCenterLabel
		);

		// Number Label
		GUI.Label(
			bounds.EdgeRight(1),
			MovementTabLabelToChars.GetChars(MovementTab + 1),
			Skin.SmallGreyLabel
		);
		rect.SlideDown(padding);

		// Props
		for (int i = 0; i < fields.Length; i++) {
			var fieldData = fields[i];
			if (fieldData.Visible != null && !fieldData.Visible.PropMatch(configMap)) {
				continue;
			}
			GUI.SmallLabel(rect, fieldData.DisplayName);
			var valueRect = rect.ShrinkLeft(GUI.LabelWidth);
			switch (fieldData.Type) {
				case MovementFieldType.Int: {
					// Int
					if (!configMap.TryGetValue(fieldData.Key, out int value)) break;
					fieldData.ValueString = GUI.SmallInputField(91243895 + i, valueRect, fieldData.ValueString, out _, out bool confirm);
					if (!confirm) break;
					if (int.TryParse(fieldData.ValueString, out int newValue)) {
						IsMovementEditorDirty = IsMovementEditorDirty || newValue != value;
						configMap[fieldData.Key] = newValue;
					} else {
						newValue = value;
					}
					fieldData.ValueString = newValue.ToString();
					break;
				}
				case MovementFieldType.Bool: {
					// Bool
					if (!configMap.TryGetValue(fieldData.Key, out int iValue)) break;
					bool value = iValue == 1;
					bool newValue = GUI.Toggle(valueRect, value);
					if (value != newValue) {
						IsMovementEditorDirty = true;
						configMap[fieldData.Key] = newValue ? 1 : 0;
					}
					break;
				}
			}
			rect.SlideDown(padding);
			// Separator
			if (fieldData.Separator) {
				rect.y -= padding * 2;
			}
		}

		rect.y -= padding * 4;

		// Apply Button
		rect.yMin = rect.yMax - GUI.FieldHeight;
		using (new GUIEnableScope(IsMovementEditorDirty))
		using (new GUIBodyColorScope(IsMovementEditorDirty ? Color32.GREEN_BETTER : Color32.WHITE)) {
			if (GUI.Button(rect.Shrink(rect.width / 6, rect.width / 6, 0, 0), BuiltInText.UI_APPLY, Skin.SmallDarkButton)) {
				int id = RigGameSelectingPlayerID;
				if (ConfigPath.TryGetValue(id, out string configPath)) {
					FrameworkUtil.Pairs_to_NameAndIntFile(configMap, configPath);
				}
				RequireReloadPlayerMovement = true;
				IsMovementEditorDirty = false;
			}
		}
		rect.SlideDown(padding);

		// Final
		panelRect.height = top - rect.yMax;
		panelRect.y -= panelRect.height;

	}


	private void InitializeMovementConfigPool () {
		string root = CurrentProject.Universe.CharacterMovementConfigRoot;
		foreach (string path in Util.EnumerateFiles(root, true, "*.txt")) {
			string name = Util.GetNameWithoutExtension(path);
			int id = name.AngeHash();
			// Load Field Data from File
			var list = new List<(string, int)>();
			FrameworkUtil.NameAndIntFile_to_List(list, path);
			var map = new Dictionary<string, int>();
			foreach (var (key, value) in list) {
				map.TryAdd(key, value);
			}
			// Fill Missing Fields
			foreach (var fields in MovementFields) {
				foreach (var field in fields) {
					if (map.ContainsKey(field.Key)) continue;
					map.Add(field.Key, field.DefaultValue);
				}
			}
			ConfigPool.TryAdd(id, map);
			ConfigPath.TryAdd(id, path);
		}
	}


}

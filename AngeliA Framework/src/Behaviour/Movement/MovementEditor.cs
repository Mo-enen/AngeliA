using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AngeliA;



public class MovementEditor {


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

	// Const
	public const int SETTING_PANEL = 3632167;

	// Data
	private static MovementEditor Instance = null;
	private readonly Dictionary<int, Dictionary<string, int>> ConfigPool = [];
	private readonly Dictionary<int, string> ConfigPath = [];
	private CharacterMovement MovementFieldHost = null;
	private LanguageCode[] MovementTabNames;
	private IntToChars MovementTabLabelToChars;
	private MovementFieldData[][] MovementFields;
	private int MovementTabCount;
	private int MovementTab = 0;
	private int PrevMovementTabIndex = -1;
	private int PrevSelectingPlayerID = -1;
	private bool ShowingPanel = false;
	private bool IsMovementEditorDirty = false;
	private bool ConfigPoolInitialized = false;


	// MSG
	[OnRemoteSettingChanged_IntID_IntData]
	internal static void OnRemoteSettingChanged (int id, int data) {
		switch (id) {
			case SETTING_PANEL:
				Instance ??= new();
				if (data == 1) {
					Instance.ShowingPanel = true;
				} else {
					Instance.ShowingPanel = false;
				}
				break;
		}
	}


#if DEBUG
	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		if (Instance == null || !Instance.ShowingPanel) return;
		if (PlayerSystem.Selecting == null) return;
		Cursor.RequireCursor();
		using var _ = new UILayerScope();
		var panelRect = Renderer.CameraRect.CornerInside(Alignment.TopRight, GUI.Unify(296), 0);
		var bgCell = Renderer.DrawPixel(panelRect, Color32.BLACK);
		Instance.DrawMovementPanel(PlayerSystem.Selecting.NativeMovement, ref panelRect);
		bgCell.SetRect(panelRect);
		if (panelRect.MouseInside()) {
			Input.UseAllMouseKey();
			Input.IgnoreMouseToActionJump();
		}
	}
#endif


	internal MovementEditor () => Instance = this;


	private void InitMovementFields (System.Type movementType) {

		// Movements
		try {

			var hostMov = System.Activator.CreateInstance(movementType, [null]);
			//var hostMov = new CharacterMovement(null);
			var fields = movementType.GetFields(
				BindingFlags.Public | BindingFlags.Instance
			).OrderBy(f => f.MetadataToken).ToArray();

			// Get Group Name and Count
			MovementTabCount = 0;
			var mTabNames = new List<LanguageCode>();
			for (int i = 0; i < fields.Length; i++) {
				var field = fields[i];
				var group = field.GetCustomAttribute<PropGroupAttribute>();
				if (group == null) continue;
				mTabNames.Add((
					$"UI.MovementTab.{group.Name}",
					Util.GetDisplayName(group.Name)
				));
			}
			MovementTabNames = [.. mTabNames];
			MovementTabCount = mTabNames.Count;
			MovementTabLabelToChars = new("  (", $"/{MovementTabCount})");

			// Get all Field Data
			var intType = typeof(FrameBasedInt);
			var boolType = typeof(FrameBasedBool);
			MovementFields = new MovementFieldData[MovementTabCount][];
			string currentGroup = "";
			var list = new List<MovementFieldData>();
			int currentFieldGroupIndex = 0;
			for (int i = 0; i < fields.Length; i++) {
				var field = fields[i];
				var group = field.GetCustomAttribute<PropGroupAttribute>();
				if (group != null || i == fields.Length - 1) {
					if (
						list.Count > 0 &&
						currentFieldGroupIndex < MovementTabCount &&
						!string.IsNullOrEmpty(currentGroup)
					) {
						MovementFields[currentFieldGroupIndex] = [.. list];
						currentFieldGroupIndex++;
						list.Clear();
					}
					if (group != null) currentGroup = group.Name;
				}
				if (field.FieldType != intType && field.FieldType != boolType) continue;
				var defaultValue = field.GetValue(hostMov);
				bool isInt = field.FieldType == intType;
				list.Add(new MovementFieldData() {
					Key = field.Name,
					Type = isInt ? MovementFieldType.Int : MovementFieldType.Bool,
					DisplayName = ($"UI.MovementProp.{field.Name}", Util.GetDisplayName(field.Name)),
					Visible = field.GetCustomAttribute<PropVisibilityAttribute>(),
					Separator = field.GetCustomAttribute<PropSeparatorAttribute>() != null,
					DefaultValue =
						defaultValue is FrameBasedInt iValue ? iValue.BaseValue :
						defaultValue is FrameBasedBool bValue ? (bValue.BaseValue ? 1 : 0) :
						0
				});
			}
			for (int i = 0; i < MovementFields.Length; i++) {
				MovementFields[i] ??= [];
			}

		} catch (System.Exception ex) { Debug.LogException(ex); }

	}


	private void InitializeMovementConfigPool () {
		string root = Universe.BuiltIn.CharacterMovementConfigRoot;
		foreach (string path in Util.EnumerateFiles(root, true, AngePath.MOVEMENT_CONFIG_SEARCH_PATTERN)) {
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


	private void DrawMovementPanel (CharacterMovement movement, ref IRect panelRect) {

		// Init for Fields
		if (MovementFieldHost != movement) {
			MovementFieldHost = movement;
			InitMovementFields(movement.GetType());
		}

		// Init for Pool
		if (!ConfigPoolInitialized) {
			ConfigPoolInitialized = true;
			InitializeMovementConfigPool();
		}

		int playerID = PlayerSystem.Selecting.TypeID;

		if (playerID == 0 || !ConfigPool.TryGetValue(playerID, out var configMap)) return;

		// Content
		int panelPadding = GUI.Unify(12);
		int padding = GUI.Unify(6);
		int toolbarSize = GUI.Unify(28);
		int top = panelRect.y;
		var rect = new IRect(panelRect.x, panelRect.y - toolbarSize, panelRect.width, toolbarSize);
		rect = rect.Shrink(panelPadding, panelPadding, 0, 0);

		// Tab Bar
		using (new GUIContentColorScope(Color32.GREY_196)) {
			if (GUI.Button(rect.EdgeInsideLeft(rect.height), BuiltInSprite.ICON_TRIANGLE_LEFT, GUI.Skin.SmallIconButton)) {
				MovementTab = (MovementTab - 1).Clamp(0, MovementTabCount - 1);
			}
			if (GUI.Button(rect.EdgeInsideRight(rect.height), BuiltInSprite.ICON_TRIANGLE_RIGHT, GUI.Skin.SmallIconButton)) {
				MovementTab = (MovementTab + 1).Clamp(0, MovementTabCount - 1);
			}
		}
		var fields = MovementFields[MovementTab];

		// Tab Changed
		if (PrevMovementTabIndex != MovementTab || PrevSelectingPlayerID != playerID) {
			PrevMovementTabIndex = MovementTab;
			PrevSelectingPlayerID = playerID;
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
			out var bounds, GUI.Skin.SmallCenterLabel
		);

		// Number Label
		GUI.Label(
			bounds.EdgeInsideRight(1),
			MovementTabLabelToChars.GetChars(MovementTab + 1),
			GUI.Skin.SmallGreyLabel
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
			if (GUI.Button(rect.Shrink(rect.width / 6, rect.width / 6, 0, 0), BuiltInText.UI_APPLY, GUI.Skin.SmallDarkButton)) {
				int id = playerID;
				if (ConfigPath.TryGetValue(id, out string configPath)) {
					FrameworkUtil.Pairs_to_NameAndIntFile(configMap, configPath);
				}
				//RequireReloadPlayerMovement = true;
				PlayerSystem.Selecting.Movement.ReloadMovementConfigFromFile();
				IsMovementEditorDirty = false;
			}
		}
		rect.SlideDown(padding);

		// Final
		panelRect.height = top - rect.yMax;
		panelRect.y -= panelRect.height;

	}


}

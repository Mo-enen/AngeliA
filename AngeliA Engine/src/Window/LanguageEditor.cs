using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;


public partial class LanguageEditor : WindowUI {




	#region --- SUB ---


	private class LanguageLine {
		public string Key;
		public string Label;
		public bool Visible;
		public List<string> Value;
	}


	private class LineComparer : IComparer<LanguageLine> {
		public static readonly LineComparer Instance = new();
		public int Compare (LanguageLine a, LanguageLine b) => a.Key.CompareTo(b.Key);
	}


	#endregion




	#region --- VAR ---


	// Const
	public static readonly int TYPE_ID = typeof(LanguageEditor).AngeHash();

	private static readonly SpriteCode UI_BG = "UI.Language.BG";
	private static readonly SpriteCode UI_TOOLBAR = "UI.Language.Toolbar";

	private static readonly LanguageCode DELETE_MSG = ("UI.LanguageEditor.DeleteMsg", "Delete Language {0}?");
	private static readonly LanguageCode ADD_KEY = ("UI.LanguageEditor.AddKey", "+ Key");
	private static readonly LanguageCode ADD_LANGUAGE = ("UI.LanguageEditor.AddLanguage", "+ Language");
	private static readonly LanguageCode ADD_ALL_LAN_CODE = ("UI.LanguageEditor.AddAllLanCode", "+ All Language Code");
	private static readonly LanguageCode MSG_LAN_CODE_ADDED = ("UI.LanguageEditor.LanCodeAddedMsg", "Language code added");
	private static readonly LanguageCode UI_LABEL_KEY = ("UI.LanguageEditor.Key", "Key");
	private static readonly LanguageCode MSG_ALL_LAN_CODE = ("UI.LanguageEditor.AddAllLanCodeMsg", "Add keys for all language code in game script?");
	private static readonly LanguageCode MSG_HELP = ("UI.LanguageEditor.HelpMsg", "Empty keys will be deleted when open the project next time");
	private const int SEARCH_ID = -19223;

	// Api
	public static LanguageEditor Instance { get; private set; }
	public Project CurrentProject { get; private set; } = null;
	public override string DefaultWindowName => "Language";
	public bool RequireAddKeysForAllLanguageCode { get; set; } = false;

	// Data
	private readonly List<string> Languages = [];
	private readonly List<LanguageLine> Lines = [];
	private readonly GUIStyle IndexLabelStyle = new(GUI.Skin.SmallGreyLabel) { Alignment = Alignment.MidRight };
	private int ScrollY = 0;
	private string SearchingText = string.Empty;
	private int RequireReloadWhenFileChangedFrame = -1;
	private long ReloadCheckingDate = 0;


	#endregion




	#region --- MSG ---


	public LanguageEditor () => Instance = this;


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		if (project == null) return;
#if DEBUG
		// Sync Engine >> Project
		if (CurrentProject.IsEngineInternalProject) {
			string from = Universe.BuiltIn.LanguageRoot;
			if (Util.FolderExists(from)) {
				string to = project.Universe.LanguageRoot;
				Util.DeleteFolder(to);
				Util.CopyFolder(from, to, true, true, true);
			}
		}
#endif
		Load(project.Universe.LanguageRoot);
		ScrollY = 0;
		SearchingText = string.Empty;
		RequireAddKeysForAllLanguageCode = false;
		RequireReloadWhenFileChangedFrame = -1;
	}


	public override void OnActivated () {
		base.OnActivated();
		ScrollY = 0;
		SearchingText = string.Empty;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {

		if (CurrentProject == null) return;

		Cursor.RequireCursor();

		var panelRect = WindowRect;
		int column = Languages.Count + 1;
		int fieldWidth = panelRect.width / column;

		panelRect.x += (panelRect.width - fieldWidth * column) / 2;
		panelRect.width = fieldWidth * column;

		if (Game.IsKeyboardKeyHolding(KeyboardKey.LeftCtrl) && Game.IsKeyboardKeyHolding(KeyboardKey.S)) {
			Save();
		}

		GUI.DrawSlice(UI_BG, WindowRect.ShrinkUp(GUI.ToolbarSize));

		using var _ = new GUIInteractableScope(Game.GlobalFrame > RequireReloadWhenFileChangedFrame);

		Update_Toolbar(panelRect.Edge(Direction4.Up, Unify(84)));
		Update_Content(panelRect.Edge(Direction4.Down, panelRect.height - Unify(84)));

		if (Game.GlobalFrame <= RequireReloadWhenFileChangedFrame) {
			Update_ReloadWhenFileChanged();
		}

	}


	private void Update_Toolbar (IRect panelRect) {

		// BG
		GUI.DrawSlice(UI_TOOLBAR, panelRect.TopHalf());

		// Shift Panel Rect
		int labelHeight = panelRect.height - Unify(42);
		panelRect.height = Unify(42);
		panelRect.y += labelHeight;

		// Line
		Renderer.DrawPixel(panelRect.Edge(Direction4.Down, Unify(1)), Color32.GREY_12, 1);

		// + Key
		var rect = panelRect;
		rect.width = Unify(108);
		if (GUI.Button(rect, ADD_KEY, Skin.SmallCenterLabelButton)) {
			ScrollY = 0;
			Lines.Insert(0, new LanguageLine() {
				Key = string.Empty,
				Label = string.Empty,
				Value = new List<string>(new string[Languages.Count].FillWithValue(string.Empty)),
			});
			SetDirty();
			SearchingText = "";
		}
		Cursor.SetCursorAsHand(rect);
		rect.SlideRight();

		// Line
		Renderer.DrawPixel(rect.EdgeOutside(Direction4.Left, Unify(1)), Color32.GREY_12, 2);

		// + Language
		rect.width = Unify(108);
		if (GUI.Button(rect, ADD_LANGUAGE, Skin.SmallCenterLabelButton)) {
			OpenAddLanguagePopup();
			SearchingText = "";
		}
		Cursor.SetCursorAsHand(rect);
		rect.SlideRight();

		// Line
		Renderer.DrawPixel(rect.EdgeOutside(Direction4.Left, Unify(1)), Color32.GREY_12, 2);

		// + All Language Code
		rect.width = Unify(158);
		if (GUI.Button(rect, ADD_ALL_LAN_CODE, Skin.SmallCenterLabelButton)) {
			GenericDialogUI.SpawnDialog_Button(
				MSG_ALL_LAN_CODE,
				BuiltInText.UI_ADD, AddForAllLanguageCode, BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
			GenericDialogUI.SetItemTint(Color32.GREEN_BETTER);
			SearchingText = "";
		}
		Cursor.SetCursorAsHand(rect);
		rect.SlideRight();

		// Line
		Renderer.DrawPixel(rect.EdgeOutside(Direction4.Left, Unify(1)), Color32.GREY_12, 2);

		// Line
		Renderer.DrawPixel(rect.EdgeOutside(Direction4.Left, Unify(1)), Color32.GREY_12, 2);

		// Search
		rect.width = panelRect.xMax - rect.x;
		var searchRect = rect.Shrink(Unify(6)).MidHalf();
		SearchingText = GUI.SmallInputField(SEARCH_ID, searchRect, SearchingText, out _, out bool confirm);
		if (GUI.TypingTextFieldID != SEARCH_ID && string.IsNullOrEmpty(SearchingText)) {
			GUI.Icon(
				searchRect.Edge(Direction4.Left, searchRect.height * 8 / 10).Shift(searchRect.height / 6, 0),
				BuiltInSprite.ICON_SEARCH
			);
		}
		if (confirm) {
			ScrollY = 0;
			const System.StringComparison OIC = System.StringComparison.OrdinalIgnoreCase;
			if (string.IsNullOrWhiteSpace(SearchingText)) {
				foreach (var line in Lines) line.Visible = true;
			} else {
				for (int i = 0; i < Lines.Count; i++) {
					var line = Lines[i];
					line.Visible = false;
					if (!line.Key.Contains(SearchingText, OIC)) {
						foreach (var value in line.Value) {
							if (value.Contains(SearchingText, OIC)) {
								line.Visible = true;
								break;
							}
						}
					} else {
						line.Visible = true;
					}
				}
			}
		}

		// Help Button
		if (GUI.Button(panelRect.Edge(Direction4.Right, Unify(36)), "?", Skin.SmallCenterLabelButton)) {
			GenericDialogUI.SpawnDialog_Button(MSG_HELP, BuiltInText.UI_OK, Const.EmptyMethod);
			SearchingText = "";
		}

		// Labels
		int panelPadding = Unify(64);
		int labelWidth = (panelRect.width - Unify(24) - panelPadding * 2) / (Languages.Count + 1);
		var labelRect = new IRect(panelRect.x + panelPadding + Unify(12), panelRect.y - labelHeight, labelWidth, labelHeight);
		GUI.Label(labelRect, UI_LABEL_KEY, Skin.SmallGreyLabel);
		labelRect.x += labelRect.width;
		for (int i = 0; i < Languages.Count; i++) {
			string name = Util.GetLanguageDisplayName(Languages[i]);
			GUI.Label(labelRect, name, Skin.SmallGreyLabel);
			labelRect.x += labelRect.width;
		}

		// Func
		static void AddForAllLanguageCode () {
			if (Instance == null || Instance.Languages.Count == 0) return;
			var project = Instance.CurrentProject;
			if (project == null) return;
			Instance.Save();
			if (project.IsEngineInternalProject) {
				// Engine Artworl Project
				Instance.Lines.RemoveAll(line => string.IsNullOrEmpty(line.Key));
				int oldLineCount = Instance.Lines.Count;
				LanguageUtil.AddKeysForAllLanguageCode(project.Universe.LanguageRoot);
				Instance.Load(project.Universe.LanguageRoot);
				Instance.SetDirty();
				GenericDialogUI.SpawnDialog_Button(
					string.Format(MSG_LAN_CODE_ADDED, (Instance.Lines.Count - oldLineCount).GreaterOrEquelThanZero()),
					BuiltInText.UI_OK, Const.EmptyMethod
				);
			} else {
				// Common Project
				Instance.RequireAddKeysForAllLanguageCode = true;
				Instance.RequireReloadWhenFileChangedFrame = Game.GlobalFrame + 60;
				string lan = Instance.Languages[0];
				Instance.ReloadCheckingDate = Util.GetFileModifyDate(
					Util.CombinePaths(project.Universe.LanguageRoot, lan, $"{lan}.{AngePath.LANGUAGE_FILE_EXT}")
				);

			}
		}
	}


	private void Update_Content (IRect panelRect) {

		int itemHeight = Unify(36);
		if (panelRect.height <= itemHeight) return;

		int scrollBarWidth = GUI.ScrollbarSize;
		int labelHeight = Unify(22);
		int labelPadding = Unify(12);
		int itemSpaceX = Unify(5);
		int itemSpaceY = Unify(1);
		int panelPadding = Unify(64);
		int indexWidth = Unify(32);
		int highlightBorder = GUI.Unify(1);
		panelRect = panelRect.Shrink(panelPadding, panelPadding + scrollBarWidth, 0, 0);
		int pageCount = panelRect.height.CeilDivide(itemHeight);
		int shiftedItemCount = Lines.Count + 6;
		if (pageCount > shiftedItemCount) {
			ScrollY = 0;
		} else {
			ScrollY = ScrollY.Clamp(0, shiftedItemCount - pageCount);
		}
		int startLine = ScrollY.GreaterOrEquelThanZero();
		int ctrlID = 23186 + startLine * (Languages.Count + 1);
		var rect = new IRect(0, panelRect.yMax, panelRect.width / (Languages.Count + 1), itemHeight);
		string prevLabel = startLine - 1 >= 0 && startLine - 1 < Lines.Count ? Lines[startLine - 1].Label : string.Empty;
		bool searching = !string.IsNullOrEmpty(SearchingText) && GUI.TypingTextFieldID != SEARCH_ID;

		for (int i = startLine; i < Lines.Count; i++) {
			var line = Lines[i];
			rect.x = panelRect.x;

			// Outside Check
			if (rect.yMax < panelRect.y) break;

			// Searching Check
			if (searching && !line.Visible) continue;

			// Label Line
			string label = line.Label;
			if (label != prevLabel) {
				prevLabel = label;
				if (i != 0) {
					rect.height = labelHeight;
					rect.y -= labelHeight;
					GUI.Label(rect.Shrink(labelPadding, 0, 0, 0), label, Skin.SmallGreyLabel);
					rect.height = itemHeight;
				}
			}

			// New Line
			rect.y -= itemHeight;

			// Index
			GUI.IntLabel(
				rect.EdgeOutside(Direction4.Left, indexWidth).Shift(-itemSpaceX, 0),
				i, IndexLabelStyle
			);

			// Key
			var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
			line.Key = GUI.SmallInputField(ctrlID++, shrinkedRect, line.Key, out bool changed, out _);
			if (changed) {
				line.Label = Key_to_Label(line.Key);
				SetDirty();
			}
			rect.x += rect.width;
			bool emptyKey = string.IsNullOrEmpty(line.Key);

			// Empty Key Highlight
			if (emptyKey) {
				Renderer.DrawSlice(
					BuiltInSprite.FRAME_16,
					shrinkedRect, highlightBorder, highlightBorder, highlightBorder, highlightBorder,
					Color32.RED
				);
			}

			// Contents
			for (int j = 0; j < line.Value.Count; j++) {
				var shrinkedContentRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				string content = line.Value[j];
				line.Value[j] = GUI.SmallInputField(ctrlID++, shrinkedContentRect, content, out changed, out _);
				if (!emptyKey && string.IsNullOrEmpty(content)) {
					Renderer.DrawSlice(BuiltInSprite.FRAME_16, shrinkedContentRect, highlightBorder, highlightBorder, highlightBorder, highlightBorder, Color32.YELLOW);
				}
				if (changed) SetDirty();
				rect.x += rect.width;
			}

		}

		// Scrollbar
		ScrollY = GUI.ScrollBar(
			56093, WindowRect.ShrinkUp(Unify(42)).Edge(Direction4.Right, scrollBarWidth),
			ScrollY, shiftedItemCount, pageCount
		);
		if (Input.MouseWheelDelta != 0 && pageCount <= shiftedItemCount) {
			ScrollY -= Input.MouseWheelDelta * 4;
			ScrollY = ScrollY.Clamp(0, shiftedItemCount - pageCount);
		}

	}


	private void Update_ReloadWhenFileChanged () {
		if (CurrentProject == null) return;
		string lan = Languages[0];
		long currentDate = Util.GetFileModifyDate(
			Util.CombinePaths(CurrentProject.Universe.LanguageRoot, lan, $"{lan}.{AngePath.LANGUAGE_FILE_EXT}")
		);
		if (currentDate != ReloadCheckingDate) {
			// Reload
			Instance.Lines.RemoveAll(line => string.IsNullOrEmpty(line.Key));
			int oldLineCount = Lines.Count;
			RequireReloadWhenFileChangedFrame = -1;
			Load(CurrentProject.Universe.LanguageRoot);
			Instance.SetDirty();
			GenericDialogUI.SpawnDialog_Button(
				string.Format(MSG_LAN_CODE_ADDED, (Lines.Count - oldLineCount).GreaterOrEquelThanZero()),
				BuiltInText.UI_OK, Const.EmptyMethod
			);
		}
	}


	#endregion




	#region --- API ---


	public override void Save (bool forceSave = false) {
		if (CurrentProject == null) return;
		if (forceSave || IsDirty) CleanDirty();
		string currentRoot = CurrentProject.Universe.LanguageRoot;
		if (Lines.Count == 0 || string.IsNullOrEmpty(currentRoot)) return;
		var list = new List<KeyValuePair<string, string>>();
		for (int languageIndex = 0; languageIndex < Languages.Count; languageIndex++) {
			string lan = Languages[languageIndex];
			list.Clear();
			foreach (var data in Lines) {
				if (string.IsNullOrWhiteSpace(data.Key)) continue;
				list.Add(new(data.Key, data.Value[languageIndex]));
			}
			string lanFolderPath = LanguageUtil.GetLanguageFolderPath(currentRoot, lan);
			string lanFilePath = Util.CombinePaths(lanFolderPath, $"{lan}.{AngePath.LANGUAGE_FILE_EXT}");
			LanguageUtil.SaveAllPairsToDisk(lanFilePath, list);
		}
		RequireReloadWhenFileChangedFrame = -1;
#if DEBUG
		// Sync Project >> Engine
		if (CurrentProject.IsEngineInternalProject) {
			string from = CurrentProject.Universe.LanguageRoot;
			if (Util.FolderExists(from)) {
				string to = Universe.BuiltIn.LanguageRoot;
				Util.DeleteFolder(to);
				Util.CopyFolder(from, to, true, true, true);
			}
			Language.SetLanguage(Language.CurrentLanguage);
		}
#endif
	}


	#endregion




	#region --- LGC ---


	private void Load (string languageRoot) {

		RequireReloadWhenFileChangedFrame = -1;
		CleanDirty();
		if (!string.IsNullOrEmpty(languageRoot) && !Util.FolderExists(languageRoot)) return;
		Lines.Clear();

		// Load Language
		Languages.Clear();
		foreach (var path in Util.EnumerateFolders(languageRoot, true)) {
			string lan = Util.GetNameWithoutExtension(path);
			string filePath = Util.CombinePaths(path, $"{lan}.{AngePath.LANGUAGE_FILE_EXT}");
			if (Util.FileExists(filePath)) {
				Languages.Add(lan);
			}
		}
		if (Languages.Count == 0) Languages.Add("en");
		Languages.Sort();

		// Load Contents
		int count = Languages.Count;
		var pool = new Dictionary<string, int>();
		for (int languageIndex = 0; languageIndex < count; languageIndex++) {
			string lanName = Languages[languageIndex];
			string lanFolderPath = LanguageUtil.GetLanguageFolderPath(languageRoot, lanName);
			string lanFilePath = Util.CombinePaths(lanFolderPath, $"{lanName}.{AngePath.LANGUAGE_FILE_EXT}");
			foreach (var (key, value) in LanguageUtil.LoadAllPairsFromDiskAtPath(lanFilePath, keepEscapeCharacters: true)) {
				LanguageLine data;
				if (pool.TryGetValue(key, out int index)) {
					data = Lines[index];
				} else {
					data = new LanguageLine() {
						Key = key,
						Label = Key_to_Label(key),
						Value = new List<string>(new string[count].FillWithValue(string.Empty)),
					};
					pool.Add(key, Lines.Count);
					Lines.Add(data);
				}
				if (value.Length > data.Value[languageIndex].Length) {
					data.Value[languageIndex] = value;
				}
			}
		}

		// Sort
		Lines.Sort(LineComparer.Instance);
	}


	private void OpenAddLanguagePopup () {
		GenericPopupUI.BeginPopup();
		foreach (string lan in Util.ForAllSystemLanguages()) {
			string language = lan;
			int index = Languages.IndexOf(language);
			GenericPopupUI.AddItem(Util.GetLanguageDisplayName(language), () => {
				if (index >= 0) {
					// Delete Language
					ShowDeleteLanguageDialog(index);
				} else {
					// Add Language
					Languages.Add(language);
					Languages.Sort();
					int newIndex = Languages.IndexOf(language);
					foreach (var data in Lines) {
						data.Value.Insert(newIndex, string.Empty);
					}
					SetDirty();
				}
			}, true, index >= 0);
		}
	}


	private void ShowDeleteLanguageDialog (int index) {
		string lanName = Util.GetLanguageDisplayName(Languages[index]);
		GenericDialogUI.SpawnDialog_Button(
			string.Format(DELETE_MSG, lanName),
			BuiltInText.UI_DELETE,
			DeleteLanguage,
			BuiltInText.UI_CANCEL,
			Const.EmptyMethod
		);
		GenericDialogUI.SetCustomData(index);
		GenericDialogUI.SetItemTint(Skin.DeleteTint, Color32.GREY_245);
		// Func
		static void DeleteLanguage () {
			if (Instance == null || Instance.CurrentProject == null) return;
			if (GenericDialogUI.InvokingData is not int lanIndex) return;
			string targetRoot = Instance.CurrentProject.Universe.LanguageRoot;
			string lan = Instance.Languages[lanIndex];
			string lanFolderPath = LanguageUtil.GetLanguageFolderPath(targetRoot, lan);
			Util.DeleteFolder(lanFolderPath);
			Instance.Languages.RemoveAt(lanIndex);
			foreach (var data in Instance.Lines) {
				data.Value.RemoveAt(lanIndex);
			}
		}
	}


	private string Key_to_Label (string key) {
		if (string.IsNullOrWhiteSpace(key)) return string.Empty;
		int dot = key.IndexOf('.');
		if (dot < 0) return char.IsLetter(key[0]) || char.IsNumber(key[0]) ? key : key[0].ToString();
		return key[..dot];
	}


	#endregion




}
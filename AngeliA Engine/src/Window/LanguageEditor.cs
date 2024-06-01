using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

[RequireLanguageFromField]
public partial class LanguageEditor : WindowUI {




	#region --- SUB ---


	private class LanguageLine {
		public string Key;
		public string Label;
		public bool Required;
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
	private static readonly SpriteCode UI_TOOLBAR = "UI.ToolbarBackground";
	private static readonly LanguageCode DELETE_MSG = ("UI.LanguageEditor.DeleteMsg", "Delete Language {0}?");
	private static readonly LanguageCode ADD_KEY = ("UI.LanguageEditor.AddKey", "+ Key");
	private static readonly LanguageCode ADD_LANGUAGE = ("UI.LanguageEditor.AddLanguage", "+ Language");
	private static readonly LanguageCode REMOVE_EMPTY = ("UI.LanguageEditor.RemoveEmpty", "Remove Empty");
	private static readonly LanguageCode UI_LABEL_KEY = ("UI.LanguageEditor.Key", "Key");
	private static readonly LanguageCode MSG_HELP = ("UI.LanguageEditor.HelpMsg", "Empty keys will be deleted when open the project next time");
	private static readonly LanguageCode MSG_REMOVE_EMPTY = ("UI.LanguageEditor.RemoveEmptyMsg", "Remove all lines without any content? Lines with only a key will also be removed.");
	private const int SEARCH_ID = -19223;

	// Api
	public static LanguageEditor Instance { get; private set; }
	public string LanguageRoot { get; private set; } = "";
	public bool IgnoreRequirements { get; init; } = false;
	public override string DefaultName => "Language";

	// Data
	private readonly List<string> Languages = new();
	private readonly List<LanguageLine> Lines = new();
	private readonly IntToChars IndexToChars = new();
	private readonly GUIStyle IndexLabelStyle = new(GUI.Skin.SmallGreyLabel) { Alignment = Alignment.MidRight };
	private int ScrollY = 0;
	private string SearchingText = string.Empty;


	#endregion




	#region --- MSG ---


	public LanguageEditor (bool ignoreRequirements = false) {
		Instance = this;
		IgnoreRequirements = ignoreRequirements;
	}


	public void SetLanguageRoot (string newRoot) {
		if (newRoot == LanguageRoot) return;
		Load(newRoot);
		LanguageRoot = newRoot;
		ScrollY = 0;
		SearchingText = string.Empty;
	}


	public override void OnActivated () {
		base.OnActivated();
		ScrollY = 0;
		SearchingText = string.Empty;
	}


	public override void UpdateWindowUI () {

		if (string.IsNullOrEmpty(LanguageRoot)) return;

		Cursor.RequireCursor();

		var windowRect = WindowRect;
		int column = Languages.Count + 1;
		int fieldWidth = windowRect.width / column;

		windowRect.x += (windowRect.width - fieldWidth * column) / 2;
		windowRect.width = fieldWidth * column;
		X = windowRect.x;
		Y = windowRect.y;
		Width = windowRect.width;
		Height = windowRect.height;

		int frameThickness = Unify(2);
		Renderer.DrawSlice(
			BuiltInSprite.FRAME_16, windowRect.Expand(frameThickness),
			frameThickness, frameThickness, frameThickness, frameThickness,
			Color32.GREY_12, int.MinValue
		);

		if (Game.IsKeyboardKeyHolding(KeyboardKey.LeftCtrl) && Game.IsKeyboardKeyHolding(KeyboardKey.S)) {
			Save();
		}

		Update_Bar(windowRect.EdgeInside(Direction4.Up, Unify(84)));
		Update_Content(windowRect.EdgeInside(Direction4.Down, windowRect.height - Unify(84)));
	}


	private void Update_Bar (IRect panelRect) {

		// BG
		GUI.DrawSliceOrTile(UI_TOOLBAR, panelRect.TopHalf());

		// Shift Panel Rect
		int labelHeight = panelRect.height - Unify(42);
		panelRect.height = Unify(42);
		panelRect.y += labelHeight;

		// Line
		Renderer.DrawPixel(panelRect.EdgeInside(Direction4.Down, Unify(1)), Color32.GREY_12, 1);

		// + Key
		var rect = panelRect;
		rect.width = Unify(108);
		if (GUI.Button(rect, ADD_KEY, Skin.SmallCenterLabelButton)) {
			ScrollY = 0;
			Lines.Insert(0, new LanguageLine() {
				Key = string.Empty,
				Label = string.Empty,
				Required = false,
				Value = new List<string>(new string[Languages.Count].FillWithValue(string.Empty)),
			});
			SetDirty();
		}
		Cursor.SetCursorAsHand(rect);
		rect.SlideRight();

		// Line
		Renderer.DrawPixel(rect.EdgeOutside(Direction4.Left, Unify(1)), Color32.GREY_12, 2);

		// + Language
		rect.width = Unify(108);
		if (GUI.Button(rect, ADD_LANGUAGE, Skin.SmallCenterLabelButton)) {
			OpenAddLanguagePopup();
		}
		Cursor.SetCursorAsHand(rect);
		rect.SlideRight();

		// Line
		Renderer.DrawPixel(rect.EdgeOutside(Direction4.Left, Unify(1)), Color32.GREY_12, 2);

		// Remove Empty
		rect.width = Unify(128);
		if (GUI.Button(rect, REMOVE_EMPTY, Skin.SmallCenterLabelButton)) {
			GenericDialogUI.SpawnDialog_Button(
				MSG_REMOVE_EMPTY, BuiltInText.UI_DELETE, RemoveAllEmptyLines, BuiltInText.UI_CANCEL, Const.EmptyMethod
			);
		}
		Cursor.SetCursorAsHand(rect);
		rect.SlideRight();

		// Line
		Renderer.DrawPixel(rect.EdgeOutside(Direction4.Left, Unify(1)), Color32.GREY_12, 2);

		// Search
		rect.width = panelRect.xMax - rect.x;
		var searchRect = rect.Shrink(Unify(6)).MidHalf();
		SearchingText = GUI.SmallInputField(SEARCH_ID, searchRect, SearchingText, out _, out bool confirm);
		if (GUI.TypingTextFieldID != SEARCH_ID && string.IsNullOrEmpty(SearchingText)) {
			GUI.Icon(
				searchRect.EdgeInside(Direction4.Left, searchRect.height * 8 / 10).Shift(searchRect.height / 6, 0),
				BuiltInSprite.ICON_SEARCH
			);
		}
		if (confirm) {
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
		if (GUI.Button(panelRect.EdgeInside(Direction4.Right, Unify(36)), "?", Skin.SmallCenterLabelButton)) {
			GenericDialogUI.SpawnDialog_Button(MSG_HELP, BuiltInText.UI_OK, Const.EmptyMethod);
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
		static void RemoveAllEmptyLines () => Instance?.RemoveAllEmptyLines();
	}


	private void Update_Content (IRect panelRect) {

		int itemHeight = Unify(36);
		if (panelRect.height <= itemHeight) return;

		int scrollBarWidth = Unify(16);
		int labelHeight = Unify(22);
		int labelPadding = Unify(12);
		int itemSpaceX = Unify(5);
		int itemSpaceY = Unify(1);
		int panelPadding = Unify(64);
		int indexWidth = Unify(32);
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
			GUI.Label(
				rect.EdgeOutside(Direction4.Left, indexWidth).Shift(-itemSpaceX, 0),
				IndexToChars.GetChars(i), IndexLabelStyle
			);

			// Key
			if (!IgnoreRequirements && line.Required) {
				int _textIndex = Renderer.GetUsedCellCount();
				var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				GUI.Label(shrinkedRect, line.Key, Skin.SmallCenterLabel);
				Renderer.ClampCells(shrinkedRect, _textIndex);
				ctrlID++;
			} else {
				var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				line.Key = GUI.SmallInputField(ctrlID++, shrinkedRect, line.Key, out bool changed, out _);
				if (changed) {
					line.Label = Key_to_Label(line.Key);
					SetDirty();
				}
			}
			rect.x += rect.width;

			// Contents
			for (int j = 0; j < line.Value.Count; j++) {
				var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				line.Value[j] = GUI.SmallInputField(ctrlID++, shrinkedRect, line.Value[j], out bool changed, out _);
				if (changed) SetDirty();
				rect.x += rect.width;
			}

		}

		// Scrollbar
		ScrollY = GUI.ScrollBar(
			56093, WindowRect.ShrinkUp(Unify(42)).EdgeInside(Direction4.Right, scrollBarWidth),
			ScrollY, shiftedItemCount, pageCount
		);
		if (Input.MouseWheelDelta != 0 && pageCount <= shiftedItemCount) {
			ScrollY -= Input.MouseWheelDelta * 4;
			ScrollY = ScrollY.Clamp(0, shiftedItemCount - pageCount);
		}

	}


	#endregion




	#region --- API ---


	public override void Save (bool forceSave = false) {
		if (forceSave || IsDirty) IsDirty = false;
		if (Lines.Count == 0 || string.IsNullOrEmpty(LanguageRoot)) return;
		var list = new List<KeyValuePair<string, string>>();
		for (int languageIndex = 0; languageIndex < Languages.Count; languageIndex++) {
			string lan = Languages[languageIndex];
			list.Clear();
			foreach (var data in Lines) {
				if (string.IsNullOrWhiteSpace(data.Key)) continue;
				list.Add(new(data.Key, data.Value[languageIndex]));
			}
			LanguageUtil.SaveAllPairsToDisk(LanguageRoot, lan, list);
		}
	}


	#endregion




	#region --- LGC ---


	private void Load (string languageRoot) {

		IsDirty = false;
		if (!string.IsNullOrEmpty(languageRoot) && !Util.FolderExists(languageRoot)) return;
		Lines.Clear();

		// Load Language
		Languages.Clear();
		foreach (var path in Util.EnumerateFiles(languageRoot, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
			Languages.Add(Util.GetNameWithoutExtension(path));
		}
		if (Languages.Count == 0) Languages.Add("en");
		Languages.Sort();

		// Load Contents
		int count = Languages.Count;
		var pool = new Dictionary<string, int>();
		for (int languageIndex = 0; languageIndex < count; languageIndex++) {
			foreach (var (key, value) in LanguageUtil.LoadAllPairsFromDisk(languageRoot, Languages[languageIndex])) {
				LanguageLine data;
				if (pool.TryGetValue(key, out int index)) {
					data = Lines[index];
				} else {
					data = new LanguageLine() {
						Key = key,
						Label = Key_to_Label(key),
						Value = new List<string>(new string[count].FillWithValue(string.Empty)),
						Required = false,
					};
					pool.Add(key, Lines.Count);
					Lines.Add(data);
				}
				data.Value[languageIndex] = value;
			}
		}

		// Fill Missing Requirements
		if (!IgnoreRequirements)
			foreach (var requiredKey in FrameworkUtil.ForAllLanguageKeyRequirements()) {
				if (pool.TryGetValue(requiredKey, out int index)) {
					Lines[index].Required = true;
					continue;
				}
				pool.Add(requiredKey, Lines.Count);
				Lines.Add(new LanguageLine() {
					Key = requiredKey,
					Label = Key_to_Label(requiredKey),
					Value = new List<string>(new string[count].FillWithValue(string.Empty)),
					Required = true,
				});
				SetDirty();
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
		int lanIndex = index;
		string lanName = Util.GetLanguageDisplayName(Languages[lanIndex]);
		GenericDialogUI.SpawnDialog_Button(
			string.Format(DELETE_MSG, lanName),
			BuiltInText.UI_DELETE,
			() => {
				string targetRoot = AngePath.LanguageRoot;
				string path = LanguageUtil.GetLanguageFilePath(targetRoot, Languages[lanIndex]);
				Util.DeleteFile(path);
				Languages.RemoveAt(lanIndex);
				foreach (var data in Lines) {
					data.Value.RemoveAt(lanIndex);
				}
			},
			BuiltInText.UI_CANCEL,
			Const.EmptyMethod
		);
		GenericDialogUI.SetItemTint(Skin.DeleteTint, Color32.GREY_245);
	}


	private string Key_to_Label (string key) {
		if (string.IsNullOrWhiteSpace(key)) return string.Empty;
		int dot = key.IndexOf('.');
		if (dot < 0) return char.IsLetter(key[0]) || char.IsNumber(key[0]) ? key : key[0].ToString();
		return key[..dot];
	}


	private void RemoveAllEmptyLines () {
		ScrollY = 0;
		for (int i = 0; i < Lines.Count; i++) {
			var line = Lines[i];
			bool empty = true;
			foreach (var value in line.Value) {
				if (!string.IsNullOrWhiteSpace(value)) {
					empty = false;
					break;
				}
			}
			if (empty) {
				Lines.RemoveAt(i);
				i--;
			}
		}
		SetDirty();
	}


	#endregion




}
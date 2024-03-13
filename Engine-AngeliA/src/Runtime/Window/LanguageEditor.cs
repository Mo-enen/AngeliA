using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

[RequireLanguageFromField]
public partial class LanguageEditor : WindowUI {




	#region --- SUB ---


	private class LanguageLine {
		public string Key;
		public string Label;
		public bool Required;
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
	private static readonly LanguageCode DELETE_MSG = ("UI.LanguageEditor.DeleteMsg", "Delete Language {0}?");
	private static readonly LanguageCode ADD_KEY = ("UI.LanguageEditor.AddKey", "+ Key");
	private static readonly LanguageCode ADD_LANGUAGE = ("UI.LanguageEditor.AddLanguage", "+ Language");
	private static readonly LanguageCode UI_LABEL_KEY = ("UI.LanguageEditor.Key", "Key");

	// Api
	public static LanguageEditor Instance { get; private set; }
	public static bool IsActived => Instance != null && Instance.Active;
	public string LanguageRoot { get; private set; } = "";

	// Data
	private readonly List<string> Languages = new();
	private readonly List<LanguageLine> Lines = new();
	private int ScrollY = 0;
	private string SearchingText = string.Empty;
	private string LoadedLanguageRoot = "";
	private bool IsDirty = false;


	#endregion




	#region --- MSG ---


	public LanguageEditor () => Instance = this;


	public void SetLanguageRoot (string newRoot) {
		if (newRoot == LanguageRoot) return;
		Save();
		LanguageRoot = newRoot;
		OnActivated();
	}


	public override void OnActivated () {
		base.OnActivated();
		if (LanguageRoot != LoadedLanguageRoot) {
			LoadedLanguageRoot = LanguageRoot;
			Load(LanguageRoot);
		}
		ScrollY = 0;
		SearchingText = string.Empty;
	}


	public override void OnInactivated () {
		base.OnInactivated();
		Save();
	}


	public override void UpdateWindowUI () {

		if (string.IsNullOrEmpty(LanguageRoot)) return;

		Cursor.RequireCursor();

		int padding = Unify(32);
		var cameraRect = WindowRect.Shrink(padding, padding, 0, 0);
		int column = Languages.Count + 1;
		int fieldWidth = Util.Clamp(cameraRect.width / column, 0, Unify(300));
		int verticalPadding = Unify(24);

		cameraRect.x += (cameraRect.width - fieldWidth * column) / 2;
		cameraRect.width = fieldWidth * column;
		cameraRect.y += verticalPadding;
		cameraRect.height -= verticalPadding * 2;
		X = cameraRect.x;
		Y = cameraRect.y;
		Width = cameraRect.width;
		Height = cameraRect.height;

		int frameThickness = Unify(2);
		Renderer.Draw_9Slice(
			BuiltInSprite.FRAME_16, cameraRect.Expand(frameThickness),
			frameThickness, frameThickness, frameThickness, frameThickness,
			Color32.GREY_12, int.MinValue
		);

		Update_Bar(cameraRect.EdgeInside(Direction4.Up, Unify(84)));
		Update_Content(cameraRect.EdgeInside(Direction4.Down, cameraRect.height - Unify(84)));
	}


	private void Update_Bar (IRect panelRect) {

		// BG
		Renderer.Draw(Const.PIXEL, panelRect, Color32.GREY_32, 0);

		// Shift Panel Rect
		int labelWidth = (panelRect.width - Unify(24)) / (Languages.Count + 1);
		int labelHeight = panelRect.height - Unify(42);
		panelRect.height = Unify(42);
		panelRect.y += labelHeight;

		// Line
		Renderer.Draw(Const.PIXEL, panelRect.EdgeInside(Direction4.Down, Unify(1.5f)), Color32.GREY_12, 1);

		// + Key
		var rect = panelRect;
		rect.width = Unify(108);
		if (GUI.Button(rect, ADD_KEY, GUISkin.Label)) {
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
		rect.x += rect.width;

		// Line
		Renderer.Draw(Const.PIXEL, rect.EdgeOutside(Direction4.Left, Unify(1.5f)), Color32.GREY_12, 2);

		// + Language
		rect.width = Unify(108);
		if (GUI.Button(rect, ADD_LANGUAGE, GUISkin.Label)) {
			OpenAddLanguagePopup();
		}
		Cursor.SetCursorAsHand(rect);
		rect.x += rect.width;

		// Line
		Renderer.Draw(Const.PIXEL, rect.EdgeOutside(Direction4.Left, Unify(1.5f)), Color32.GREY_12, 2);

		// Search
		rect.width = panelRect.xMax - rect.x;
		var searchRect = rect.Shrink(Unify(6));
		SearchingText = GUI.InputField(-19223, searchRect, SearchingText);

		// Labels
		var labelRect = new IRect(panelRect.x + Unify(12), panelRect.y - labelHeight, labelWidth, labelHeight);
		GUI.Label(labelRect, UI_LABEL_KEY);
		labelRect.x += labelRect.width;
		for (int i = 0; i < Languages.Count; i++) {
			string name = Util.GetLanguageDisplayName(Languages[i]);
			GUI.Label(labelRect, name);
			labelRect.x += labelRect.width;
		}

	}


	private void Update_Content (IRect panelRect) {

		Renderer.Draw(Const.PIXEL, panelRect, Color32.GREY_32, 0);

		int scrollBarWidth = Unify(24);
		int itemHeight = Unify(30);
		int labelHeight = Unify(22);
		int labelPadding = Unify(12);
		int itemSpaceX = Unify(5);
		int itemSpaceY = Unify(1);
		panelRect = panelRect.Shrink(0, scrollBarWidth, 0, 0);
		int pageCount = panelRect.height.CeilDivide(itemHeight);
		int shiftedItemCount = Lines.Count + 6;
		if (pageCount > shiftedItemCount) {
			ScrollY = 0;
		} else {
			ScrollY = ScrollY.Clamp(0, shiftedItemCount - pageCount);
		}
		int startLine = ScrollY;
		int ctrlID = 23186 + startLine * (Languages.Count + 1);
		var rect = new IRect(0, panelRect.yMax, panelRect.width / (Languages.Count + 1), itemHeight);
		int startCellIndex = Renderer.GetUsedCellCount();
		int startTextCellIndex = Renderer.GetTextUsedCellCount();
		string prevLabel = startLine - 1 >= 0 ? Lines[startLine - 1].Label : string.Empty;
		bool searching = !string.IsNullOrEmpty(SearchingText);
		var OIC = System.StringComparison.OrdinalIgnoreCase;

		for (int i = startLine; i < Lines.Count; i++) {
			var line = Lines[i];
			rect.x = panelRect.x;

			// Outside Check
			if (rect.yMax < panelRect.y) break;

			// Searching Check
			if (searching && !line.Key.Contains(SearchingText, OIC)) {
				bool founded = false;
				foreach (var value in line.Value) {
					if (value.Contains(SearchingText, OIC)) {
						founded = true;
						break;
					}
				}
				if (!founded) continue;
			}

			// Label Line
			string label = line.Label;
			if (label != prevLabel) {
				prevLabel = label;
				if (i != 0) {
					rect.height = labelHeight;
					rect.y -= labelHeight;
					GUI.Label(rect.Shrink(labelPadding, 0, 0, 0), label);
					rect.height = itemHeight;
				}
			}

			// New Line
			rect.y -= itemHeight;

			// Key
			if (line.Required) {
				int _textIndex = Renderer.GetTextUsedCellCount();
				var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				GUI.Label(shrinkedRect, line.Key, GUISkin.CenterSmallLabel);
				Renderer.ClampTextCells(shrinkedRect, _textIndex);
				ctrlID++;
			} else {
				var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				line.Key = GUI.InputField(ctrlID++, shrinkedRect, line.Key, out bool changed, out _);
				if (changed) {
					line.Label = Key_to_Label(line.Key);
					SetDirty();
				}
			}
			rect.x += rect.width;

			// Contents
			for (int j = 0; j < line.Value.Count; j++) {
				var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				line.Value[j] = GUI.InputField(ctrlID++, shrinkedRect, line.Value[j], out bool changed, out _);
				if (changed) SetDirty();
				rect.x += rect.width;
			}

		}

		// Clamp
		Renderer.ClampCells(panelRect, startCellIndex);
		Renderer.ClampTextCells(panelRect, startTextCellIndex);

		// Scrollbar
		ScrollY = GUI.ScrollBar(
			56093, panelRect.EdgeOutside(Direction4.Right, scrollBarWidth),
			ScrollY, shiftedItemCount, pageCount
		);
		if (Input.MouseWheelDelta != 0 && pageCount <= shiftedItemCount) {
			ScrollY -= Input.MouseWheelDelta * 4;
			ScrollY = ScrollY.Clamp(0, shiftedItemCount - pageCount);
		}

	}


	#endregion




	#region --- API ---


	public void SetDirty (bool dirty = true) => IsDirty = dirty;


	public void Save (bool forceSave = false) {
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
		if (!string.IsNullOrEmpty(languageRoot) && !Util.FolderExists(languageRoot)) return;
		SetDirty(false);
		Lines.Clear();
		int count = Language.LanguageCount;
		// Load Language
		Languages.Clear();
		foreach (var path in Util.EnumerateFiles(languageRoot, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
			Languages.Add(Util.GetNameWithoutExtension(path));
		}
		Languages.Sort();
		// Load Contents
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
		GenericDialogUI.SpawnDialog(
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
	}


	private string Key_to_Label (string key) {
		if (string.IsNullOrWhiteSpace(key)) return string.Empty;
		int dot = key.IndexOf('.');
		if (dot < 0) return char.IsLetter(key[0]) || char.IsNumber(key[0]) ? key : key[0].ToString();
		return key[..dot];
	}


	#endregion




}
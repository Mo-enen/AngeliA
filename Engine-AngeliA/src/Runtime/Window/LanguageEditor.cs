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
	public string LanguageRoot { get; set; } = "";

	// Data
	private readonly List<string> Languages = new();
	private readonly List<LanguageLine> Lines = new();
	private readonly TextContent KeyLabelContent = new() { Alignment = Alignment.MidLeft, CharSize = 22, Tint = Color32.GREY_216, };
	private readonly TextContent LabelContent = new() { Alignment = Alignment.MidLeft, CharSize = 15, Tint = Color32.GREY_96, };
	private readonly TextContent InputContent = new() { Alignment = Alignment.MidLeft, CharSize = 22, Tint = Color32.GREY_216, };
	private bool IsDirty = false;
	private int ScrollY = 0;
	private string SearchingText = string.Empty;


	#endregion




	#region --- MSG ---


	public LanguageEditor () => Instance = this;


	public override void OnActivated () {
		base.OnActivated();
		if (string.IsNullOrEmpty(LanguageRoot)) {
			Active = false;
			return;
		}
		LoadFromDisk(LanguageRoot);
		ScrollY = 0;
		SearchingText = string.Empty;
		Task.AddToLast(EntityHookTask.TYPE_ID, this);
	}


	public override void OnInactivated () {
		base.OnInactivated();
		if (IsDirty) SaveToDisk();
		Lines.Clear();
		Languages.Clear();
		Language.SetLanguage(Language.CurrentLanguage);
	}


	public override void UpdateWindowUI () {

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

		bool interactable = true;
		Update_Bar(cameraRect.EdgeInside(Direction4.Up, Unify(84)), interactable);
		Update_Content(cameraRect.EdgeInside(Direction4.Down, cameraRect.height - Unify(84)), interactable);
	}


	private void Update_Bar (IRect panelRect, bool interactable) {

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
		if (GUI.Button(rect, ADD_KEY, z: 1, charSize: 16) && interactable) {
			ScrollY = 0;
			Lines.Insert(0, new LanguageLine() {
				Key = string.Empty,
				Label = string.Empty,
				Required = false,
				Value = new List<string>(new string[Languages.Count].FillWithValue(string.Empty)),
			});
			IsDirty = true;
		}
		Cursor.SetCursorAsHand(rect);
		rect.x += rect.width;

		// Line
		Renderer.Draw(Const.PIXEL, rect.EdgeOutside(Direction4.Left, Unify(1.5f)), Color32.GREY_12, 2);

		// + Language
		rect.width = Unify(108);
		if (GUI.Button(rect, ADD_LANGUAGE, z: 1, charSize: 16) && interactable) {
			OpenAddLanguagePopup();
		}
		Cursor.SetCursorAsHand(rect);
		rect.x += rect.width;

		// Line
		Renderer.Draw(Const.PIXEL, rect.EdgeOutside(Direction4.Left, Unify(1.5f)), Color32.GREY_12, 2);

		// Search
		int border = Unify(1);
		rect.width = panelRect.xMax - rect.x;
		var searchRect = rect.Shrink(Unify(6));
		string newText = GUI.TextField(-19223, searchRect, InputContent.SetText(SearchingText));
		SearchingText = interactable ? newText : SearchingText;
		Renderer.Draw_9Slice(
			BuiltInSprite.FRAME_16, searchRect,
			border, border, border, border,
Color32.GREY_128, 1
		);

		// Labels
		var labelRect = new IRect(panelRect.x + Unify(12), panelRect.y - labelHeight, labelWidth, labelHeight);
		GUI.Label(LabelContent.SetText(UI_LABEL_KEY), labelRect);
		labelRect.x += labelRect.width;
		for (int i = 0; i < Languages.Count; i++) {
			string name = Util.GetLanguageDisplayName(Languages[i]);
			GUI.Label(LabelContent.SetText(name), labelRect);
			labelRect.x += labelRect.width;
		}

	}


	private void Update_Content (IRect panelRect, bool interactable) {

		Renderer.Draw(Const.PIXEL, panelRect, Color32.GREY_32, 0);

		int scrollBarWidth = Unify(24);
		int itemHeight = Unify(30);
		int labelHeight = Unify(22);
		int labelPadding = Unify(12);
		int itemSpaceX = Unify(5);
		int itemSpaceY = Unify(1);
		int itemBorder = Unify(1);
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
					GUI.Label(LabelContent.SetText(label), rect.Shrink(labelPadding, 0, 0, 0));
					rect.height = itemHeight;
				}
			}

			// New Line
			rect.y -= itemHeight;

			// Key
			if (line.Required) {
				int _textIndex = Renderer.GetTextUsedCellCount();
				var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				GUI.Label(KeyLabelContent.SetText(line.Key), shrinkedRect);
				Renderer.ClampTextCells(shrinkedRect, _textIndex);
				ctrlID++;
			} else {
				var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				line.Key = GUI.TextField(
					ctrlID++, shrinkedRect, InputContent.SetText(line.Key), out bool changed, out _
				);
				Renderer.Draw_9Slice(
					BuiltInSprite.FRAME_16, shrinkedRect,
					itemBorder, itemBorder, itemBorder, itemBorder,
Color32.GREY_128, 1
				);
				if (changed) {
					line.Label = Key_to_Label(line.Key);
					IsDirty = true;
				}
			}
			rect.x += rect.width;

			// Contents
			for (int j = 0; j < line.Value.Count; j++) {
				var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
				line.Value[j] = GUI.TextField(
					ctrlID++, shrinkedRect, InputContent.SetText(line.Value[j]), out bool changed, out _
				);
				Renderer.Draw_9Slice(
					BuiltInSprite.FRAME_16, shrinkedRect,
					itemBorder, itemBorder, itemBorder, itemBorder,
Color32.GREY_128, 1
				);
				if (changed) IsDirty = true;
				rect.x += rect.width;
			}

		}

		// Clamp
		Renderer.ClampCells(panelRect, startCellIndex);
		Renderer.ClampTextCells(panelRect, startTextCellIndex);

		// Scrollbar
		ScrollY = interactable ? GUI.ScrollBar(
			56093, panelRect.EdgeOutside(Direction4.Right, scrollBarWidth), z: 1,
			ScrollY, shiftedItemCount, pageCount
		) : ScrollY;
		if (Input.MouseWheelDelta != 0 && pageCount <= shiftedItemCount) {
			ScrollY -= Input.MouseWheelDelta * 4;
			ScrollY = ScrollY.Clamp(0, shiftedItemCount - pageCount);
		}

	}


	#endregion




	#region --- LGC ---


	private void LoadFromDisk (string languageRoot) {
		IsDirty = false;
		Lines.Clear();
		string targetRoot = languageRoot;
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
			foreach (var (key, value) in LanguageUtil.LoadAllPairsFromDisk(targetRoot, Languages[languageIndex])) {
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
			IsDirty = true;
		}
		// Sort
		Lines.Sort(LineComparer.Instance);
	}


	private void SaveToDisk () {
		IsDirty = false;
		string targetRoot = AngePath.LanguageRoot;
		var list = new List<KeyValuePair<string, string>>();
		for (int languageIndex = 0; languageIndex < Languages.Count; languageIndex++) {
			string lan = Languages[languageIndex];
			list.Clear();
			foreach (var data in Lines) {
				if (string.IsNullOrWhiteSpace(data.Key)) continue;
				list.Add(new(data.Key, data.Value[languageIndex]));
			}
			LanguageUtil.SaveAllPairsToDisk(targetRoot, lan, list);
		}
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
					IsDirty = true;
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
using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[RequireLanguageFromField]
	public partial class LanguageEditor : GlobalEditorUI {




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
		private static readonly LanguageCode DELETE_MSG = "UI.LanguageEditor.DeleteMsg";
		private static readonly LanguageCode ADD_KEY = "UI.LanguageEditor.AddKey";
		private static readonly LanguageCode ADD_LANGUAGE = "UI.LanguageEditor.AddLanguage";
		private static readonly LanguageCode UI_LABEL_KEY = "UI.LanguageEditor.Key";
		private static readonly LanguageCode UI_DELETE = "UI.Delete";
		private static readonly LanguageCode UI_CANCEL = "UI.Cancel";

		// Api
		public new static LanguageEditor Instance => GlobalEditorUI.Instance as LanguageEditor;
		public static bool IsActived => Instance != null && Instance.Active;

		// Data
		private readonly List<string> Languages = new();
		private readonly List<LanguageLine> Lines = new();
		private readonly CellContent KeyLabelContent = new() { Alignment = Alignment.MidLeft, CharSize = 22, Tint = Const.GREY_216, };
		private readonly CellContent LabelContent = new() { Alignment = Alignment.MidLeft, CharSize = 15, Tint = Const.GREY_96, };
		private readonly CellContent InputContent = new() { Alignment = Alignment.MidLeft, CharSize = 22, Tint = Const.GREY_216, };
		private bool IsDirty = false;
		private int ScrollY = 0;
		private string SearchingText = string.Empty;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			LoadFromDisk();
			ScrollY = 0;
		}


		public override void OnInactivated () {
			base.OnInactivated();
			if (IsDirty) SaveToDisk();
			Lines.Clear();
			Languages.Clear();
		}


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			CursorSystem.RequireCursor();
		}


		public override void UpdateUI () {
			base.UpdateUI();
			int padding = Unify(32);
			var cameraRect = CellRenderer.CameraRect.Shrink(padding, padding, 0, 0);
			int column = Languages.Count + 1;
			int fieldWidth = Util.Clamp(cameraRect.width / column, 0, Unify(300));
			cameraRect.x += (cameraRect.width - fieldWidth * column) / 2;
			cameraRect.width = fieldWidth * column;
			bool interactable = !GenericPopupUI.ShowingPopup && !GenericDialogUI.ShowingDialog;
			Update_Bar(cameraRect.EdgeInside(Direction4.Up, Unify(84)), interactable);
			Update_Content(cameraRect.EdgeInside(Direction4.Down, CellRenderer.CameraRect.height - Unify(84)), interactable);
		}


		private void Update_Bar (IRect panelRect, bool interactable) {

			// BG
			CellRenderer.Draw(Const.PIXEL, panelRect, Const.GREY_32, 0);

			// Shift Panel Rect
			int labelWidth = (panelRect.width - Unify(24)) / (Languages.Count + 1);
			int labelHeight = panelRect.height - Unify(42);
			panelRect.height = Unify(42);
			panelRect.y += labelHeight;

			// Line
			CellRenderer.Draw(Const.PIXEL, panelRect.EdgeInside(Direction4.Down, Unify(1.5f)), Const.GREY_12, 1);

			// + Key
			var rect = panelRect;
			rect.width = Unify(96);
			if (CellRendererGUI.Button(rect, Language.Get(ADD_KEY, "+ Key"), 1) && interactable) {
				Lines.Insert(0, new LanguageLine() {
					Key = string.Empty,
					Label = string.Empty,
					Required = false,
					Value = new List<string>(new string[Languages.Count].FillWithValue(string.Empty)),
				});
				IsDirty = true;
			}
			CursorSystem.SetCursorAsHand(rect);
			rect.x += rect.width;

			// Line
			CellRenderer.Draw(Const.PIXEL, rect.EdgeOutside(Direction4.Left, Unify(1.5f)), Const.GREY_12, 2);

			// + Language
			rect.width = Unify(96);
			if (CellRendererGUI.Button(rect, Language.Get(ADD_LANGUAGE, "+ Language"), 1) && interactable) {
				OpenAddLanguagePopup();
			}
			CursorSystem.SetCursorAsHand(rect);
			rect.x += rect.width;

			// Line
			CellRenderer.Draw(Const.PIXEL, rect.EdgeOutside(Direction4.Left, Unify(1.5f)), Const.GREY_12, 2);

			// Search
			int border = Unify(1);
			rect.width = panelRect.xMax - rect.x;
			var searchRect = rect.Shrink(Unify(6));
			string newText = CellRendererGUI.TextField(-19223, searchRect, InputContent.SetText(SearchingText));
			SearchingText = interactable ? newText : SearchingText;
			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_16, searchRect,
				border, border, border, border,
				Const.GREY_128, 1
			);

			// Labels
			var labelRect = new IRect(panelRect.x + Unify(12), panelRect.y - labelHeight, labelWidth, labelHeight);
			CellRendererGUI.Label(LabelContent.SetText(Language.Get(UI_LABEL_KEY, "Key")), labelRect);
			labelRect.x += labelRect.width;
			for (int i = 0; i < Languages.Count; i++) {
				string name = Util.GetLanguageDisplayName(Languages[i]);
				CellRendererGUI.Label(LabelContent.SetText(name), labelRect);
				labelRect.x += labelRect.width;
			}

		}


		private void Update_Content (IRect panelRect, bool interactable) {

			CellRenderer.Draw(Const.PIXEL, panelRect, Const.GREY_32, 0);

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
			int startCellIndex = CellRenderer.GetUsedCellCount();
			int startTextCellIndex = CellRenderer.GetTextUsedCellCount();
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
						CellRendererGUI.Label(LabelContent.SetText(label), rect.Shrink(labelPadding, 0, 0, 0));
						rect.height = itemHeight;
					}
				}

				// New Line
				rect.y -= itemHeight;

				// Key
				if (line.Required) {
					int _textIndex = CellRenderer.GetTextUsedCellCount();
					var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
					CellRendererGUI.Label(KeyLabelContent.SetText(line.Key), shrinkedRect);
					CellRenderer.ClampTextCells(shrinkedRect, _textIndex);
					ctrlID++;
				} else {
					var shrinkedRect = rect.Shrink(itemSpaceX, itemSpaceX, itemSpaceY, itemSpaceY);
					line.Key = CellRendererGUI.TextField(
						ctrlID++, shrinkedRect, InputContent.SetText(line.Key), out bool changed
					);
					CellRenderer.Draw_9Slice(
						BuiltInIcon.FRAME_16, shrinkedRect,
						itemBorder, itemBorder, itemBorder, itemBorder,
						Const.GREY_128, 1
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
					line.Value[j] = CellRendererGUI.TextField(
						ctrlID++, shrinkedRect, InputContent.SetText(line.Value[j]), out bool changed
					);
					CellRenderer.Draw_9Slice(
						BuiltInIcon.FRAME_16, shrinkedRect,
						itemBorder, itemBorder, itemBorder, itemBorder,
						Const.GREY_128, 1
					);
					if (changed) IsDirty = true;
					rect.x += rect.width;
				}

			}

			// Clamp
			CellRenderer.ClampCells(panelRect, startCellIndex);
			CellRenderer.ClampTextCells(panelRect, startTextCellIndex);

			// Scrollbar
			ScrollY = interactable ? CellRendererGUI.ScrollBar(
				panelRect.EdgeOutside(Direction4.Right, scrollBarWidth), z: 1,
				ScrollY, shiftedItemCount, pageCount
			) : ScrollY;
			if (FrameInput.MouseWheelDelta != 0 && pageCount <= shiftedItemCount) {
				ScrollY -= FrameInput.MouseWheelDelta * 4;
				ScrollY = ScrollY.Clamp(0, shiftedItemCount - pageCount);
			}

		}


		#endregion




		#region --- LGC ---


		private void LoadFromDisk () {
			IsDirty = false;
			Lines.Clear();
			string targetRoot = Game.IsEdittime ? AngePath.BuiltInLanguageRoot : AngePath.UserLanguageRoot;
			int count = Game.IsEdittime ? Language.BuiltInLanguageCount : Language.UserLanguageCount;
			// Load Language
			for (int languageIndex = 0; languageIndex < count; languageIndex++) {
				Languages.Add(Game.IsEdittime ? Language.GetBuiltInLanguageAt(languageIndex) : Language.GetUserLanguageAt(languageIndex));
			}
			Languages.Sort();
			// Load Contents
			var pool = new Dictionary<string, int>();
			for (int languageIndex = 0; languageIndex < count; languageIndex++) {
				foreach (var (key, value) in Language.LoadAllPairsFromDisk(targetRoot, Languages[languageIndex])) {
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
			foreach (var requiredKey in AngeUtil.ForAllLanguageKeyRequirements()) {
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
			string targetRoot = Game.IsEdittime ? AngePath.BuiltInLanguageRoot : AngePath.UserLanguageRoot;
			var list = new List<KeyValuePair<string, string>>();
			for (int languageIndex = 0; languageIndex < Languages.Count; languageIndex++) {
				string lan = Languages[languageIndex];
				list.Clear();
				foreach (var data in Lines) {
					if (string.IsNullOrWhiteSpace(data.Key)) continue;
					list.Add(new(data.Key, data.Value[languageIndex]));
				}
				Language.SaveAllPairsToDisk(targetRoot, lan, list);
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
				string.Format(Language.Get(DELETE_MSG, "Delete Language {0}?"), lanName),
				Language.Get(UI_DELETE, "Delete"),
				() => {
					string targetRoot = Game.IsEdittime ? AngePath.BuiltInLanguageRoot : AngePath.UserLanguageRoot;
					string path = Language.GetLanguageFilePath(targetRoot, Languages[lanIndex]);
					Util.DeleteFile(path);
					Languages.RemoveAt(lanIndex);
					foreach (var data in Lines) {
						data.Value.RemoveAt(lanIndex);
					}
				},
				Language.Get(UI_CANCEL, "Cancel"),
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
}
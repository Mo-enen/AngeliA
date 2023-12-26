using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using AngeliaFramework;


namespace AngeliaForUnity.Editor {
	public class LanguageEditor : UtilWindow {



		// SUB
		private class WindowStyle {
			public GUIContent HelpContent = EditorGUIUtility.IconContent("_Help");
		}


		// Data
		private readonly List<bool> KeyVisibility = new();
		private readonly List<string> Keys = new();
		private readonly List<string> Languages = new();
		private WindowStyle Style = null;
		private string[,] Contents = null;
		private bool IsDirty = false;
		private bool Loaded = false;
		private string SearchingText = "";
		private string CurrentTag = "Main";


		// MSG
		[MenuItem("AngeliA/Language Editor", false, 22)]
		public static void OpenWindow () {
			var window = GetWindow<LanguageEditor>(true, "Language", true);
			window.minSize = new Vector2(512, 256);
			window.maxSize = new Vector2(1600, 1024);
		}


		private void OnEnable () {
			try {
				CurrentTag = "Main";
				SearchingText = "";
				InitMainTag();
				Loaded = Load();
			} catch (System.Exception ex) {
				Loaded = false;
				Debug.LogException(ex);
			}
		}


		protected override void BeforeWindowGUI () {
			base.BeforeWindowGUI();
			if (!Loaded) {
				Close();
				return;
			}
			Style ??= new();
			GUI_Bar();
		}


		protected override void OnWindowGUI () {
			if (!Loaded) {
				Close();
				return;
			}
			if (Languages.Count == 0) {
				MGUI.Space(16);
				EditorGUILayout.HelpBox("No language file.\nClick \"+ Language\" button to add one.", MessageType.Warning, true);
			} else if (Keys.Count == 0) {
				MGUI.Space(16);
				EditorGUILayout.HelpBox("No keys.\nClick \"+ Key\" button to add one.", MessageType.Warning, true);
			} else {
				GUI_Content();
			}
		}


		private void GUI_Bar () {

			using var _ = new GUILayout.HorizontalScope(EditorStyles.toolbar);
			const int HEIGHT = 20;

			// Create Key
			bool oldE = GUI.enabled;
			GUI.enabled = Languages.Count > 0;
			if (GUI.Button(MGUI.Rect(84, HEIGHT).Expand(1, 0, 0, 0), "+ Key", EditorStyles.toolbarButton)) {
				GUI.FocusControl("");
				SetScrollY(0f);
				SearchingText = "";
				RefreshKeyVisibility();
				EditorApplication.delayCall += () => {
					Keys.Insert(0, "");
					int len0 = Contents.GetLength(0);
					int len1 = Contents.GetLength(1);
					var newContent = new string[len0, len1 + 1];
					for (int i = 0; i < len0; i++) {
						newContent[i, 0] = "";
						for (int j = 1; j <= len1; j++) {
							newContent[i, j] = Contents[i, j - 1];
						}
					}
					Contents = newContent;
					Repaint();
				};
			}
			if (GUI.enabled) {
				EditorGUIUtility.AddCursorRect(MGUI.LastRect(), MouseCursor.Link);
			}
			GUI.enabled = oldE;

			// Help
			if (GUI.Button(MGUI.Rect(26, HEIGHT), Style.HelpContent, EditorStyles.toolbarButton)) {
				EditorUtil.Dialog("Help", "Make key empty to delete a line. It will be delete when you close the language editor.", "OK");
			}
			EditorGUIUtility.AddCursorRect(MGUI.LastRect(), MouseCursor.Link);

			// Search
			MGUI.Space(8);
			string newSearchingText = EditorGUI.DelayedTextField(MGUI.Rect(0, 22).Shift(0, 2), SearchingText, EditorStyles.toolbarSearchField);
			if (SearchingText != newSearchingText) {
				SearchingText = newSearchingText;
				RefreshKeyVisibility();
			}
			MGUI.Space(8);

			// Tag
			if (GUI.Button(MGUI.Rect(90, HEIGHT).Expand(0, 1, 0, 0), $" {CurrentTag}", EditorStyles.toolbarPopup)) {
				ShowTagMenu();
			}
			EditorGUIUtility.AddCursorRect(MGUI.LastRect(), MouseCursor.Link);

		}


		private void GUI_Content () {

			if (Keys.Count == 0 || Languages.Count == 0) return;

			// Labels
			using (new GUILayout.HorizontalScope()) {
				GUI.Label(MGUI.Rect(0, 18), "key", MGUI.MiniGreyLabel);
				MGUI.Space(2);
				for (int lanIndex = 0; lanIndex < Languages.Count; lanIndex++) {
					GUI.Label(
						MGUI.Rect(0, 18),
						$"<color=#CC9900>{Languages[lanIndex]}</color>",
						MGUI.RichMiniGreyLabel
					);
					MGUI.Space(2);
				}
			}
			MGUI.Space(2);

			// Contents
			string prevLabel = "";
			using var change = new EditorGUI.ChangeCheckScope();
			for (int keyIndex = 0; keyIndex < Keys.Count; keyIndex++) {
				string oldKey = Keys[keyIndex];
				if (keyIndex >= 0 && keyIndex < KeyVisibility.Count && !KeyVisibility[keyIndex]) continue;
				// Mini Label
				string label = oldKey;
				int dotIndex = label.IndexOf('.');
				if (dotIndex > 0) {
					label = label[..dotIndex];
				} else if (label.Length > 0) {
					label = label[..1];
				}
				if (label != prevLabel) {
					prevLabel = label;
					if (keyIndex > 0) {
						GUI.Label(MGUI.Rect(0, 18), label, MGUI.MiniGreyLabel);
					}
				}
				using (new GUILayout.HorizontalScope()) {
					// Key
					//string newKey = EditorGUI.DelayedTextField(MGUI.Rect(0, 18), oldKey);
					string newKey = EditorGUI.TextField(MGUI.Rect(0, 18), oldKey);
					if (!newKey.Equals(oldKey)) {
						newKey = newKey.Replace(":", "");
						if (string.IsNullOrEmpty(newKey) || !Keys.Contains(newKey)) {
							Keys[keyIndex] = newKey;
						}
					}
					MGUI.Space(2);
					// Languages
					for (int lanIndex = 0; lanIndex < Languages.Count; lanIndex++) {
						Contents[lanIndex, keyIndex] = EditorGUI.TextField(
							MGUI.Rect(0, 18), Contents[lanIndex, keyIndex]
						);
						MGUI.Space(2);
					}
				}
				MGUI.Space(2);
			}
			if (change.changed) IsDirty = true;

			MGUI.Space(64);

		}


		private void OnDestroy () {
			if (Loaded && IsDirty) {
				Save();
			}
		}


		protected override void OnLostFocus () { }


		private bool Load () {

			IsDirty = false;
			if (string.IsNullOrEmpty(CurrentTag)) return false;

			// Get Keys
			KeyVisibility.Clear();
			Keys.Clear();
			Languages.Clear();
			var keyMap = new Dictionary<string, int>();
			foreach (var folderPath in Util.EnumerateFolders(AngePath.LanguageRoot, true, "*")) {
				// Add Language
				Languages.Add(Util.GetNameWithoutExtension(folderPath));
				// Add Keys
				string filePath = Util.CombinePaths(folderPath, $"{CurrentTag}.{AngePath.LANGUAGE_FILE_EXT}");
				if (!Util.FileExists(filePath)) continue;
				string key;
				foreach (var line in Util.ForAllLines(filePath)) {
					int colon = line.IndexOf(':');
					if (colon <= 0) continue;
					key = line[..colon];
					if (!keyMap.ContainsKey(key)) {
						keyMap.Add(key, -1);
						Keys.Add(key);
					}
				}
			}
			Keys.Sort();
			for (int i = 0; i < Keys.Count; i++) keyMap[Keys[i]] = i;

			// Get Contents
			Contents = new string[Languages.Count, Keys.Count];
			foreach (var folderPath in Util.EnumerateFolders(AngePath.LanguageRoot, true, "*")) {
				string filePath = Util.CombinePaths(folderPath, $"{CurrentTag}.{AngePath.LANGUAGE_FILE_EXT}");
				if (!Util.FileExists(filePath)) continue;
				string key, value;
				int lanIndex = Languages.IndexOf(Util.GetNameWithoutExtension(folderPath));
				if (lanIndex < 0) continue;
				foreach (var line in Util.ForAllLines(filePath, Encoding.UTF8)) {
					if (string.IsNullOrWhiteSpace(line)) continue;
					int colon = line.IndexOf(':');
					if (colon <= 0) continue;
					key = line[..colon];
					if (keyMap.TryGetValue(key, out int index)) {
						value = colon + 1 < line.Length ? line[(colon + 1)..] : "";
						Contents[lanIndex, index] = value;
					}
				}
			}

			return true;
		}


		private void Save () {

			IsDirty = false;
			if (string.IsNullOrEmpty(CurrentTag)) return;

			for (int lanIndex = 0; lanIndex < Languages.Count; lanIndex++) {
				var language = Languages[lanIndex];
				string path = Util.CombinePaths(AngePath.LanguageRoot, language.ToString(), $"{CurrentTag}.{AngePath.LANGUAGE_FILE_EXT}");
				var builder = new StringBuilder();
				for (int keyIndex = 0; keyIndex < Keys.Count; keyIndex++) {
					string key = Keys[keyIndex];
					string content = Contents[lanIndex, keyIndex];
					if (string.IsNullOrWhiteSpace(key)) continue;
					builder.Append(key);
					builder.Append(':');
					if (!string.IsNullOrEmpty(content)) {
						builder.Append(content.Replace("\n", ""));
					}
					builder.AppendLine();
				}
				Util.TextToFile(builder.ToString(), path, Encoding.UTF8);
			}
		}


		private void ShowTagMenu () {
			if (Languages.Count < 1) return;
			var menu = new GenericMenu();
			AddTagItem("Main");
			string folderPath = Util.CombinePaths(AngePath.LanguageRoot, Languages[0].ToString());
			foreach (string tagFilePath in Util.EnumerateFiles(folderPath, true, $"*.{AngePath.LANGUAGE_FILE_EXT}")) {
				string tag = Util.GetNameWithoutExtension(tagFilePath);
				if (tag != "Main") AddTagItem(tag);
			}
			void AddTagItem (string _tag) {
				menu.AddItem(new GUIContent($"{_tag}\t"), _tag == CurrentTag, () => {
					if (_tag == CurrentTag) return;
					if (IsDirty) Save();
					CurrentTag = _tag;
					Load();
				});
			}
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("New Tag"), false, () => {
				StringWindow.OpenWindow((newName) => {
					if (string.IsNullOrEmpty(newName)) return;
					if (IsDirty) Save();
					foreach (var language in Languages) {
						string filePath = Util.CombinePaths(AngePath.LanguageRoot, language.ToString(), $"{newName}.{AngePath.LANGUAGE_FILE_EXT}");
						if (!Util.FileExists(filePath)) Util.TextToFile("", filePath);
					}
					CurrentTag = newName;
					Load();
				}, "New Tag", "Create New Tag", "New Tag");
			});
			if (CurrentTag != "Main") {
				menu.AddItem(new GUIContent("Rename Tag"), false, () => {
					string oldTag = CurrentTag;
					StringWindow.OpenWindow((newName) => {
						if (oldTag == newName || string.IsNullOrEmpty(newName)) return;
						string oldFileName = $"{oldTag}.{AngePath.LANGUAGE_FILE_EXT}";
						string newFileName = $"{newName}.{AngePath.LANGUAGE_FILE_EXT}";
						foreach (var language in Languages) {
							string folderPath = Util.CombinePaths(AngePath.LanguageRoot, language.ToString());
							string oldPath = Util.CombinePaths(folderPath, oldFileName);
							string newPath = Util.CombinePaths(folderPath, newFileName);
							Util.MoveFile(oldPath, newPath);
						}
						if (IsDirty) Save();
						CurrentTag = newName;
						Load();
					}, oldTag, "Rename Tag", "New Name");
				});
				menu.AddItem(new GUIContent("Delete Tag"), false, () => {
					string targetTag = CurrentTag;
					if (EditorUtil.Dialog("", $"Delete Tag {targetTag} ?", "Delete", "Cancel")) {
						if (targetTag == "Main") return;
						foreach (var language in Languages) {
							string folderPath = Util.CombinePaths(AngePath.LanguageRoot, language.ToString());
							Util.DeleteFile(Util.CombinePaths(folderPath, $"{targetTag}.{AngePath.LANGUAGE_FILE_EXT}"));
						}
						CurrentTag = "Main";
						Load();
					}
				});
			} else {
				menu.AddDisabledItem(new GUIContent("Rename Tag"), false);
				menu.AddDisabledItem(new GUIContent("Delete Tag"), false);
			}
			menu.ShowAsContext();
		}


		private void RefreshKeyVisibility () {
			KeyVisibility.Clear();
			if (string.IsNullOrEmpty(SearchingText)) return;
			const System.StringComparison OIC = System.StringComparison.OrdinalIgnoreCase;
			for (int i = 0; i < Keys.Count; i++) {
				string key = Keys[i];
				bool visible = key.Contains(SearchingText, OIC);
				if (!visible) {
					for (int lanIndex = 0; lanIndex < Languages.Count; lanIndex++) {
						string con = Contents[lanIndex, i];
						if (!string.IsNullOrEmpty(con) && con.Contains(SearchingText, OIC)) {
							visible = true;
							break;
						}
					}
				}
				KeyVisibility.Add(visible);
			}
		}


		private void InitMainTag () {
			foreach (var folderPath in Util.EnumerateFolders(AngePath.LanguageRoot, true, "*")) {
				string filePath = Util.CombinePaths(folderPath, $"Main.{AngePath.LANGUAGE_FILE_EXT}");
				if (Util.FileExists(filePath)) continue;
				Util.TextToFile("", filePath);
			}
		}


	}
}
namespace AngeliaFramework.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using Moenen.Standard;
	using AngeliaFramework.Text;


	public class LanguageEditor : EditorWindow {


		// VAR
		private static GUIStyle MasterStyle => _MasterStyle ??= new GUIStyle() {
			padding = new RectOffset(24, 24, 24, 24),
		};
		private static GUIStyle _MasterStyle = null;
		private string[,] LanguageData = null;
		private string[] LanguageNames = null;
		private Vector2 MasterScrollPos = default;
		private Game Game = null;


		// API
		public static void OpenEditor (Game game) {
			var window = GetWindow<LanguageEditor>(true, "Language Editor", true);
			window.minSize = new Vector2(256, 256);
			window.maxSize = new Vector2(1024, 1024);
			window.Game = game;
			if (!window.Load(game)) {
				window.Close();
			}
		}


		// MSG
		private void OnGUI () {
			using (new GUILayout.VerticalScope(MasterStyle)) {

				// Button
				using (new GUILayout.HorizontalScope()) {
					if (GUI.Button(Layout.Rect(128, 20), "+ Key")) {
						AddNewKey();
					}
					Layout.Rect(0, 20);
				}
				Layout.Space(4);

				// Main
				using var scroll = new GUILayout.ScrollViewScope(MasterScrollPos);
				MasterScrollPos = scroll.scrollPosition;
				const int HEIGHT = 20;
				int languageCount = LanguageData.GetLength(0) - 1;
				int keyCount = LanguageData.GetLength(1);

				// Title
				using (new GUILayout.HorizontalScope()) {
					GUI.Label(Layout.Rect(0, HEIGHT), "key", EditorStyles.centeredGreyMiniLabel);
					Layout.Space(2);
					for (int i = 0; i < languageCount; i++) {
						GUI.Label(Layout.Rect(0, HEIGHT), LanguageNames[i], EditorStyles.centeredGreyMiniLabel);
						Layout.Space(2);
					}
				}

				// Content
				string prevGroup = "";
				for (int i = 0; i < keyCount; i++) {
					string key = LanguageData[0, i];
					// Key Group
					int gIndex = key.IndexOf('.');
					string group = gIndex <= 0 ? key : key[0..gIndex];
					if (group != prevGroup) {
						GUI.Label(Layout.Rect(0, 12), group, Layout.MiniGreyLabel);
						Layout.Space(2);
					}
					using (new GUILayout.HorizontalScope()) {
						var oldC = GUI.backgroundColor;
						GUI.backgroundColor = string.IsNullOrEmpty(key) ? new Color(1f, 0.5f, 0.5f, 1f) : oldC;
						// Key
						LanguageData[0, i] = EditorGUI.TextField(
							Layout.Rect(0, HEIGHT), key
						);
						Layout.Space(2);
						// Values
						for (int index = 1; index <= languageCount; index++) {
							LanguageData[index, i] = EditorGUI.TextField(
								Layout.Rect(0, HEIGHT), LanguageData[index, i]
							);
							Layout.Space(2);
						}
						GUI.backgroundColor = oldC;
					}
					prevGroup = group;
					Layout.Space(2);
				}
				Layout.Space(64);
			}

			// Final
			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl("");
				Repaint();
			}

		}


		private void OnLostFocus () {
			Save(Game);
			Close();
		}


		// LGC
		private bool Load (Game game) {
			if (
				Util.GetFieldValue(game, "m_Languages") is not Language[] languages ||
				languages.Length == 0
			) {
				Debug.LogWarning("[Language Editor] No language data founded.");
				return false;
			}
			// Game >> Language Data
			var keys = new HashSet<string>();
			foreach (var language in languages) {
				var cells = Util.GetFieldValue(language, "m_Cells") as object[];
				for (int i = 0; i < cells.Length; i++) {
					keys.TryAdd(Util.GetFieldValue(cells[i], "Key") as string);
				}
			}
			var keyList = keys.ToList();
			keyList.Sort((a, b) => a.CompareTo(b));
			LanguageData = new string[languages.Length + 1, keyList.Count];
			for (int y = 0; y < keyList.Count; y++) {
				LanguageData[0, y] = keyList[y];
			}
			for (int x = 1; x <= languages.Length; x++) {
				var lan = languages[x - 1];
				lan.Init();
				for (int y = 0; y < keyList.Count; y++) {
					LanguageData[x, y] = lan[keyList[y].ACode()];
				}
				lan.ClearCache();
			}
			// Language Names
			LanguageNames = new string[languages.Length];
			for (int i = 0; i < languages.Length; i++) {
				LanguageNames[i] = languages[i].DisplayName;
			}
			return true;
		}


		private void Save (Game game) {
			// Language Data >> Game
			var languages = Util.GetFieldValue(game, "m_Languages") as Language[];
			int keyCount = LanguageData.GetLength(1);
			for (int valueIndex = 0; valueIndex < languages.Length; valueIndex++) {
				var cells = new List<Language.Cell>();
				for (int keyIndex = 0; keyIndex < keyCount; keyIndex++) {
					string key = LanguageData[0, keyIndex];
					if (string.IsNullOrEmpty(key)) { continue; }
					cells.Add(new Language.Cell() {
						Key = key,
						Value = LanguageData[valueIndex + 1, keyIndex],
					});
				}
				Util.SetFieldValue(languages[valueIndex], "m_Cells", cells.ToArray());
			}
			Util.SetFieldValue(game, "m_Languages", languages);
			foreach (var lan in languages) {
				EditorUtility.SetDirty(lan);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		private void AddNewKey () {
			int languageCount = LanguageData.GetLength(0);
			int keyCount = LanguageData.GetLength(1);
			var newData = new string[languageCount, keyCount + 1];
			for (int x = 0; x < languageCount; x++) {
				newData[x, 0] = "";
				for (int y = 1; y < keyCount + 1; y++) {
					newData[x, y] = LanguageData[x, y - 1];
				}
			}
			LanguageData = newData;
		}


	}
}

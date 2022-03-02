using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class DialogueEditor : EditorWindow {


		// Short
		private static GUIStyle MasterStyle => _MasterStyle ??= new GUIStyle() {
			padding = new RectOffset(24, 24, 24, 24),
		};
		private static GUIStyle _MasterStyle = null;
		private GameData GameData => _GameData != null ? _GameData : (_GameData = TryGetGameData());
		private GameData _GameData = null;

		// Data
		private Vector2 MasterScrollPos = default;
		private readonly Dictionary<SystemLanguage, Dialogue> DialoguePool = new();


		// API
		public static void OpenEditor () {
			var window = GetWindow<DialogueEditor>(true, "Dialogue Editor", true);
			window.minSize = new Vector2(256, 256);
			window.maxSize = new Vector2(1024, 1024);
			if (!window.Load()) {
				window.Close();
			}
		}


		private void OnGUI () {
			using (new GUILayout.VerticalScope(MasterStyle)) {
				using var scroll = new GUILayout.ScrollViewScope(MasterScrollPos);
				MasterScrollPos = scroll.scrollPosition;






			}
			Layout.CancelFocusOnClick(this);
		}


		private void OnLostFocus () => Close();


		private void OnDestroy () => Save();


		private bool Load () {
			DialoguePool.Clear();
			foreach (var lan in GameData.Languages) {
				var dialogue = Dialogue.LoadFromDisk(lan.LanguageID);
				if (dialogue == null) continue;
				DialoguePool.TryAdd(lan.LanguageID, dialogue);
			}
			return true;
		}


		private void Save () {
			foreach (var pair in DialoguePool) {
				Dialogue.EditorOnly_SaveToDisk(pair.Value);
			}
		}


		private GameData TryGetGameData () {
			foreach (var data in EditorUtil.ForAllAssets<GameData>()) return data;
			return null;
		}


	}
}

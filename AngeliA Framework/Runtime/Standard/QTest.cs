#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace AngeliaFramework {
	public class QTest : EditorWindow {


		public class IntTest {
			public int this[string key, int @default = 0, int min = int.MinValue, int max = int.MaxValue] {
				get {
					RequireWindow();
					if (!IntPool.TryGetValue(key, out var value)) {
						value = (@default, min, max, true);
						IntPool.Add(key, value);
					}
					value.editable = true;
					return value.value;
				}
				set {
					RequireWindow();
					bool editable = false;
					if (IntPool.TryGetValue(key, out var result)) {
						editable = result.editable;
					}
					IntPool[key] = (value, min, max, editable);
				}
			}
		}

		public static readonly IntTest Int = new();
		private static QTest Instance = null;
		private static readonly Dictionary<string, (int value, int min, int max, bool editable)> IntPool = new();
		private static readonly GUIStyle PanelStyle = new() { padding = new RectOffset(24, 24, 24, 24), };
		private Vector2 ScrollPos = default;


		[InitializeOnLoadMethod]
		public static void Initialize () {
			EditorApplication.playModeStateChanged += (state) => {
				if (state == PlayModeStateChange.ExitingPlayMode) {
					IntPool.Clear();
					Instance = null;
				}
			};
		}


		private static void RequireWindow () {
			if (!EditorApplication.isPlaying) return;
			if (Instance == null) {
				var oldFocus = focusedWindow;
				Instance = GetWindow<QTest>(true, "QTest", false);
				Instance.minSize = new Vector2(320, 240);
				if (oldFocus != null) {
					FocusWindowIfItsOpen(oldFocus.GetType());
				}
			} else {
				Instance.Repaint();
			}
		}


		private void OnGUI () {
			if (!EditorApplication.isPlaying) {
				Close();
				return;
			}
			using var scroll = new EditorGUILayout.ScrollViewScope(ScrollPos, PanelStyle);
			float oldW = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 48f;
			ScrollPos = scroll.scrollPosition;
			// Int
			string changingKey = "";
			int changingValue = 0;
			int changingMin = 0;
			int changingMax = 0;
			foreach (var (key, (value, min, max, editable)) in IntPool) {
				int newValue;
				if (editable) {
					if (min == int.MinValue || max == int.MaxValue) {
						newValue = EditorGUILayout.IntField(key, value);
					} else {
						newValue = EditorGUILayout.IntSlider(key, value, min, max);
					}
				} else {
					newValue = value;
					EditorGUILayout.LabelField(key, value.ToString());
				}
				if (newValue != value) {
					changingKey = key;
					changingValue = newValue;
					changingMin = min;
					changingMax = max;
				}
				GUILayout.Space(2);
			}
			if (!string.IsNullOrEmpty(changingKey)) {
				IntPool[changingKey] = (changingValue, changingMin, changingMax, true);
			}
			EditorGUIUtility.labelWidth = oldW;
		}


	}
}
#endif
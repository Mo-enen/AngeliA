using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace PixelJelly.Editor {
	public abstract class JellyInspector {


		// Short
		private static GUIStyle HighlightLabel => _HighlightLabel ??= new GUIStyle(GUI.skin.label) {
			normal = new GUIStyleState() { textColor = Color.white, },
			alignment = TextAnchor.MiddleLeft,
			wordWrap = false,
			fontSize = 8,
		};
		private static GUIStyle _HighlightLabel = null;

		// Data
		private readonly Dictionary<string, string> PropertySwaper = new Dictionary<string, string>();
		private readonly Dictionary<string, MessageType> ParamHighlightMap = new Dictionary<string, MessageType>();


		// API
		public static void DefaultInspectorGUI (SerializedProperty[] properties, Dictionary<string, string> swaper = null, Dictionary<string, MessageType> highlightParam = null) {
			var oldC = GUI.color;
			bool prevSeparator = false;
			for (int i = 0; i < properties.Length; i++) {
				var prop = properties[i];
				if (prop != null) {
					// Naming Swape
					string label;
					if (swaper != null && swaper.ContainsKey(prop.name)) {
						label = swaper[prop.name];
					} else {
						label = prop.displayName;
					}
					if (string.IsNullOrEmpty(label)) { continue; }
					if (label == ".") { label = prop.displayName; }
					// Field
					EditorGUI.PropertyField(
						Layout.Rect(0, (int)EditorGUI.GetPropertyHeight(prop)).Expand(0, -2, 0, 0),
						prop,
						new GUIContent(label),
						true
					);
					prevSeparator = false;
					// Highlight
					if (highlightParam != null && highlightParam.ContainsKey(prop.name)) {
						var type = highlightParam[prop.name];
						var rect = Layout.LastRect().Expand(-3, 0, 0, 0);
						if (EditorGUIUtility.isProSkin) {
							GUI.color =
								type == MessageType.Info ? new Color(240f / 255f, 240f / 255f, 240f / 255f) :
								type == MessageType.Warning ? new Color(255f / 255f, 193f / 255f, 7f / 255f) :
								type == MessageType.Error ? new Color(255f / 255f, 110f / 255f, 64f / 255f) :
								Color.clear;
						} else {
							GUI.color =
								type == MessageType.Info ? new Color(128f / 255f, 128f / 255f, 128f / 255f) :
								type == MessageType.Warning ? new Color(201f / 255f, 151f / 255f, 0) :
								type == MessageType.Error ? new Color(177f / 255f, 12f / 255f, 12f / 255f) :
								Color.clear;
						}
						GUI.Label(rect, "¡ñ", HighlightLabel);
						GUI.color = oldC;
					}
					Layout.Space(4);
				} else {
					// Separator
					if (!prevSeparator) {
						Layout.Space(4);
						EditorGUI.DrawRect(
							Layout.Rect(0, 1),
							EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.4f) : new Color(1f, 1f, 1f, 0.4f)
						);
						Layout.Space(8);
					}
					prevSeparator = true;
				}
			}
		}


		public virtual void OnPropertySwape (SerializedObject serializedObject) { }


		public virtual void OnInspectorGUI (SerializedObject serializedObject, SerializedProperty[] properties) => DefaultInspectorGUI(properties, PropertySwaper, ParamHighlightMap);


		public virtual void OnCustomButtonGUI () { }


		public void SwapeProperty (string key, string newKey) => PropertySwaper.SetOrAdd(key, newKey);


		public void HighlightParam (string name, MessageType type) => ParamHighlightMap.SetOrAdd(name, type);


		public void ClearParamHighlight () => ParamHighlightMap.Clear();


	}
}

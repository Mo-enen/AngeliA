namespace AngeliaFramework.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using Moenen.Standard;
	[CustomEditor(typeof(Game))]
	public class Game_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
			Layout.Space(6);
			if (GUI.Button(Layout.Rect(0, 32), "Language Editor")) {
				LanguageEditor.OpenEditor(target as Game);
			}
		}
	}
}
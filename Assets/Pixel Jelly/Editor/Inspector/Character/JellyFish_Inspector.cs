using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(JellyFish))]
	public class JellyFish_Inspector : JellyInspector {


		public override void OnPropertySwape (SerializedObject serializedObject) {
			SwapeProperty("m_Width", "");
			SwapeProperty("m_Height", "");
			SwapeProperty("m_FrameDuration", "");
			SwapeProperty("m_FrameCount", "");
			SwapeProperty("m_SpriteCount", "");

		}


		public override void OnInspectorGUI (SerializedObject serializedObject, SerializedProperty[] properties) {

			// Basic
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Width"));
			Layout.Space(2);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Height"));
			Layout.Space(2);

			// Animation Page
			var target = serializedObject.targetObject as JellyFish;
			if (target.Animations != null && target.Animations.Length > 1) {
				var p_sAni = serializedObject.FindProperty("m_SelectingAnimation");
				using (new GUILayout.HorizontalScope()) {
					int index = Mathf.Clamp(p_sAni.intValue, 0, target.Animations.Length - 1);
					GUI.Label(EditorGUI.IndentedRect(Layout.Rect(130, 18)), $"Animation: {target.Animations[index].Name}");
					p_sAni.intValue = Layout.PageField(Layout.Rect(0, 18), index, target.Animations.Length);
					Layout.Space(4);
				}
			}

			// Default
			base.OnInspectorGUI(serializedObject, properties);
		}


	}
}

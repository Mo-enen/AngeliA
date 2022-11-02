using System.Collections;
using System.Collections.Generic;
using AngeliaFramework.Editor;
using AngeliaFramework;
using Moenen.Standard;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace Yaya.Editor {
	[CustomEditor(typeof(Yaya), true)]
	[DisallowMultipleComponent]
	public class Yaya_Inspector : Game_Inspector {


		private SerializedProperty p_YayaMeta = null;
		private SerializedProperty p_YayaAsset = null;


		protected override void OnEnable () {
			base.OnEnable();
			p_YayaMeta = serializedObject.FindProperty("m_YayaMeta");
			p_YayaAsset = serializedObject.FindProperty("m_YayaAsset");
		}


		public override void OnInspectorGUI () {
			base.OnInspectorGUI();
			serializedObject.Update();
			EditorGUILayout.PropertyField(p_YayaMeta);
			EditorGUILayout.PropertyField(p_YayaAsset);
			serializedObject.ApplyModifiedProperties();
		}


	}
}
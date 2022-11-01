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


		private string OldYayaMetaJson = "";
		private SerializedProperty p_YayaMeta = null;
		private SerializedProperty p_YayaAsset = null;


		protected override void OnEnable () {
			base.OnEnable();
			p_YayaMeta = serializedObject.FindProperty("m_YayaMeta");
			p_YayaAsset = serializedObject.FindProperty("m_YayaAsset");
			// Get Meta for Changing Check
			OldYayaMetaJson = "";
			if (Util.GetFieldValue(target, "m_YayaMeta") is YayaMeta oldMeta) {
				OldYayaMetaJson = JsonUtility.ToJson(oldMeta, false);
			}
		}


		protected override void OnDestroy () {
			base.OnDestroy();
			// Check Meta Changed
			if (!string.IsNullOrEmpty(OldYayaMetaJson)) {
				if (Util.GetFieldValue(target, "m_YayaMeta") is YayaMeta newMeta) {
					string newMetaJson = JsonUtility.ToJson(newMeta, false);
					if (newMetaJson != OldYayaMetaJson) {
						// Changed
						Game.SaveMeta(newMeta);
					}
				}
			}

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
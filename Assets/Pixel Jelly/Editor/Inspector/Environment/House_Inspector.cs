using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(House))]
	public class House_Inspector : JellyInspector {
		public override void OnInspectorGUI (SerializedObject serializedObject, SerializedProperty[] properties) {

			SwapeProperty("m_FrameCount", "");
			SwapeProperty("m_FrameDuration", "");
			

			base.OnInspectorGUI(serializedObject, properties);
		}
	}
}

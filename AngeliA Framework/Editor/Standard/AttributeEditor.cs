namespace AngeliaFramework.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;



	[CustomPropertyDrawer(typeof(DisableAttribute))]
	public class Disable_AttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			bool oldE = GUI.enabled;
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label);
			GUI.enabled = oldE;
		}
	}
	


	[CustomPropertyDrawer(typeof(NullAlertAttribute))]
	public class NullAlert_AttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.PropertyField(position, property, label);
			if (property.objectReferenceValue == null) {
				var oldC = GUI.color;
				GUI.color = new Color(1f, 0f, 0f, 1f);
				GUI.Box(new Rect(0, position.y, position.width + position.x, position.height), GUIContent.none);
				GUI.color = oldC;
			}
		}
	}



	[CustomPropertyDrawer(typeof(ClampAttribute))]
	public class Clamp_AttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			switch (property.propertyType) {
				case SerializedPropertyType.Float: {
					var cAtt = attribute as ClampAttribute;
					property.floatValue = Mathf.Clamp(property.floatValue, cAtt.Min, cAtt.Max);
					break;
				}
				case SerializedPropertyType.Integer: {
					var cAtt = attribute as ClampAttribute;
					property.intValue = Mathf.Clamp(property.intValue, (int)cAtt.Min, (int)cAtt.Max);
					break;
				}
			}
			EditorGUI.PropertyField(position, property, label);
		}
	}



	[CustomPropertyDrawer(typeof(HelpAttribute))]
	public class Help_AttributeDrawer : PropertyDrawer {

		private static GUIContent ButtonContent => _ButtonContent ??= EditorGUIUtility.IconContent("_Help");
		private static GUIContent _ButtonContent = null;

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			position.width -= position.height + 2f;
			EditorGUI.PropertyField(position, property, label);
			position.x += position.width + 2f;
			position.width = position.height;
			if (GUI.Button(position, ButtonContent, EditorStyles.iconButton)) {
				Application.OpenURL(Util.GetUrl((attribute as HelpAttribute).Link));
			}
			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
		}

	}





	[CustomPropertyDrawer(typeof(DisableAtRuntimeAttribute))]
	public class DisableAtRuntimeAttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			bool oldE = GUI.enabled;
			GUI.enabled = !EditorApplication.isPlaying;
			EditorGUI.PropertyField(position, property, label);
			GUI.enabled = oldE;
		}
	}



	[CustomPropertyDrawer(typeof(DisableAtEdittimeAttribute))]
	public class DisableAtEdittimeAttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			bool oldE = GUI.enabled;
			GUI.enabled = EditorApplication.isPlaying;
			EditorGUI.PropertyField(position, property, label);
			GUI.enabled = oldE;
		}
	}


	[CustomPropertyDrawer(typeof(HideAtRuntimeAttribute))]
	public class HideAtRuntimeAttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (!EditorApplication.isPlaying)
				EditorGUI.PropertyField(position, property, label);
		}
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			return !EditorApplication.isPlaying ? EditorGUI.GetPropertyHeight(property, label, true) : 0;
		}
	}



	[CustomPropertyDrawer(typeof(HideAtEdittimeAttribute))]
	public class HideAtEdittimeAttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (EditorApplication.isPlaying)
				EditorGUI.PropertyField(position, property, label);
		}
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			return EditorApplication.isPlaying ? EditorGUI.GetPropertyHeight(property, label, true) : 0;
		}
	}



	[CustomPropertyDrawer(typeof(DisplayLabelAttribute))]
	public class DisplayLabelAttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			var att = attribute as DisplayLabelAttribute;
			label.text = att.Name;
			if (!string.IsNullOrEmpty(att.IconPath)) {
				label.image = EditorGUIUtility.IconContent(att.IconPath).image;
			}
			EditorGUI.PropertyField(position, property, label, true);
		}
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true);
		}
	}

}
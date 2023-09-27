#if UNITY_EDITOR
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Overlays;
using AngeliaFramework;


namespace Moenen.Editor {
	public static class HideHierarchyLeftPanel {

		private const float TARGET_PANEL_LEFT = -32f;
		private static EditorWindow Hierarchy = null;
		private static bool RequireFix = true;

		[InitializeOnLoadMethod]
		public static void Init () {
			//EditorApplication.hierarchyWindowItemOnGUI += HieGUI;
			//EditorApplication.update += Update;
			// Func
			//static void HieGUI (int instanceID, Rect rect) => RequireFix = true;
		}

		public static void Update () {
			if (RequireFix && Hierarchy == null) {
				RequireFix = false;
				var hierarchy = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
				if (hierarchy != null) {
					var objs = Resources.FindObjectsOfTypeAll(hierarchy);
					if (objs.Length > 0) {
						Hierarchy = objs[0] as EditorWindow;
						FixHierarchy();
					}
				}
			}
			if (Hierarchy != null && !Mathf.Approximately(Hierarchy.rootVisualElement.style.left.value.value, TARGET_PANEL_LEFT)) {
				FixHierarchy();
			}
			// Func
			static void FixHierarchy () {
				Hierarchy.rootVisualElement.style.left = -32;

				//m_Layout
				//var layout = Hierarchy.rootVisualElement.layout;
				//layout.height -= 132;
				//Util.SetFieldValue(Hierarchy.rootVisualElement, "m_Layout", layout);


				//Hierarchy.rootVisualElement.style.transformOrigin = new TransformOrigin(Hierarchy.position.width, 0);
				//Hierarchy.rootVisualElement.style.position = Position.Absolute;
				//Hierarchy.rootVisualElement.style.unitySliceLeft = -132;
				//Hierarchy.rootVisualElement.style.translate = new StyleTranslate(
				//	new Translate(-32, 0)
				//);
				//float deltaScl = 32f / Hierarchy.position.width;
				//Hierarchy.rootVisualElement.style.scale = new StyleScale(
				//	new Scale(new Vector3(1f + deltaScl, 1f, 1f))
				//);
			}
		}

	}
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PixelJelly.Editor {
	[CustomJellyInspector(typeof(MagicCircle))]
	public class MagicCircle_Inspector : JellyInspector {





		// Data

		private EditorSavingInt PageIndex = new EditorSavingInt("Jelly.MagicCircle_Inspector.PageIndex", 0);
		private bool PageMode = true;



		// MSG
		public override void OnPropertySwape (SerializedObject serializedObject) {
			SwapeProperty("m_SpriteCount", "");
			if (PageMode) {
				SwapeProperty("m_Width", "");
				SwapeProperty("m_Height", "");
				SwapeProperty("m_FrameCount", "");
				SwapeProperty("m_FrameDuration", "");
			} else {
				SwapeProperty("m_Width", ".");
				SwapeProperty("m_Height", ".");
				SwapeProperty("m_FrameCount", ".");
				SwapeProperty("m_FrameDuration", ".");
			}
		}


		public override void OnInspectorGUI (SerializedObject serializedObject, SerializedProperty[] properties) {

			var p_Width = serializedObject.FindProperty("m_Width");
			var p_Height = serializedObject.FindProperty("m_Height");
			var p_FrameCount = serializedObject.FindProperty("m_FrameCount");
			var p_FrameDuration = serializedObject.FindProperty("m_FrameDuration");


			EditorGUILayout.PropertyField(p_Width, new GUIContent("Width"));
			Layout.Space(2);
			EditorGUILayout.PropertyField(p_Height, new GUIContent("Height"));
			Layout.Space(2);
			EditorGUILayout.PropertyField(p_FrameCount, new GUIContent("Frame Count"));
			Layout.Space(2);
			EditorGUILayout.PropertyField(p_FrameDuration, new GUIContent("Frame Duration"));
			Layout.Space(6);
			EditorGUI.DrawRect(
				Layout.Rect(0, 1),
				EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.4f) : new Color(1f, 1f, 1f, 0.4f)
			);
			Layout.Space(6);

			PageGUI(serializedObject);

			Layout.Space(6);
			Layout.SimpleLine();
			Layout.Space(6);

		}


		// LGC
		private void PageGUI (SerializedObject serializedObject) {

			var p_Rings = serializedObject.FindProperty("m_Rings");
			bool pageMode = PageMode;
			int pageCount = p_Rings.arraySize;

			// Bar
			using (new GUILayout.HorizontalScope()) {
				// Mode Switch
				if (GUI.Button(EditorGUI.IndentedRect(Layout.Rect(64, 18)), PageMode ? "Edit" : "Back")) {
					pageMode = !pageMode;
				}
				if (pageMode) {
					PageIndex.Value = Layout.PageField(Layout.Rect(0, 18), PageIndex.Value, pageCount);
				}
				Layout.Space(4);
			}

			if (pageMode) {
				// Page Content
				Layout.Space(6);
				if (pageCount > 0) {
					var p_Ele = p_Rings.GetArrayElementAtIndex(PageIndex.Value);

					// Name
					EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Name"));
					Layout.Space(2);

					// Style
					var p_Style = p_Ele.FindPropertyRelative("m_Style");
					var style = (MagicCircle.Ring.RingStyle)p_Style.enumValueIndex;
					EditorGUILayout.PropertyField(p_Style);
					Layout.Space(2);

					// Position
					EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Center"));
					Layout.Space(2);

					// Distance
					EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Distance"));
					Layout.Space(2);

					if (style != MagicCircle.Ring.RingStyle.Sprite) {
						if (style == MagicCircle.Ring.RingStyle.Star) {
							// Radius B
							EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_RadiusB"), new GUIContent("Radius"));
							Layout.Space(2);
						} else {
							// Radius A
							EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_RadiusA"), new GUIContent("Radius"));
							Layout.Space(2);
						}
					}

					// Scale
					if (style == MagicCircle.Ring.RingStyle.Sprite || style == MagicCircle.Ring.RingStyle.Letter) {
						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_SpriteScale"), new GUIContent("Scale"));
						Layout.Space(2);
					}

					// Thickness
					if (style != MagicCircle.Ring.RingStyle.Sprite && style != MagicCircle.Ring.RingStyle.Letter) {
						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Thickness"));
						Layout.Space(2);
					}

					// Poly Side Count
					if (style == MagicCircle.Ring.RingStyle.Polygon || style == MagicCircle.Ring.RingStyle.Star) {
						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_PolySideCount"), new GUIContent("Side Count"));
						Layout.Space(2);
					}

					// More Letter
					if (style == MagicCircle.Ring.RingStyle.Letter) {
						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_MoreLetter"), new GUIContent("More Letter"));
						Layout.Space(2);
					}

					// Revolution
					EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Revolution"));
					Layout.Space(2);

					// Rotation
					EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Rotation"));
					Layout.Space(2);


					// Gap
					if (style != MagicCircle.Ring.RingStyle.Sprite) {
						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Gap"));
						Layout.Space(2);
					}

					// Color
					EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Color"));
					Layout.Space(2);

					// Sprite
					if (style == MagicCircle.Ring.RingStyle.Sprite) {
						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Sprite"));
						Layout.Space(2);
					}

					// Dynamic
					var p_Dynamic = p_Ele.FindPropertyRelative("m_Dynamic");
					EditorGUILayout.PropertyField(p_Dynamic);
					Layout.Space(2);

					// Curves
					if (p_Dynamic.boolValue) {
						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveCenterX"), new GUIContent("Center X"));
						Layout.Space(2);

						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveCenterY"), new GUIContent("Center Y"));
						Layout.Space(2);

						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveDistance"), new GUIContent("Distance"));
						Layout.Space(2);

						if (style != MagicCircle.Ring.RingStyle.Sprite) {
							if (style == MagicCircle.Ring.RingStyle.Star) {
								EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveRadiusBX"), new GUIContent("Radius In"));
								EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveRadiusBY"), new GUIContent("Radius Out"));
								Layout.Space(2);
							} else {
								EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveRadiusA"), new GUIContent("Radius"));
								Layout.Space(2);
							}
						}

						if (style == MagicCircle.Ring.RingStyle.Sprite || style == MagicCircle.Ring.RingStyle.Letter) {
							EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveSpriteScale"), new GUIContent("Scale"));
							Layout.Space(2);
						}

						if (style != MagicCircle.Ring.RingStyle.Sprite && style != MagicCircle.Ring.RingStyle.Letter) {
							EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveThickness"), new GUIContent("Thickness"));
							Layout.Space(2);
						}

						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveRevolution"), new GUIContent("Revolution"));
						Layout.Space(2);

						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveRotation"), new GUIContent("Rotation"));
						Layout.Space(2);

						if (style != MagicCircle.Ring.RingStyle.Sprite) {
							EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveGap"), new GUIContent("Gap"));
							Layout.Space(2);
						}

						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_CurveAlpha"), new GUIContent("Alpha"));
						Layout.Space(2);
					}

					// Letters
					if (style == MagicCircle.Ring.RingStyle.Letter) {
						EditorGUILayout.PropertyField(p_Ele.FindPropertyRelative("m_Letters"), true);
						Layout.Space(2);
					}

				} else {
					// No Ring
					EditorGUILayout.HelpBox("No ring in this magic circle", UnityEditor.MessageType.Info, true);
				}
				Layout.Space(6);
			} else {
				// Edit Content
				EditorGUILayout.PropertyField(p_Rings, new GUIContent("Rings"), true);
				Layout.Space(2);
			}
			PageMode = pageMode;
		}


	}
}

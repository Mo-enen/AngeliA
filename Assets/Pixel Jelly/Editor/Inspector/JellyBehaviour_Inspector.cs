using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PixelJelly.Editor {
	[CustomEditor(typeof(JellyBehaviour), true)]
	[DisallowMultipleComponent]
	public class JellyBehaviour_Inspector : UnityEditor.Editor {
		private static Texture2D TextureIcon => _TextureIcon ??= EditorGUIUtility.IconContent("Texture Icon").image as Texture2D;
		private static Texture2D _TextureIcon = null;
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
			Layout.Space(8);
			ColorLineGUI(0, 1);
			Layout.Space(8);
			if (Layout.IconButton(Layout.Rect(0, 24).Expand(-24, -24, 0, 0), "Export Texture", TextureIcon)) {
				var behaviour = target as JellyBehaviour;
				PixelJellyWindow.ExportTexture(behaviour, EditorUtility.SaveFilePanelInProject("Export Texture", behaviour.FinalDisplayName, "png", "Export Texture"));
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				PixelPostprocessor.Clear();
			}
		}
		private void ColorLineGUI (int width, int height, float alpha = 0.2f) => EditorGUI.DrawRect(
			Layout.Rect(width, height),
			EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, alpha) : new Color(1f, 1f, 1f, alpha)
		);
	}
}

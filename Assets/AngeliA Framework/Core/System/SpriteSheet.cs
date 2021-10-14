using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Sheet", menuName = "AngeliA/New Sheet", order = 99)]
	public class SpriteSheet : ScriptableObject {


		public Sprite[] Sprites => m_Sprites;
		[SerializeField] Sprite[] m_Sprites = null;


		public Rect[] GetUVs () {
			var uvs = new Rect[m_Sprites.Length];
			var texture = m_Sprites.Length > 0 ? m_Sprites[0].texture : null;
			if (texture == null) { return uvs; }
			for (int i = 0; i < m_Sprites.Length; i++) {
				var sp = m_Sprites[i];
				var rect = sp.rect;
				float width = texture.width;
				float height = texture.height;
				uvs[i] = new Rect(
					rect.x / width, rect.y / height,
					rect.width / width, rect.height / height
				);
			}
			return uvs;
		}


		public Material GetMaterial () => new Material(Shader.Find("Cell")) {
			name = "Material",
			shader = Shader.Find("Cell"),
			mainTexture = m_Sprites.Length > 0 ? m_Sprites[0].texture : null,
			enableInstancing = true,
			mainTextureOffset = Vector2.zero,
			mainTextureScale = Vector2.one,
			doubleSidedGI = false,
			renderQueue = 3000
		};


	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(SpriteSheet)), DisallowMultipleComponent]
	public class SpriteSheet_Inspector : Editor {
		private int m_TextureWarning = -2;
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
			if (m_TextureWarning >= 0) {
				EditorGUILayout.HelpBox($"Sprites must have same texture\nElement {m_TextureWarning} has different texture.", MessageType.Error, true);
			}
			if (GUI.changed || m_TextureWarning == -2) {
				var sheet = serializedObject.targetObject as SpriteSheet;
				m_TextureWarning = -1;
				if (sheet.Sprites != null && sheet.Sprites.Length > 1 && sheet.Sprites[0] != null) {
					var targetTexture = sheet.Sprites[0].texture;
					for (int i = 0; i < sheet.Sprites.Length; i++) {
						Sprite sp = sheet.Sprites[i];
						if (sp != null && sp.texture != targetTexture) {
							m_TextureWarning = i;
							break;
						}
					}
				}
			}
		}
	}
}
#endif

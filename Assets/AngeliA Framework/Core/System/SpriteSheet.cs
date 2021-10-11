using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Sheet", menuName = "AngeliA/New Sheet", order = 99)]
	public class SpriteSheet : ScriptableObject {

		public Material Material => m_Material;
		public Sprite[] Sprites => m_Sprites;

		[SerializeField, Disable] Material m_Material = null;
		[SerializeField] Sprite[] m_Sprites = null;

		public Rect[] GetUVs () {
			var uvs = new Rect[m_Sprites.Length];
			var texture = Material.mainTexture;
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

	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(SpriteSheet)), DisallowMultipleComponent]
	public class SpriteSheet_Inspector : Editor {


		// VAR
		private int m_TextureWarning = -2;


		// MSG
		private void OnEnable () => FixMaterial();
		private void OnDisable () => FixMaterial();


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
				if (sheet.Sprites.Length > 1) {
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


		// LGC
		private void FixMaterial () {
			var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(serializedObject.targetObject));
			var sheet = serializedObject.targetObject as SpriteSheet;
			Material mat = null;
			for (int i = 0; i < objs.Length; i++) {
				var obj = objs[i];
				if (!(obj is Material)) { continue; }
				mat = obj as Material;
				RefreshMat();
				serializedObject.Update();
				serializedObject.FindProperty("m_Material").objectReferenceValue = mat;
				serializedObject.ApplyModifiedProperties();
				break;
			}
			for (int i = 0; i < objs.Length; i++) {
				var obj = objs[i];
				if (obj != mat && obj != sheet) {
					DestroyImmediate(obj, true);
				}
			}
			if (mat == null) {
				mat = new Material(Shader.Find("Cell"));
				RefreshMat();
				AssetDatabase.AddObjectToAsset(mat, sheet);
				serializedObject.Update();
				serializedObject.FindProperty("m_Material").objectReferenceValue = mat;
				serializedObject.ApplyModifiedProperties();
			}
			void RefreshMat () {
				mat.name = "Material";
				mat.shader = Shader.Find("Cell");
				mat.hideFlags = HideFlags.NotEditable;
				mat.mainTexture = sheet.Sprites.Length > 0 ? sheet.Sprites[0].texture : null;
				mat.enableInstancing = true;
				mat.mainTextureOffset = Vector2.zero;
				mat.mainTextureScale = Vector2.one;
				mat.doubleSidedGI = false;
				mat.renderQueue = 3000;
			}
		}


	}
}
#endif

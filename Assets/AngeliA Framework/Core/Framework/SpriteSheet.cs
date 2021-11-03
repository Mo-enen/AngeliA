using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Sheet", menuName = "AngeliA/New Sheet", order = 99)]
	public class SpriteSheet : ScriptableObject {


		public Texture2D Texture => m_Texture;
		public Sprite[] Sprites => m_Sprites;
		public int RendererCapacity => m_RendererCapacity;

		[SerializeField, NullAlert] Texture2D m_Texture = null;
		[SerializeField, NullAlert, Disable] Sprite[] m_Sprites = null;
		[SerializeField] int m_RendererCapacity = 1024;


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
			mainTexture = m_Texture,
			enableInstancing = true,
			mainTextureOffset = Vector2.zero,
			mainTextureScale = Vector2.one,
			doubleSidedGI = false,
			renderQueue = 3000
		};


#if UNITY_EDITOR
		public void SetSprites (Sprite[] sprites) => m_Sprites = sprites;
#endif


	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(SpriteSheet)), DisallowMultipleComponent]
	public class SpriteSheet_Inspector : Editor {
		[MenuItem("Tools/Reload Sheet Assets")]
		private static void Init () {
			foreach (var guid in AssetDatabase.FindAssets("t:SpriteSheet")) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var sheet = AssetDatabase.LoadAssetAtPath<SpriteSheet>(path);
				var sprites = new List<Sprite>();
				var tPath = AssetDatabase.GetAssetPath(sheet.Texture);
				var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(tPath);
				for (int i = 0; i < objs.Length; i++) {
					var obj = objs[i];
					if (obj != null && obj is Sprite sp) {
						sprites.Add(sp);
					}
				}
				sheet.SetSprites(sprites.ToArray());
				EditorUtility.SetDirty(sheet);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		private void OnEnable () => ReloadSprites();
		private void OnDisable () => ReloadSprites();
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
			if (GUI.Button(GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true)), "Reload Sprites")) {
				ReloadSprites();
			}
		}
		private void ReloadSprites () {
			serializedObject.Update();
			var p_Texture = serializedObject.FindProperty("m_Texture");
			var p_Sprites = serializedObject.FindProperty("m_Sprites");
			var texture = p_Texture.objectReferenceValue as Texture2D;
			p_Sprites.ClearArray();
			serializedObject.ApplyModifiedProperties();
			if (texture != null) {
				string path = AssetDatabase.GetAssetPath(texture);
				var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
				for (int i = objs.Length - 1; i >= 0; i--) {
					var obj = objs[i];
					if (obj == null || !(obj is Sprite sprite)) { continue; }
					p_Sprites.InsertArrayElementAtIndex(0);
					p_Sprites.GetArrayElementAtIndex(0).objectReferenceValue = sprite;
				}
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif

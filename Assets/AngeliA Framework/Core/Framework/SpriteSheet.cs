using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Sheet", menuName = "бя AngeliA/Sprite Sheet", order = 99)]
	[PreferBinarySerialization]
	public class SpriteSheet : ScriptableObject {


		// SUB
		[System.Serializable]
		public class UvSprite {
			public int GlobalID;
			public UvRect Rect;
		}


		// Api
		public Texture2D Texture => m_Texture;
		public UvSprite[] Sprites => m_Sprites;
		public int RendererCapacity => m_RendererCapacity;

		// Ser
		[SerializeField, NullAlert] Texture2D m_Texture = null;
		[SerializeField, Disable] UvSprite[] m_Sprites = null;
		[SerializeField] int m_RendererCapacity = 1024;
		[SerializeField] string m_ShaderName = "Cell";


		// API
		public UvRect[] GetUVs () {
			var uvs = new UvRect[m_Sprites.Length];
			for (int i = 0; i < m_Sprites.Length; i++) {
				var sp = m_Sprites[i];
				uvs[i] = sp.Rect;
			}
			return uvs;
		}


		public Material GetMaterial () => new(Shader.Find(m_ShaderName)) {
			name = "Material-" + name,
			mainTexture = m_Texture,
			enableInstancing = true,
			mainTextureOffset = Vector2.zero,
			mainTextureScale = Vector2.one,
			doubleSidedGI = false,
			renderQueue = 3000
		};


#if UNITY_EDITOR

		public void SetSprites (Sprite[] sprites) {
			m_Sprites = new UvSprite[sprites.Length];
			if (sprites.Length == 0) { return; }
			float width = sprites[0].texture.width;
			float height = sprites[0].texture.height;
			for (int i = 0; i < sprites.Length; i++) {
				var sp = sprites[i];
				m_Sprites[i] = new UvSprite() {
					GlobalID = sp.name.AngeHash(),
					Rect = new UvRect() {
						BottomLeft = new(sp.rect.xMin / width, sp.rect.yMin / height),
						BottomRight = new(sp.rect.xMax / width, sp.rect.yMin / height),
						TopLeft = new(sp.rect.xMin / width, sp.rect.yMax / height),
						TopRight = new(sp.rect.xMax / width, sp.rect.yMax / height),
						Width = sp.rect.width.RoundToInt(),
						Height = sp.rect.height.RoundToInt(),
					},
				};
			}
		}


		public void SetUvSprites (UvSprite[] sprites) => m_Sprites = sprites;


		public void SetTexture (Texture2D texture) => m_Texture = texture;

#endif


	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	using System.Text;

	[CustomEditor(typeof(SpriteSheet), true), DisallowMultipleComponent]
	public class SpriteSheet_Inspector : Editor {


		private void OnEnable () => ReloadSprites();


		private void OnDisable () => ReloadSprites();


		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
			GUILayout.Space(4);
			if (GUI.Button(GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true)), "Reload Sprites from Texture")) {
				ReloadSprites();
			}
			if (target is SpriteSheetChar) {
				GUILayout.Space(4);
				if (GUI.Button(GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true)), "Load from Font")) {
					CreateFontTexture();
				}
			}
		}


		private void ReloadSprites () {
			serializedObject.Update();
			var p_Texture = serializedObject.FindProperty("m_Texture");
			var texture = p_Texture.objectReferenceValue as Texture2D;
			serializedObject.ApplyModifiedProperties();
			if (texture != null) {
				string path = AssetDatabase.GetAssetPath(texture);
				var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
				var spList = new List<Sprite>();
				for (int i = objs.Length - 1; i >= 0; i--) {
					var obj = objs[i];
					if (obj == null || obj is not Sprite sprite) { continue; }
					spList.Add(sprite);
				}
				if (spList.Count > 0) {
					(serializedObject.targetObject as SpriteSheet).SetSprites(spList.ToArray());
				}
			}
			serializedObject.ApplyModifiedProperties();
		}


		private void CreateFontTexture () {

			if (target is not SpriteSheetChar) { return; }

			string path = EditorUtility.OpenFilePanel("Pick a font", "Assets", "ttf");
			if (string.IsNullOrEmpty(path)) { return; }
			var font = AssetDatabase.LoadAssetAtPath<Font>("Assets" + path[Application.dataPath.Length..]);
			if (font == null) { return; }

			// Build Charset
			string charSetPath = EditorUtility.OpenFilePanel("Pick txt as char set", "Assets", "txt");
			if (!string.IsNullOrEmpty(charSetPath)) {
				var builder = new StringBuilder();
				for (int i = 1; i < 2048; i++) {
					builder.Append((char)i);
				}
				string bg2312 = Util.FileToText(charSetPath, Encoding.GetEncoding("gb2312"));
				builder.Append(bg2312);
				font.RequestCharactersInTexture(builder.ToString(), 0, FontStyle.Normal);
			}

			// Get Texture
			var fontTexture = font.material.mainTexture as Texture2D;
			var texture = new Texture2D(
				fontTexture.width, fontTexture.height,
				fontTexture.format, false
			);
			Graphics.CopyTexture(font.material.mainTexture, texture);

			// Get UV
			var uvList = new List<SpriteSheet.UvSprite>();
			var cSpriteList = new List<SpriteSheetChar.CharSprite>();

			foreach (var info in font.characterInfo) {
				string targetStr = char.ConvertFromUtf32(info.index);
				uvList.Add(new SpriteSheet.UvSprite() {
					GlobalID = ("c_" + targetStr).AngeHash(),
					Rect = new UvRect() {
						BottomLeft = info.uvBottomLeft,
						BottomRight = info.uvBottomRight,
						TopLeft = info.uvTopLeft,
						TopRight = info.uvTopRight,
					}
				});
				float size = info.size == 0 ? font.fontSize : info.size;

				cSpriteList.Add(new SpriteSheetChar.CharSprite() {
					FullWidth = targetStr.Length != Encoding.Default.GetByteCount(targetStr),
					UvOffset = Rect.MinMaxRect(
						info.minX / size, info.minY / size, info.maxX / size, info.maxY / size
					),
				});
			}

			// Data >> Sheet
			var sheet = target as SpriteSheetChar;
			sheet.SetUvSprites(uvList.ToArray());
			sheet.SetCharSprites(cSpriteList.ToArray());
			EditorUtility.SetDirty(sheet);

			// Texture >> File
			string resultPath = $"Assets/{font.name}.png";
			if (sheet.Texture != null) {
				resultPath = AssetDatabase.GetAssetPath(sheet.Texture);
			}
			Util.ByteToFile(texture.EncodeToPNG(), resultPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// Texture >> Sheet
			var resultTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(resultPath);
			sheet.SetTexture(resultTexture);
			EditorUtility.SetDirty(sheet);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

		}


	}
}
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;


namespace PixelJelly.Editor {
	public class PixelPostprocessor : AssetPostprocessor {



		// SUB
		public class TextureImportData {
			public int PixelPerUnit;
			public SpriteMetaData[] SpriteMetas;
		}


		public class AnimationFrameImportData {
			public string TexturePath;
			public string SpriteName;
			public float Duration;
		}


		public delegate void PostprocessorHandler (string path);


		// VAR
		private readonly static Dictionary<string, TextureImportData> ImportMapTexture = new Dictionary<string, TextureImportData>();
		private readonly static Dictionary<string, AnimationFrameImportData[]> ImportMapAnimation = new Dictionary<string, AnimationFrameImportData[]>();
		private readonly static EditorCurveBinding SPRITE_BINDING = new EditorCurveBinding() {
			path = "",
			propertyName = "m_Sprite",
			type = typeof(SpriteRenderer),
		};
		public static PostprocessorHandler OnTextureProcessed { get; set; } = null;



		// API
		public static void Clear () {
			ImportMapTexture.Clear();
			ImportMapAnimation.Clear();
		}


		public static void Add (string assetPath, TextureImportData data) {
			assetPath = EditorUtil.FixedRelativePath(assetPath);
			if (!ImportMapTexture.ContainsKey(assetPath)) {
				ImportMapTexture.Add(assetPath, data);
			}
		}


		public static void Add (string assetPath, AnimationFrameImportData[] datas) {
			assetPath = EditorUtil.FixedRelativePath(assetPath);
			if (!ImportMapAnimation.ContainsKey(assetPath)) {
				ImportMapAnimation.Add(assetPath, datas);
			} else {
				ImportMapAnimation[assetPath] = datas;
			}
		}


		// MSG
		private void OnPreprocessTexture () {

			TextureImporter ti = assetImporter as TextureImporter;
			if (ti == null) { return; }

			var path = EditorUtil.FixedRelativePath(ti.assetPath);
			if (!ImportMapTexture.ContainsKey(path)) { return; }
			var data = ImportMapTexture[path];
			ImportMapTexture.Remove(path);

			// Texture
			ti.isReadable = true;
			ti.textureType = TextureImporterType.Sprite;
			ti.spriteImportMode = SpriteImportMode.Multiple;
			ti.filterMode = FilterMode.Point;
			ti.textureCompression = TextureImporterCompression.Uncompressed;
			ti.mipmapEnabled = false;
			ti.alphaIsTransparency = true;
			ti.spritesheet = data.SpriteMetas;
			ti.spritePixelsPerUnit = data.PixelPerUnit;
			var textureSettings = new TextureImporterSettings();
			ti.ReadTextureSettings(textureSettings);
			textureSettings.spriteMeshType = SpriteMeshType.FullRect;
			textureSettings.spriteExtrude = 0;
			textureSettings.spriteGenerateFallbackPhysicsShape = true;
			ti.SetTextureSettings(textureSettings);

			OnTextureProcessed?.Invoke(path);

		}


		private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (ImportMapAnimation.Count == 0) { return; }

			var animationMap = new Dictionary<string, AnimationFrameImportData[]>(ImportMapAnimation);
			ImportMapAnimation.Clear();
			
			EditorApplication.delayCall += () => {

				bool needRefresh = false;

				// Create Animations
				foreach (var pair in animationMap) {
					if (pair.Value == null || pair.Value.Length == 0) { continue; }

					var importData = pair.Value;
					var path = pair.Key;

					// Old or New
					AnimationClip aniClip = null;
					if (Util.FileExists(path)) {
						aniClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
					}
					if (aniClip == null) {
						aniClip = new AnimationClip() {
							name = Util.GetNameWithoutExtension(path),
							wrapMode = WrapMode.Once,
						};
					}
					aniClip.frameRate = 120f;

					// Set Sprites
					float currentTime = 0f;
					var frameList = new List<ObjectReferenceKeyframe>();
					Texture2D texture = null;
					string texturePath = "";
					Object[] spritesInTexture = null;
					for (int i = 0; i < importData.Length; i++) {
						var frame = importData[i];
						float duration = Mathf.Max(frame.Duration, 0.001f);
						// Get Texture and Sprites Obj
						if (!texture || frame.TexturePath != texturePath) {
							texturePath = "";
							spritesInTexture = null;
							texture = AssetDatabase.LoadAssetAtPath<Texture2D>(frame.TexturePath);
							if (texture) {
								texturePath = frame.TexturePath;
								spritesInTexture = AssetDatabase.LoadAllAssetsAtPath(frame.TexturePath);
							}
						}
						// Get Sprite
						Sprite sprite = null;
						if (spritesInTexture != null) {
							for (int j = 0; j < spritesInTexture.Length; j++) {
								var sp = spritesInTexture[j] as Sprite;
								if (sp && sp.name == frame.SpriteName) {
									sprite = sp;
									break;
								}
							}
						}

						// Final
						frameList.Add(new ObjectReferenceKeyframe() { time = currentTime, value = sprite, });
						currentTime += duration;
					}
					frameList.Add(new ObjectReferenceKeyframe() { time = currentTime, value = frameList.Count == 0 ? null : frameList[frameList.Count - 1].value, });
					AnimationUtility.SetObjectReferenceCurve(aniClip, SPRITE_BINDING, frameList.ToArray());

					// Dirty Old or Create New
					if (Util.FileExists(path)) {
						EditorUtility.SetDirty(aniClip);
						needRefresh = true;
					} else {
						Util.CreateFolder(Util.GetParentPath(path));
						AssetDatabase.CreateAsset(aniClip, path);
					}
				}

				// Final
				if (needRefresh) {
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			};

		}



	}
}
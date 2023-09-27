using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using System.Linq;
using UnityEngine.Rendering;
using System.Text;
using UnityEngine.UIElements;



namespace AngeliaFramework.Editor {
	public static class EditorUtil {




		#region --- Path ---


		private static string ROOT_PATH = "";
		private static bool FixPathForIconLoading = true;


		public static string GetRootPath (string rootName, string packageName = "") {
			if (!string.IsNullOrEmpty(ROOT_PATH) && Util.FolderExists(ROOT_PATH)) { return ROOT_PATH; }
			var paths = AssetDatabase.GetAllAssetPaths();
			foreach (var path in paths) {
				if (Util.PathIsFolder(path) && Util.GetNameWithoutExtension(path) == rootName) {
					ROOT_PATH = FixedRelativePath(path);
					break;
				}
			}
			if (string.IsNullOrEmpty(ROOT_PATH) && !string.IsNullOrEmpty(packageName)) {
				ROOT_PATH = "Packages/" + packageName;
				FixPathForIconLoading = false;
			}
			return ROOT_PATH;
		}


		public static Texture2D GetImage (string rootName, string packageName, params string[] imagePath) =>
			GetAsset<Texture2D>(rootName, packageName, imagePath);


		public static T GetAsset<T> (string rootName, string packageName, params string[] assetPath) where T : Object {
			T result = null;
			string path = Util.CombinePaths(assetPath);
			path = Util.CombinePaths(GetRootPath(rootName, packageName), path);
			if (Util.FileExists(path)) {
				result = AssetDatabase.LoadAssetAtPath<T>(
					FixPathForIconLoading ? FixedRelativePath(path) : path
				);
			}
			return result;
		}


		public static string FixedRelativePath (string path, string packageRoot = "") {
			path = Util.FixPath(path);
			if (path.StartsWith("Assets")) {
				return path;
			}
			var fixedDataPath = Util.FixPath(Application.dataPath);
			if (path.StartsWith(fixedDataPath)) {
				return "Assets" + path[fixedDataPath.Length..];
			} else if (!string.IsNullOrEmpty(packageRoot)) {
				packageRoot = "Packages/" + packageRoot;
				string packageFullPath = Util.FixPath(Util.GetFolderFullPath(packageRoot));
				if (path.StartsWith(packageFullPath)) {
					return packageRoot + "/" + path[packageRoot.Length..];
				}
				return "";
			} else {
				return "";
			}
		}


		#endregion




		#region --- Message ---


		public static bool Dialog (string title, string msg, string ok, string cancel = "") {
			if (string.IsNullOrEmpty(cancel)) {
				return EditorUtility.DisplayDialog(title, msg, ok);
			} else {
				return EditorUtility.DisplayDialog(title, msg, ok, cancel);
			}
		}


		public static int DialogComplex (string title, string msg, string labelA, string labelB, string labelC) {
			int index = EditorUtility.DisplayDialogComplex(title, msg, labelA, labelC, labelB);
			return
				index == 1 ? 2 :
				index == 2 ? 1 :
				0;
		}


		public static void ProgressBar (string title, string msg, float value) {
			value = Mathf.Clamp01(value);
			EditorUtility.ClearProgressBar();
			EditorUtility.DisplayProgressBar(title, msg, value);
		}


		public static void ClearProgressBar () {
			EditorUtility.ClearProgressBar();
		}


		#endregion




		#region --- Misc ---


		public static bool GetExpandComponent<T> () where T : Component {
			bool result = false;
			var g = new GameObject("", typeof(T));
			try {
				g.hideFlags = HideFlags.HideAndDontSave;
				result = UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(
					g.GetComponent(typeof(T))
				);
			} catch { }
			Object.DestroyImmediate(g, false);
			return result;
		}


		public static void SetExpandComponent<T> (bool expand) where T : Component {
			var g = new GameObject("", typeof(T));
			try {
				g.hideFlags = HideFlags.HideAndDontSave;
				UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(
					g.GetComponent(typeof(T)), expand
				);
			} catch { }
			Object.DestroyImmediate(g, false);


		}


		public static Texture2D GetFixedAssetPreview (Object obj) {

			var texture = AssetPreview.GetAssetPreview(obj);
			if (texture == null) { return null; }
			int width = texture.width;
			int height = texture.height;
			if (width == 0 || height == 0) { return null; }
			var pixels = texture.GetPixels32();
			int length = width * height;

			// Remove Background
			Color32 CLEAR = new(0, 0, 0, 0);
			var stack = new Stack<(int x, int y)>();
			RemoveColorAt(0, 0);
			RemoveColorAt(width - 1, 0);
			RemoveColorAt(0, height - 1);
			RemoveColorAt(width - 1, height - 1);

			// Fix Color Brightness
			Color32 pixel;
			for (int i = 0; i < length; i++) {
				pixel = pixels[i];
				if (pixel.a == 0) { continue; }
				pixel.r = (byte)Mathf.Clamp((pixel.r - 128f) * 1.5f + 190f, byte.MinValue, byte.MaxValue);
				pixel.g = (byte)Mathf.Clamp((pixel.g - 128f) * 1.5f + 190f, byte.MinValue, byte.MaxValue);
				pixel.b = (byte)Mathf.Clamp((pixel.b - 128f) * 1.5f + 190f, byte.MinValue, byte.MaxValue);
				pixels[i] = pixel;
			}

			// Final
			var result = new Texture2D(width, height, TextureFormat.RGBA32, false) {
				alphaIsTransparency = true,
				filterMode = FilterMode.Point
			};
			result.SetPixels32(pixels);
			result.Apply();
			return result;

			// === Func ===
			bool SameColor (Color32 colorA, Color32 colorB) =>
				colorA.r == colorB.r &&
				colorA.g == colorB.g &&
				colorA.b == colorB.b &&
				colorA.a == colorB.a;
			void RemoveColorAt (int _x, int _y) {
				var color32 = pixels[_y * width + _x];
				if (color32.a == 0) { return; }
				stack.Clear();
				stack.Push((_x, _y));
				for (int safeCount = 0; safeCount < length * 8 && stack.Count > 0; safeCount++) {
					(int x, int y) = stack.Pop();
					pixels[y * width + x] = CLEAR;
					AddToStack(x, y - 1, color32);
					AddToStack(x, y + 1, color32);
					AddToStack(x - 1, y, color32);
					AddToStack(x + 1, y, color32);
					AddToStack(x - 1, y - 1, color32);
					AddToStack(x - 1, y + 1, color32);
					AddToStack(x + 1, y - 1, color32);
					AddToStack(x + 1, y + 1, color32);
				}
			}
			void AddToStack (int x, int y, Color32 color32) {
				int i = y * width + x;
				if (
					x >= 0 && y >= 0 && x < width && y < height &&
					SameColor(color32, pixels[i])
				) {
					stack.Push((x, y));
				}
			}
		}


		public static List<SerializedProperty> GetProps (ScriptableObject source, SerializedObject sObj) {
			var type = source.GetType();
			var pubList = new List<FieldInfo>(type.GetFields(BindingFlags.Instance | BindingFlags.Public));
			for (int i = 0; i < pubList.Count; i++) {
				var field = pubList[i];
				if (field.GetCustomAttribute<HideInInspector>() != null) {
					pubList.RemoveAt(i);
					i--;
				}
			}
			var priList = new List<FieldInfo>(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
			for (int i = 0; i < priList.Count; i++) {
				var field = priList[i];
				if (field.GetCustomAttribute<HideInInspector>() != null || field.GetCustomAttribute<SerializeField>() == null) {
					priList.RemoveAt(i);
					i--;
				}
			}
			var pList = new List<SerializedProperty>();
			foreach (var field in pubList) {
				var p = sObj.FindProperty(field.Name);
				if (p != null) {
					pList.Add(p);
				}
			}
			foreach (var field in priList) {
				var p = sObj.FindProperty(field.Name);
				if (p != null) {
					pList.Add(p);
				}
			}
			return pList;
		}


		public static IEnumerable<T> ForAllAssets<T> () where T : Object {
			foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}")) {
				var _path = AssetDatabase.GUIDToAssetPath(guid);
				var obj = AssetDatabase.LoadAssetAtPath<T>(_path);
				if (obj is T t) yield return t;
			}
		}


		public static IEnumerable<Object> ForAllAssets (System.Type type) {
			foreach (var guid in AssetDatabase.FindAssets($"t:{type.Name}")) {
				var _path = AssetDatabase.GUIDToAssetPath(guid);
				var obj = AssetDatabase.LoadAssetAtPath(_path, type);
				yield return obj;
			}
		}


		public static IEnumerable<(T, string)> ForAllAssetsWithPath<T> () where T : Object {
			foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}")) {
				var _path = AssetDatabase.GUIDToAssetPath(guid);
				var obj = AssetDatabase.LoadAssetAtPath<T>(_path);
				if (obj is T t) yield return (t, _path);
			}
		}


		public static IEnumerable<string> ForAllPackages () {
			var req = Client.List(true, false);
			while (!req.IsCompleted) { }
			if (req.Status == StatusCode.Success) {
				foreach (var package in req.Result) {
					yield return "Packages/" + package.name;
				}
			}
		}


		public static void AddAlwaysIncludedShader (string shaderName) {
			var shader = Shader.Find(shaderName);
			if (shader == null)
				return;

			var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
			var serializedObject = new SerializedObject(graphicsSettingsObj);
			var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
			bool hasShader = false;
			for (int i = 0; i < arrayProp.arraySize; ++i) {
				var arrayElem = arrayProp.GetArrayElementAtIndex(i);
				if (shader == arrayElem.objectReferenceValue) {
					hasShader = true;
					break;
				}
			}

			if (!hasShader) {
				int arrayIndex = arrayProp.arraySize;
				arrayProp.InsertArrayElementAtIndex(arrayIndex);
				var arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
				arrayElem.objectReferenceValue = shader;

				serializedObject.ApplyModifiedProperties();

			}

		}


		public static bool CreateFontTexture (Font font, string charset, bool fixOffsetY, out Texture2D texture, out CharacterInfo[] infos) {

			texture = null;
			infos = null;
			if (font == null) { return false; }

			// Build Charset
			var builder = new StringBuilder();
			for (int i = 1; i < 2048; i++) {
				builder.Append((char)i);
			}

			int offsetY = 0;
			if (fixOffsetY) {
				font.RequestCharactersInTexture("_", 0, FontStyle.Normal);
				if (font.GetCharacterInfo('_', out var info)) {
					offsetY = -info.minY;
				}
			}
			builder.Append(charset);
			font.RequestCharactersInTexture(builder.ToString(), 0, FontStyle.Normal);

			// Get Texture
			var fontTexture = font.material.mainTexture as Texture2D;
			texture = new Texture2D(
				fontTexture.width, fontTexture.height,
				fontTexture.format, false
			);
			Graphics.CopyTexture(font.material.mainTexture, texture);
			infos = font.characterInfo;
			if (offsetY != 0) {
				for (int i = 0; i < infos.Length; i++) {
					infos[i].minY += offsetY;
					infos[i].maxY += offsetY;
				}
			}
			return true;
		}


		public static void InjectVisualTreeToEditorWindow (EditorWindow window, string assetName, System.Action<VisualElement> callback) {
			if (window.rootVisualElement.Children().Any(v => v.name == assetName)) return;
			foreach (var guid in AssetDatabase.FindAssets(assetName)) {
				string uxmlPath = AssetDatabase.GUIDToAssetPath(guid);
				if (Util.GetExtension(uxmlPath) != ".uxml") continue;
				var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
				if (tree == null) continue;
				InjectVisualTreeToEditorWindow(window, tree, callback);
				break;
			}
		}


		public static void InjectVisualTreeToEditorWindow (EditorWindow window, VisualTreeAsset asset, System.Action<VisualElement> callback) {
			if (asset == null || window.rootVisualElement.Children().Any(v => v.name == asset.name)) return;
			var root = new VisualElement() { name = asset.name, };
			asset.CloneTree(root);
			callback?.Invoke(root);
			window.rootVisualElement.Insert(0, root);
		}


		public static void MoveFileToTrash (string path) {
			if (!Util.FileExists(path)) return;
			if (path.StartsWith("Assets")) {
				AssetDatabase.MoveAssetToTrash(path);
			} else {
				string tempFolder = Util.CombinePaths("Assets", GUID.Generate().ToString());
				string tempPath = Util.CombinePaths(tempFolder, Util.GetNameWithExtension(path));
				try {
					Util.CreateFolder(tempFolder);
					Util.MoveFile(path, tempPath);
					AssetDatabase.Refresh();
					AssetDatabase.MoveAssetToTrash(tempPath);
				} catch { }
				AssetDatabase.DeleteAsset(tempFolder);
				AssetDatabase.Refresh();
			}
		}


		public static void MoveFilesToTrash (string[] paths) {
			if (paths == null || paths.Length == 0) return;
			string tempFolder = Util.CombinePaths("Assets", GUID.Generate().ToString());
			Util.CreateFolder(tempFolder);
			AssetDatabase.Refresh();
			foreach (var path in paths) {
				if (!Util.FileExists(path)) continue;
				try {
					if (path.StartsWith("Assets")) {
						AssetDatabase.MoveAssetToTrash(path);
					} else {
						string tempPath = Util.CombinePaths(tempFolder, Util.GetNameWithExtension(path));
						Util.MoveFile(path, tempPath);
						AssetDatabase.MoveAssetToTrash(tempPath);
					}
				} catch { }
			}
			AssetDatabase.DeleteAsset(tempFolder);
			AssetDatabase.Refresh();
		}


		public static void MoveFolderToTrash (string path) {
			if (!Util.FolderExists(path)) return;
			if (path.StartsWith("Assets")) {
				AssetDatabase.MoveAssetToTrash(path);
			} else {
				string tempFolder = Util.CombinePaths("Assets", GUID.Generate().ToString());
				string tempPath = Util.CombinePaths(tempFolder, Util.GetNameWithExtension(path));
				try {
					Util.CreateFolder(tempFolder);
					Util.MoveFolder(path, tempPath);
					AssetDatabase.Refresh();
					AssetDatabase.MoveAssetToTrash(tempPath);
				} catch { }
				AssetDatabase.DeleteAsset(tempFolder);
				AssetDatabase.Refresh();
			}
		}


		public static void OpenFileWithCodeEditor (string filePath, int line = -1, int column = -1) {
			var editor = Unity.CodeEditor.CodeEditor.Editor;
			if (editor != null && editor.CurrentCodeEditor != null) {
				editor.CurrentCodeEditor.OpenProject(filePath, line, column);
			} else {
				Application.OpenURL(Util.GetUrl(filePath));
			}
		}


		#endregion




	}


}
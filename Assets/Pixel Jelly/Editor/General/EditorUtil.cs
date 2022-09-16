using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;





namespace PixelJelly.Editor {
	public static class EditorUtil {




		#region --- Path ---


		private static string ROOT_PATH = "";
		private static bool PACKAGE_ROOT = false;


		public static string GetRootPath (string rootName, string packageName = "") {
			if (string.IsNullOrEmpty(ROOT_PATH) || (!PACKAGE_ROOT && !Util.FolderExists(ROOT_PATH))) {
				ROOT_PATH = "";
				var paths = AssetDatabase.GetAllAssetPaths();
				foreach (var path in paths) {
					if (!Util.PathIsDirectory(path)) { continue; }
					bool isPackage = path.StartsWith("Packages");
					if (Util.GetNameWithExtension(path) == (isPackage ? packageName : rootName)) {
						ROOT_PATH = FixedRelativePath(path);
						PACKAGE_ROOT = isPackage;
						break;
					}
				}
			}
			return string.IsNullOrEmpty(packageName) && PACKAGE_ROOT ? "" : ROOT_PATH;
		}


		public static Texture2D GetImage (string rootName, string packageName, params string[] imagePath) =>
			GetAsset<Texture2D>(rootName, packageName, imagePath);


		public static T GetAsset<T> (string rootName, string packageName, params string[] assetPath) where T : Object {
			T result = null;
			string path = Util.CombinePaths(assetPath);
			path = Util.CombinePaths(GetRootPath(rootName, packageName), path);
			if (Util.FileExists(path)) {
				result = AssetDatabase.LoadAssetAtPath<T>(FixedRelativePath(path));
			}
			return result;
		}


		public static string FixedRelativePath (string path) {
			path = Util.FixPath(path);
			if (path.StartsWith("Assets") || path.StartsWith("Packages")) {
				return path;
			}
			var fixedDataPath = Util.FixPath(Application.dataPath);
			if (path.StartsWith(fixedDataPath)) {
				return "Assets" + path.Substring(fixedDataPath.Length);
			} else {
				return path;
			}
		}


		#endregion




		#region --- Message ---


		public static bool Dialog (string title, string msg, string ok, string cancel = "") {

			if (string.IsNullOrEmpty(cancel)) {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok);

				return sure;
			} else {
				bool sure = EditorUtility.DisplayDialog(title, msg, ok, cancel);

				return sure;
			}
		}


		public static int DialogComplex (string title, string msg, string ok, string cancel, string alt) {
			//EditorApplication.Beep();

			int index = EditorUtility.DisplayDialogComplex(title, msg, ok, cancel, alt);

			return index;
		}


		#endregion




		#region --- Misc ---


		public static List<SerializedProperty> GetInspectorProps (System.Type type, SerializedObject sObj, out List<IEnumerable<CustomAttributeData>> attributes, bool nullGap = false) {
			var propList = new List<FieldInfo>();
			for (var t = type; t != null && t != typeof(ScriptableObject); t = t.BaseType) {
				var fields = t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (fields == null || fields.Length == 0) { continue; }
				if (nullGap) {
					propList.Insert(0, null);
				}
				propList.InsertRange(0, fields);
			}
			for (int i = 0; i < propList.Count; i++) {
				var field = propList[i];
				if (field == null) { continue; }
				if (field.IsPublic) {
					if (field.GetCustomAttribute<HideInInspector>() != null) {
						propList.RemoveAt(i);
						i--;
					}
				} else {
					if (field.GetCustomAttribute<HideInInspector>() != null || field.GetCustomAttribute<SerializeField>() == null) {
						propList.RemoveAt(i);
						i--;
					}
				}
			}
			attributes = new List<IEnumerable<CustomAttributeData>>();
			var pList = new List<SerializedProperty>();
			foreach (var field in propList) {
				if (field == null) {
					if (nullGap) {
						pList.Add(null);
						attributes.Add(null);
					}
					continue;
				}
				var p = sObj.FindProperty(field.Name);
				if (p != null) {
					pList.Add(p);
					attributes.Add(field.CustomAttributes);
				}
			}
			return pList;
		}


		public static Color32? GetColorFromClipboard () {
			if (ColorUtility.TryParseHtmlString(EditorGUIUtility.systemCopyBuffer, out var result)) {
				return result;
			}
			return null;
		}


		public static int ShowColorPicker (Color color) {
			try {
				int id = GUIUtility.GetControlID("s_ColorHash".GetHashCode(), FocusType.Keyboard);
				var assembly = Assembly.GetAssembly(typeof(EditorWindow));
				var t_ColorPicker = assembly.GetType("UnityEditor.ColorPicker");
				var t_GUIView = assembly.GetType("UnityEditor.GUIView");
				var m_GetCurrent = t_GUIView.GetProperty("current", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
				var method = t_ColorPicker.GetMethod("Show", new System.Type[] { t_GUIView, typeof(Color), typeof(bool), typeof(bool) });
				GUIUtility.keyboardControl = id;
				method.Invoke(null, new object[] {
					m_GetCurrent.Invoke(null, null),
					color, true, false
				});
				return id;
			} catch (System.Exception ex) {
				Debug.LogException(ex);
			}
			return -1;
		}


		public static bool ColorPickerChanged (int id, out Color color) {
			color = default;
			try {
				var assembly = Assembly.GetAssembly(typeof(EditorWindow));
				var t_ColorPicker = assembly.GetType("UnityEditor.ColorPicker");
				int m_okcID = (int)t_ColorPicker.GetProperty("originalKeyboardControl", BindingFlags.Public | BindingFlags.Static).GetGetMethod().Invoke(null, null);
				if (GUIUtility.keyboardControl == id || m_okcID == id) {
					if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ColorPickerChanged") {
						var m_color = t_ColorPicker.GetProperty("color", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
						color = (Color)m_color.Invoke(null, null);
						return true;
					}
				}
			} catch (System.Exception ex) {
				Debug.LogException(ex);
			}
			return false;
		}


		#endregion




	}


}
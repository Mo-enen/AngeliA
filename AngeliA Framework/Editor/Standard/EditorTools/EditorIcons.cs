using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;

namespace Moenen.Editor {
	public class EditorIcons : EditorWindow {


		static bool darkPreview = true;
		static GUIContent iconSelected;
		static List<GUIContent> iconContentListAll;
		static List<string> iconMissingNames;
		static GUIStyle iconButtonStyle = null;
		static GUIStyle iconPreviewBlack = null;
		static GUIStyle iconPreviewWhite = null;
		static string[] ico_list = new string[0];
		Vector2 scroll;
		int buttonSize = 70;
		string search = "";



		[MenuItem("AngeliA/Other/Editor Icons", priority = -1001)]
		public static void EditorIconsOpen () {
			var w = GetWindow<EditorIcons>(true, "Editor Icons", true);
			w.minSize = new Vector2(320, 450);
		}


		private void OnGUI () {
			var ppp = EditorGUIUtility.pixelsPerPoint;

			InitIcons();

			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
				GUILayout.Space(10);
				search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
				if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(48))) {
					DeleteIconListFile();
					iconContentListAll = null;
					InitIcons();
				}
			}

			GUILayout.Space(3);

			using (var scope = new GUILayout.ScrollViewScope(scroll)) {
				GUILayout.Space(10);

				scroll = scope.scrollPosition;

				buttonSize = 40;

				var render_width = (Screen.width / ppp - 13f);
				var gridW = Mathf.FloorToInt(render_width / buttonSize);
				var margin_left = (render_width - buttonSize * gridW) / 2;

				int row = 0, index = 0;

				List<GUIContent> iconList = iconContentListAll;

				if (!string.IsNullOrWhiteSpace(search) && search != "") {
					iconList = iconContentListAll.Where(x => x.tooltip.ToLower().Contains(search.ToLower())).ToList();
				}

				while (index < iconList.Count) {
					using (new GUILayout.HorizontalScope()) {
						GUILayout.Space(margin_left);

						for (var i = 0; i < gridW; ++i) {
							int k = i + row * gridW;

							var icon = iconList[k];

							if (GUILayout.Button(icon,
								iconButtonStyle,
								GUILayout.Width(buttonSize),
								GUILayout.Height(buttonSize))) {
								EditorGUI.FocusTextInControl("");
								iconSelected = icon;
							}

							index++;

							if (index == iconList.Count) break;
						}
					}

					row++;
				}

				GUILayout.Space(10);
			}


			if (iconSelected == null) return;

			GUILayout.FlexibleSpace();

			using (new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.MaxHeight(120))) {
				using (new GUILayout.VerticalScope(GUILayout.Width(130))) {
					GUILayout.Space(2);

					GUILayout.Button(iconSelected,
						darkPreview ? iconPreviewBlack : iconPreviewWhite,
						GUILayout.Width(128), GUILayout.Height(40));

					GUILayout.Space(5);

					darkPreview = GUILayout.SelectionGrid(
					  darkPreview ? 1 : 0, new string[] { "Light", "Dark" },
					  2, EditorStyles.miniButton) == 1;

					GUILayout.FlexibleSpace();
				}

				GUILayout.Space(10);

				using (new GUILayout.VerticalScope()) {
					var s = $"Size: {iconSelected.image.width}x{iconSelected.image.height}";
					s += "\nIs Pro Skin Icon: " + (iconSelected.tooltip.IndexOf("d_") == 0 ? "Yes" : "No");
					s += $"\nTotal {iconContentListAll.Count} icons";
					GUILayout.Space(5);
					EditorGUILayout.HelpBox(s, MessageType.None);
					GUILayout.Space(5);
					EditorGUILayout.TextField("EditorGUIUtility.IconContent(\"" + iconSelected.tooltip + "\")");
					GUILayout.Space(5);
					if (GUILayout.Button("Copy to clipboard", EditorStyles.miniButton))
						EditorGUIUtility.systemCopyBuffer = iconSelected.tooltip;
					if (GUILayout.Button("Export", EditorStyles.miniButton)) {
						var texture2d = iconSelected.image as Texture2D;
						var texture = new Texture2D(texture2d.width, texture2d.height) {
							alphaIsTransparency = true,
							mipMapBias = texture2d.mipMapBias,
							minimumMipmapLevel = texture2d.mipmapCount,
							requestedMipmapLevel = texture2d.mipmapCount,
						};
						texture.SetPixels32(new Color32[texture2d.width * texture2d.height]);
						texture.Apply();
						Graphics.CopyTexture(texture2d, 0, 0, texture, 0, 0);
						ByteToFile(texture.EncodeToPNG(), $"Assets/{iconSelected.tooltip}.png");
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
					}
				}

				GUILayout.Space(10);

				if (GUILayout.Button("X", GUILayout.ExpandHeight(true))) {
					iconSelected = null;
				}

			}
		}


		private void OnDestroy () {
			iconContentListAll = null;
			System.GC.Collect();
		}


		Texture2D Texture2DPixel (Color c) {
			Texture2D t = new(1, 1);
			t.SetPixel(0, 0, c);
			t.Apply();
			return t;
		}


		void InitIcons () {
			if (iconContentListAll != null) return;

			LoadIconListFromFile();

			iconButtonStyle = new GUIStyle(EditorStyles.miniButton) {
				margin = new RectOffset(0, 0, 0, 0),
				fixedHeight = 0
			};

			iconPreviewBlack = new GUIStyle(iconButtonStyle);
			AllTheTEXTURES(ref iconPreviewBlack, Texture2DPixel(new Color(0.15f, 0.15f, 0.15f)));

			iconPreviewWhite = new GUIStyle(iconButtonStyle);
			AllTheTEXTURES(ref iconPreviewWhite, Texture2DPixel(new Color(0.85f, 0.85f, 0.85f)));

			iconMissingNames = new List<string>();
			iconContentListAll = new List<GUIContent>();

			for (var i = 0; i < ico_list.Length; ++i) {
				var ico = GetIcon(ico_list[i]);
				if (ico == null) {
					iconMissingNames.Add(ico_list[i]);
					continue;
				}
				ico.tooltip = ico_list[i];
				iconContentListAll.Add(ico);
				static GUIContent GetIcon (string icon_name) {
					GUIContent valid = null;
					Debug.unityLogger.logEnabled = false;
					if (!string.IsNullOrEmpty(icon_name)) valid = EditorGUIUtility.IconContent(icon_name);
					Debug.unityLogger.logEnabled = true;
					return valid?.image == null ? null : valid;
				}
			}
		}


		void DeleteIconListFile () {
			string path = GetCacheFilePath();
			if (File.Exists(path)) {
				File.Delete(path);
			}
		}


		void LoadIconListFromFile () {
			string path = GetCacheFilePath();
			if (!File.Exists(path)) {
				CreateCacheFileFromResource(path);
			}
			if (!File.Exists(path)) {
				Debug.LogWarning("Failed to create cache file");
				return;
			}
			var list = new List<string>();
			using StreamReader sr = new(path, System.Text.Encoding.ASCII);
			while (sr.Peek() >= 0) {
				string line = sr.ReadLine();
				list.Add(line);
			}
			ico_list = list.ToArray();
		}


		void CreateCacheFileFromResource (string cachePath) {
			string baseFolder = System.AppDomain.CurrentDomain.BaseDirectory;
			string sourcePath = Path.Combine(baseFolder, "Data", "Resources", "unity editor resources");
			if (!File.Exists(sourcePath)) return;
			string tempPath = $"Assets/{GUID.Generate()}.asset";
			File.Copy(sourcePath, tempPath);
			Debug.unityLogger.logEnabled = false;
			var result = new List<string>();
			try {
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(tempPath);
				foreach (var obj in objs) {
					if (obj is not Texture texture) continue;
					result.Add(obj.name);
				}
			} catch { }
			ClearConsole();
			Debug.unityLogger.logEnabled = true;
			if (File.Exists(tempPath)) File.Delete(tempPath);
			if (File.Exists(tempPath + ".meta")) File.Delete(tempPath + ".meta");
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			var builder = new StringBuilder();
			builder.AppendJoin('\n', result.Distinct());
			TextToFile(builder.ToString(), cachePath, Encoding.ASCII);
		}


		string GetCacheFilePath () {
			var script = MonoScript.FromScriptableObject(this);
			string scriptPath = AssetDatabase.GetAssetPath(script);
			return Path.ChangeExtension(scriptPath, "cache");
		}


		#region --- UTL ---


		static void AllTheTEXTURES (ref GUIStyle s, Texture2D t) {
			s.hover.background = s.onHover.background = s.focused.background = s.onFocused.background = s.active.background = s.onActive.background = s.normal.background = s.onNormal.background = t;
			s.hover.scaledBackgrounds = s.onHover.scaledBackgrounds = s.focused.scaledBackgrounds = s.onFocused.scaledBackgrounds = s.active.scaledBackgrounds = s.onActive.scaledBackgrounds = s.normal.scaledBackgrounds = s.onNormal.scaledBackgrounds = new Texture2D[] { t };
		}


		static void ByteToFile (byte[] bytes, string path) {
			string parentPath = GetParentPath(path);
			CreateFolder(parentPath);
			FileStream fs = new(path, FileMode.Create, FileAccess.Write);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();
			fs.Dispose();
		}


		static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !FolderExists(path)) {
				string pPath = GetParentPath(path);
				if (!FolderExists(pPath)) {
					CreateFolder(pPath);
				}
				Directory.CreateDirectory(path);
			}
		}


		static string GetParentPath (string path) => Directory.GetParent(path).FullName;


		static bool FolderExists (string path) => Directory.Exists(path);


		static void TextToFile (string data, string path, Encoding encoding) {
			CreateFolder(GetParentPath(path));
			using FileStream fs = new(path, FileMode.Create);
			using StreamWriter sw = new(fs, encoding);
			sw.Write(data);
			sw.Close();
			fs.Close();
		}


		static void ClearConsole () {
			try {
				var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
				var type = assembly.GetType("UnityEditorInternal.LogEntries");
				if (type == null) {
					type = assembly.GetType("UnityEditor.LogEntries");
				}
				var method = type.GetMethod("Clear");
				method.Invoke(new object(), null);
			} catch { }
		}


		#endregion




	}
}
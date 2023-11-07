using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;
using AngeliaFramework;


namespace AngeliaFramework.Editor {

	public static class MoenenTools {



		// VAR
		private static long PrevDoTheThingTime = 0;
		private static int DoTheTingCombo = 0;
		private static bool PrevUnityFocused = true;

		// Project Highlight
		private static double LastCacheUpdateTime = -3d;
		private static EditorWindow ProjectWindow = null;



		[InitializeOnLoadMethod]
		public static void EditorInit () {

			// Scene Camera 3D
			SceneView.duringSceneGui += MoenenSceneCamera3D;

			// Update
			EditorApplication.update += () => {
				// Deselect Script File 
				bool focused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
				if (!focused && PrevUnityFocused) {
					if (Selection.objects.Any(o => o is MonoScript)) {
						Selection.activeObject = null;
					}
				}
				PrevUnityFocused = focused;

			};
		}



		[InitializeOnLoadMethod]
		public static void ProjectWindowHighlightInit () {

			// Update
			EditorApplication.update += () => {
				if (ProjectWindow != null || EditorApplication.timeSinceStartup <= LastCacheUpdateTime + 2d) return;
				LastCacheUpdateTime = EditorApplication.timeSinceStartup;
				foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>()) {
					if (window.GetType().Name.EndsWith("ProjectBrowser")) {
						window.wantsMouseMove = true;
						ProjectWindow = window;
						break;
					}
				}
			};

			// Project Item GUI
			EditorApplication.projectWindowItemOnGUI -= ProjectItemGUI;
			EditorApplication.projectWindowItemOnGUI += ProjectItemGUI;
			static void ProjectItemGUI (string guid, Rect selectionRect) {
				selectionRect.width += selectionRect.x;
				selectionRect.x = 0;
				if (selectionRect.Contains(Event.current.mousePosition)) {
					EditorGUI.DrawRect(selectionRect, new Color(1, 1, 1, 0.06f));
				}
				if (EditorWindow.mouseOverWindow != null && Event.current.type == EventType.MouseMove) {
					EditorWindow.mouseOverWindow.Repaint();
					Event.current.Use();
				}
			}

		}


		// & alt   % ctrl   # Shift
		[MenuItem("AngeliA/Other/Do the Thing _F5")]
		public static void DoTheThing () {

			// Clear Console
			var assembly = Assembly.GetAssembly(typeof(ActiveEditorTracker));
			var type = assembly.GetType("UnityEditorInternal.LogEntries") ?? assembly.GetType("UnityEditor.LogEntries");
			var method = type.GetMethod("Clear");
			method.Invoke(new object(), null);

			// Save
			if (!EditorApplication.isPlaying) {
				EditorSceneManager.SaveOpenScenes();
			}

			// Compile if need
			AssetDatabase.Refresh();

			// Combo
			long time = System.DateTime.Now.Ticks;
			if (time - PrevDoTheThingTime < 5000000) {
				if (DoTheTingCombo == 0) {

					// Deselect
					Selection.activeObject = null;

				} else if (DoTheTingCombo == 1) {

				}
				DoTheTingCombo++;
			} else {
				DoTheTingCombo = 0;
			}
			PrevDoTheThingTime = time;

		}



		[MenuItem("Assets/Create/Package Json", priority = 99)]
		public static void CreatePackageJson () {
			string rootPath = "Assets";
			string packageName = "Package";
			if (Selection.activeObject != null) {
				string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
				if (!string.IsNullOrEmpty(selectionPath)) {
					rootPath = PathIsFolder(selectionPath) ?
						selectionPath :
						GetParentPath(selectionPath);
					string rootName = GetNameWithoutExtension(rootPath);
					if (rootName != "Assets") {
						packageName = rootName.Replace(" ", "");
					}
				}
			}
			var builder = new StringBuilder();
			builder.AppendLine("{");
			builder.AppendLine(@$"  ""name"": ""com.{Application.companyName.ToLower()}.{packageName.ToLower()}"", ");
			builder.AppendLine(@$"  ""displayName"": ""{packageName}"",");
			builder.AppendLine(@"  ""version"": ""1.0.0"",");
			builder.AppendLine(@"  ""description"": """",");
			builder.AppendLine(@"  ""type"": """",");
			builder.AppendLine(@"  ""hideInEditor"": false,");
			builder.AppendLine(@"  ""dependencies"": { }");
			builder.AppendLine("}");
			TextToFile(
				builder.ToString(),
				FixedRelativePath(CombinePaths(rootPath, "package.json"))
			);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}



		[MenuItem("AngeliA/Other/Check for Empty Scripts")]
		public static void EmptyScriptChecker () {
			int resultCount = 0;
			resultCount += Check(Application.dataPath);
			foreach (string path in ForAllPackages()) {
				resultCount += Check(new DirectoryInfo(path).FullName);
			}
			if (resultCount == 0) {
				Debug.Log("No empty script founded.");
			}
			static int Check (string root) {
				int count = 0;
				foreach (var path in EnumerateFiles(root, false, "*.cs")) {
					foreach (string line in ForAllLines(path)) {
						if (!string.IsNullOrWhiteSpace(line)) {
							goto _NEXT_;
						}
					}
					Debug.Log($"{path}\nis empty.");
					count++;
					_NEXT_:;
				}
				return count;
			}
		}



		[MenuItem("AngeliA/Other/Check for Empty Sprites")]
		public static void EmptySpriteChecker () {

			string sheetPath = Util.CombinePaths(AngePath.SheetRoot, $"{nameof(SpriteSheet)}.json");
			if (!Util.FileExists(sheetPath)) {
				Debug.LogWarning("Sprite sheet not found.");
				return;
			}
			var sheet = AngeUtil.LoadJson<SpriteSheet>(AngePath.SheetRoot);
			if (sheet == null) {
				Debug.LogWarning("Failed to load sprite sheet.");
				return;
			}

			var game = Object.FindFirstObjectByType<Game>(FindObjectsInactive.Include);
			if (game == null) {
				Debug.LogWarning("Game not found.");
				return;
			}

			var texture = AngeUtil.LoadSheetTexture();
			if (texture == null) {
				Debug.LogWarning("Sheet texture not found.");
				return;
			}

			var pixels = texture.GetPixels32();

			// Name Pool from Editing Meta
			var editingMeta = AngeUtil.LoadJson<SpriteEditingMeta>(AngePath.SheetRoot);
			if (editingMeta == null) {
				Debug.LogWarning("Sprite editing meta not found.");
				return;
			}
			var namePool = new Dictionary<int, (string name, string sheetName)>();
			foreach (var meta in editingMeta.Metas) {
				namePool.TryAdd(
					meta.GlobalID,
					(meta.RealName, editingMeta.SheetNames[meta.SheetNameIndex])
				);
			}

			// Check all Sprites
			var resultList = new List<string>();
			int textureWidth = texture.width;
			int textureHeight = texture.height;
			foreach (var sprite in sheet.Sprites) {
				var rect = sprite.GetTextureRect(textureWidth, textureHeight);
				int xMax = rect.xMax.Clamp(0, textureWidth);
				int yMax = rect.yMax.Clamp(0, textureHeight);
				for (int x = rect.x; x < xMax; x++) {
					for (int y = rect.y; y < yMax; y++) {
						if (pixels[y * textureWidth + x].a > 0) {
							goto _PASS;
						}
					}
				}
				if (namePool.TryGetValue(sprite.GlobalID, out var nameData)) {
					resultList.Add($"Sheet: <color=#FFCC00>{nameData.sheetName}</color> Name: <color=#FFCC00>{nameData.name}</color>");
				}
				_PASS:;
			}

			// Log
			if (resultList.Count == 0) {
				Debug.Log("No empty sprite found.");
			} else {
				Debug.Log($"{resultList.Count} empty sprites found.");
				foreach (var msg in resultList) {
					Debug.Log(msg);
				}
			}

		}


		#region --- LGC ---


		private static void MoenenSceneCamera3D (SceneView sceneView) {
			// Moenen's Scene Camera
			if (!sceneView.in2DMode) {
				switch (Event.current.type) {
					case EventType.MouseDrag:
						if (Event.current.button == 1) {
							// Mosue Right Drag
							if (!Event.current.alt) {
								// View Rotate
								Vector2 del = Event.current.delta * 0.2f;
								float angle = sceneView.camera.transform.rotation.eulerAngles.x + del.y;
								angle = angle > 89 && angle < 180 ? 89 : angle;
								angle = angle > 180 && angle < 271 ? 271 : angle;
								sceneView.LookAt(
									sceneView.pivot,
									Quaternion.Euler(
										angle,
										sceneView.camera.transform.rotation.eulerAngles.y + del.x,
										0f
									),
									sceneView.size,
									sceneView.orthographic,
									true
								);
								Event.current.Use();
							}
						}
						break;
				}
			}
		}


		private static string FixedRelativePath (string path) {
			path = FixPath(path);
			if (path.StartsWith("Assets")) {
				return path;
			}
			var fixedDataPath = FixPath(Application.dataPath);
			if (path.StartsWith(fixedDataPath)) {
				return "Assets" + path[fixedDataPath.Length..];
			} else {
				return "";
			}
		}


		static void TextToFile (string data, string path) {
			CreateFolder(GetParentPath(path));
			FileStream fs = new(path, FileMode.Create);
			StreamWriter sw = new(fs, Encoding.ASCII);
			sw.Write(data);
			sw.Close();
			fs.Close();
		}


		static bool PathIsFolder (string path) => File.GetAttributes(path).HasFlag(FileAttributes.Directory);


		static void CreateFolder (string path) {
			if (!string.IsNullOrEmpty(path) && !FolderExists(path)) {
				string pPath = GetParentPath(path);
				if (!FolderExists(pPath)) {
					CreateFolder(pPath);
				}
				Directory.CreateDirectory(path);
			}
		}


		static string FixPath (string path, bool forUnity = true) {
			char dsChar = forUnity ? '/' : Path.DirectorySeparatorChar;
			char adsChar = forUnity ? '\\' : Path.AltDirectorySeparatorChar;
			path = path.Replace(adsChar, dsChar);
			path = path.Replace(new string(dsChar, 2), dsChar.ToString());
			while (path.Length > 0 && path[0] == dsChar) {
				path = path.Remove(0, 1);
			}
			while (path.Length > 0 && path[^1] == dsChar) {
				path = path.Remove(path.Length - 1, 1);
			}
			return path;
		}


		static string GetNameWithoutExtension (string path) => Path.GetFileNameWithoutExtension(path);


		static string CombinePaths (params string[] paths) {
			string path = "";
			for (int i = 0; i < paths.Length; i++) {
				path = Path.Combine(path, paths[i]);
			}
			return path;
		}


		static string GetParentPath (string path) => Directory.GetParent(path).FullName;


		static bool FolderExists (string path) => Directory.Exists(path);


		static IEnumerable<string> ForAllPackages () {
			var req = Client.List(true, false);
			while (!req.IsCompleted) { }
			if (req.Status == StatusCode.Success) {
				foreach (var package in req.Result) {
					yield return "Packages/" + package.name;
				}
			}
		}


		static IEnumerable<string> EnumerateFiles (string path, bool topOnly, string searchPattern) {
			if (!FolderExists(path)) return null;
			var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
			return Directory.EnumerateFiles(path, searchPattern, option);
		}


		static IEnumerable<string> ForAllLines (string path) {
			using StreamReader sr = new(path, Encoding.ASCII);
			while (sr.Peek() >= 0) yield return sr.ReadLine();
		}


		#endregion



	}



}
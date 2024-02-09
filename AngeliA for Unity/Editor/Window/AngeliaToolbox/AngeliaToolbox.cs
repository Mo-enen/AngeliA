using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;
using AngeliaFramework;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;
using System.IO;
using System.Text;
using UnityEditor.PackageManager;
using System.Reflection;


[assembly: AngeliA]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaForUnity.Editor {
	public class AngeliaToolbox : IPreprocessBuildWithReport {




		#region --- VAR ---


		int IOrderedCallback.callbackOrder => 0;

		// Deselect Script File When Lost Focus
		private static bool PrevUnityFocused = true;

		// Project Highlight
		private static double LastCacheUpdateTime = -3d;
		private static EditorWindow ProjectWindow = null;

		// Data
		private static readonly List<VisualElement> EdittimeOnlyElements = new();
		private static readonly List<VisualElement> RuntimeOnlyElements = new();
		private static readonly List<VisualElement> ProfilerProgressBarTints = new();
		private static readonly VisualElement[] ProfilerEntityBars = new VisualElement[EntityLayer.COUNT];
		private static readonly List<PhysicsCell[,,]> CellPhysicsCells = new();
		private static EditorWindow Inspector = null;
		private static VisualElement Toolbox = null;
		private static Image SheetThumbnail = null;
		private static Label SheetThumbnailLabel = null;
		private static double AutoRefreshTime = 0f;
		private static bool RequireRefresh = true;


		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		public static void Init () {

			EditorApplication.update -= Update;
			EditorApplication.update += Update;

			EditorApplication.playModeStateChanged -= PlayModeStateChanged;
			EditorApplication.playModeStateChanged += PlayModeStateChanged;

			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;

			EditorSceneManager.sceneOpened -= OnSceneOpened;
			EditorSceneManager.sceneOpened += OnSceneOpened;

		}


		private static void Update () {

			double time = EditorApplication.timeSinceStartup;
			if (time > AutoRefreshTime + 1f) {
				AutoRefreshTime = time;
				RequireRefresh = true;
			}

			// Try Get Inspector
			if (Inspector == null && RequireRefresh) {

				ProfilerProgressBarTints.Clear();
				for (int i = 0; i < EntityLayer.COUNT; i++) {
					ProfilerEntityBars[i] = null;
				}

				var inspector = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
				if (inspector != null) {
					var objs = Resources.FindObjectsOfTypeAll(inspector);
					if (objs.Length > 0) {
						Inspector = objs[0] as EditorWindow;
						InjectAll(Inspector);

					}
				}
			}

			if (Inspector != null) {

				// Refresh Enable and Visible
				if (RequireRefresh) {
					RefreshVisualElement();
					RequireRefresh = false;
				}

			}

		}


		private static void PlayModeStateChanged (PlayModeStateChange mode) {
			RequireRefresh = true;
			CellPhysicsCells.Clear();
			RefreshSheetThumbnail();
		}


		private static void OnSelectionChanged () {
			RefreshVisualElement();
			RequireRefresh = false;
		}


		private static void OnSceneOpened (Scene scene, OpenSceneMode mode) {
			RefreshVisibility();
			RefreshSheetThumbnail();
		}


		public void OnPreprocessBuild (BuildReport report) => Refresh();


		[MenuItem("AngeliA/Refresh _r", false, 0)]
		private static void Refresh () {
			try {
				EditorUtility.ClearProgressBar();

				EditorUtility.DisplayProgressBar("Refreshing", "Refreshing...", 0.5f);

				// Aseprite >> Sheet
				string universeRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot);
				SheetUtil.CreateSheetFromAsepriteFiles(
					AngePath.GetArtworkRoot(universeRoot)
				)?.SaveToDisk(
					AngePath.GetSheetPath(universeRoot)
				);

				// For Unity
				AddAlwaysIncludeShadersForUnity();
				AngeliaProjectInfo_to_Unity();
				RefreshSheetThumbnail(true);
				if (EditorWindow.HasOpenInstances<ScriptHubWindow>()) {
					EditorWindow.GetWindow<ScriptHubWindow>().ReloadAllScripts(repaint: true);
				}
				PlayerSettings.colorSpace = ColorSpace.Gamma;
				AssetDatabase.Refresh();
				EditorSceneManager.SaveOpenScenes();
				EditorUtility.DisplayProgressBar("Refreshing", "Finished", 1f);
			} catch (System.Exception ex) { Debug.LogException(ex); }
			EditorUtility.ClearProgressBar();
			// Func
			static void AddAlwaysIncludeShadersForUnity () {
				// Angelia Shaders
				foreach (var guid in AssetDatabase.FindAssets("t:shader")) {
					try {
						string path = AssetDatabase.GUIDToAssetPath(guid);
						var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
						if (shader == null) continue;
						if (shader.name.StartsWith("Angelia/", System.StringComparison.OrdinalIgnoreCase)) {
							AddAlwaysIncludedShader(shader.name);
						}
					} catch (System.Exception ex) { Debug.LogException(ex); }
				}
				// Final
				AssetDatabase.SaveAssets();
				// Func
				static void AddAlwaysIncludedShader (string shaderName) {
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
			}
			static void AngeliaProjectInfo_to_Unity () {
				if (AngeliaVersionAttribute.GetVersion(out int major, out int minor, out int patch, out _)) {
					PlayerSettings.bundleVersion = $"{major}.{minor}.{patch}";
				}
				PlayerSettings.companyName = AngeliaGameDeveloperAttribute.GetDeveloper();
				PlayerSettings.productName = AngeliaGameTitleAttribute.GetTitle();
				PlayerSettings.SetApplicationIdentifier(
					NamedBuildTarget.Standalone,
					$"com.{PlayerSettings.companyName.Replace(" ", "").ToLower()}.{PlayerSettings.productName.Replace(" ", "").ToLower()}"
				);
			}
		}


		[InitializeOnLoadMethod]
		public static void DeselectScriptFileWhenLostFocus () {
			EditorApplication.update += () => {
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
			static void ProjectItemGUI (string guid, UnityEngine.Rect selectionRect) {
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

			// Deselect
			Selection.activeObject = null;

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
				if (!Directory.Exists(path)) return null;
				var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
				return Directory.EnumerateFiles(path, searchPattern, option);
			}


			static IEnumerable<string> ForAllLines (string path) {
				using StreamReader sr = new(path, Encoding.ASCII);
				while (sr.Peek() >= 0) yield return sr.ReadLine();
			}

		}


		#endregion




		#region --- LGC ---


		// Refresh
		public static void RefreshVisualElement () {

			// Root
			bool isPlaying = EditorApplication.isPlaying;
			bool showRoot = Selection.activeObject == null;
			bool compiling = EditorApplication.isCompiling;

			if (Toolbox != null) {
				Toolbox.style.display = showRoot ? DisplayStyle.Flex : DisplayStyle.None;
				if (Toolbox.enabledSelf == compiling) {
					Toolbox.SetEnabled(!compiling);
				}
			}

			// Sheet Thumbnail
			if (SheetThumbnail != null) SheetThumbnail.style.display = showRoot ? DisplayStyle.Flex : DisplayStyle.None;
			if (SheetThumbnailLabel != null) SheetThumbnailLabel.style.display = showRoot ? DisplayStyle.Flex : DisplayStyle.None;

			// Other
			if (showRoot) {
				// Edittime Only
				foreach (var ve in EdittimeOnlyElements) {
					ve.SetEnabled(!isPlaying);
				}
				// Runtime Only
				foreach (var ve in RuntimeOnlyElements) {
					ve.SetEnabled(isPlaying);
				}
			}

		}


		public static void RefreshVisibility () {

			if (Inspector == null) return;

			// Scrollbar
			if (Toolbox != null) {
				var scroll = Toolbox.Query<ScrollView>().First();
				if (scroll != null && scroll.verticalScroller != null) {
					scroll.verticalScroller.visible = false;
				}
			}

		}


		public static void RefreshSheetThumbnail (bool forceRefresh = false) {

			if (SheetThumbnail == null) return;

			// Texture
			var texture = SheetThumbnail.image as Texture2D;
			string sheetPath = AngePath.GetSheetPath(Util.CombinePaths(AngePath.ApplicationDataPath, "Universe"));
			int spriteCount = 0;
			if (texture == null || forceRefresh) {
				SheetThumbnail.image = texture = SheetUtil.LoadSheetTextureFromDisk(
					sheetPath, out spriteCount
				) as Texture2D;
			}

			// Label
			if (texture != null) {
				var rawBytes = texture.GetRawTextureData();
				float rawSize = rawBytes.Length / 1024f / 1024f;
				float fileSize = Util.GetFileSizeInMB(sheetPath);
				SheetThumbnailLabel.text = $"{texture.width}×{texture.height}  |  {spriteCount} Sprites  |  {rawSize:0.00} MB  |  {fileSize:0.00} MB";
				SheetThumbnailLabel.tooltip = "texture size | sprite count | texture raw size | sheet file size";
			} else {
				SheetThumbnailLabel.text = "";
			}

		}


		// Inject
		private static void InjectAll (EditorWindow inspector) => InjectVisualTreeToEditorWindow(inspector, "AngeliaToolbox", (root) => {

			inspector.minSize = new Vector2(345f, inspector.minSize.y);
			EdittimeOnlyElements.Clear();
			RuntimeOnlyElements.Clear();

			Toolbox = root;

			// Add Callbacks for Small Buttons
			root.Query<Button>(className: "SmallButton").ForEach((btn) => {
				switch (btn.name) {
					case "Setting":
						btn.clicked += () => SettingsService.OpenProjectSettings("Project/Player");
						break;
					case "BuildSetting":
						btn.clicked += () => EditorApplication.ExecuteMenuItem("File/Build Settings...");
						break;
					case "Preferences":
						btn.clicked += () => SettingsService.OpenUserPreferences();
						break;
					case "Profiler":
						btn.clicked += () => EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
						break;
					case "Package":
						btn.clicked += () => EditorApplication.ExecuteMenuItem("Window/Package Manager");
						break;
					case "ScriptHub":
						btn.clicked += () => EditorApplication.ExecuteMenuItem("AngeliA/Script Hub");
						break;
					case "SliceEditor":
						btn.clicked += () => EditorApplication.ExecuteMenuItem("AngeliA/Slice Editor");
						break;
					case "Refresh":
						btn.clicked += () => Refresh();
						break;
					case "Language":
						btn.clicked += () => EditorApplication.ExecuteMenuItem("AngeliA/Language Editor");
						break;
					case "Icon":
						btn.clicked += () => EditorApplication.ExecuteMenuItem("AngeliA/Other/Editor Icons");
						break;
				}
			});

			// Add Callbacks for Links
			root.Query<Button>(className: "Link").ForEach((btn) => {
				switch (btn.name) {
					case "Universe":
						btn.clicked += () => EditorUtility.OpenWithDefaultApp(Util.CombinePaths(AngePath.ApplicationDataPath, "Universe"));
						break;
					case "Persis":
						btn.clicked += () => EditorUtility.OpenWithDefaultApp(AngePath.PersistentDataPath);
						break;
					case "Project":
						btn.clicked += () => EditorUtility.OpenWithDefaultApp(Util.GetParentPath(Application.dataPath));
						break;
				}
			});

			// Sheet Thumbnail
			SheetThumbnail = root.Query<Image>(name: "SheetThumbnail").First();
			SheetThumbnailLabel = root.Query<Label>(name: "SheetThumbnailLabel").First();
			RefreshSheetThumbnail();

		});


		private static void InjectVisualTreeToEditorWindow (EditorWindow window, string assetName, System.Action<VisualElement> callback) {
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


		private static void InjectVisualTreeToEditorWindow (EditorWindow window, VisualTreeAsset asset, System.Action<VisualElement> callback) {
			if (asset == null || window.rootVisualElement.Children().Any(v => v.name == asset.name)) return;
			var root = new VisualElement() { name = asset.name, };
			asset.CloneTree(root);
			callback?.Invoke(root);
			window.rootVisualElement.Insert(0, root);
		}


		#endregion



	}
}
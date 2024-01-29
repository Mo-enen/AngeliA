using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;
using AngeliaFramework;
using System.Linq;



namespace AngeliaForUnity.Editor {
	public static class AngeliaToolbox {




		#region --- VAR ---


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
		private static int SpriteCount = 0;


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
			if (texture == null || forceRefresh) {
				SpriteCount = 0;
				SheetThumbnail.image = texture = Sheet.LoadSheetTextureFromDisk(
					sheetPath, out SpriteCount
				) as Texture2D;
			}

			// Label
			if (texture != null) {
				var rawBytes = texture.GetRawTextureData();
				float rawSize = rawBytes.Length / 1024f / 1024f;
				float fileSize = Util.GetFileSizeInMB(sheetPath);
				SheetThumbnailLabel.text = $"{texture.width}×{texture.height}  |  {SpriteCount} Sprites  |  {rawSize:0.00} MB  |  {fileSize:0.00} MB";
				SheetThumbnailLabel.tooltip = "texture size | sprite count | texture raw size | sheet file size";
			} else {
				SheetThumbnailLabel.text = "";
			}

		}


		// Inject
		private static void InjectAll (EditorWindow inspector) {
			inspector.minSize = new Vector2(345f, inspector.minSize.y);
			EdittimeOnlyElements.Clear();
			RuntimeOnlyElements.Clear();
			InjectToolbox(inspector, "AngeliaToolbox");
			RefreshVisibility();
		}


		private static void InjectToolbox (EditorWindow inspector, string assetName) => InjectVisualTreeToEditorWindow(inspector, assetName, (root) => {

			Toolbox = root;

			// Add Callbacks for Big Buttons
			root.Query<Button>(className: "ToolboxButton").ForEach((btn) => {
				switch (btn.name) {
					case "Refresh":
						btn.clicked += AngeliaRefresh.RefreshFromMenu;
						EdittimeOnlyElements.Add(btn);
						break;
					case "ForceRefresh":
						btn.clicked += AngeliaRefresh.ForceRefresh;
						EdittimeOnlyElements.Add(btn);
						break;
					case "Language":
						btn.clicked += LanguageEditor.OpenWindow;
						EdittimeOnlyElements.Add(btn);
						break;
				}
			});

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
					case "AngeHash":
						btn.clicked += () => EditorApplication.ExecuteMenuItem("AngeliA/Other/Ange Hash");
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
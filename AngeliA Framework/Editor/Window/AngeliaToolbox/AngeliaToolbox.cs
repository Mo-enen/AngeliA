using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;



namespace AngeliaFramework.Editor {
	using Debug = UnityEngine.Debug;
	public static class AngeliaToolbox {




		#region --- SUB ---


		private class Refresh : IRefreshEvent {
			void IRefreshEvent.Refresh (bool forceRefresh) => RefreshSheetThumbnail();
		}


		#endregion




		#region --- VAR ---


		// Const
		private const string UNITY_PROGRESS = "unity-progress-bar__progress";
		private static readonly Color PROFILER_GREEN = new(0.5f, 1f, 0.4f, 0.5f);
		private static readonly Color PROFILER_RED = new(1f, 0f, 0f, 0.5f);

		// Data
		private static readonly Color[] COLLIDER_TINTS = { Const.RED_BETTER, Const.ORANGE_BETTER, Color.yellow, Const.GREEN, Const.CYAN, Const.BLUE, Const.GREY_128, };
		private static readonly List<VisualElement> EdittimeOnlyElements = new();
		private static readonly List<VisualElement> RuntimeOnlyElements = new();
		private static readonly List<VisualElement> ProfilerProgressBarTints = new();
		private static readonly VisualElement[] ProfilerEntityBars = new VisualElement[EntityLayer.COUNT];
		private static readonly List<PhysicsCell[,,]> CellPhysicsCells = new();
		private static readonly List<GameObject> CacheRootObjects = new();
		private static EditorWindow Inspector = null;
		private static VisualElement GameStarter = null;
		private static VisualElement Toolbox = null;
		private static VisualElement Profiler = null;
		private static VisualElement ProfilerVE_CellContainer = null;
		private static VisualElement ProfilerVE_EntityContainer = null;
		private static VisualElement ProfilerVE_TaskContainer = null;
		private static Image SheetThumbnail = null;
		private static Label SheetThumbnailLabel = null;
		private static Material GizmosMaterial = null;
		private static double RefreshProfilerTime = 0f;
		private static double AutoRefreshTime = 0f;
		private static bool RequireRefresh = true;
		private static int GizmosIndex = -1;


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

			Application.onBeforeRender -= OnBeforeRender;
			Application.onBeforeRender += OnBeforeRender;

			AngeEditorUtil.HideMetaFiles(AngePath.UniverseRoot);

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
				GameStarter = null;
				ProfilerVE_CellContainer = null;
				ProfilerVE_EntityContainer = null;
				ProfilerVE_TaskContainer = null;
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

				// Refresh Profiler
				if (EditorApplication.isPlaying && time > RefreshProfilerTime + 0.5f) {
					RefreshProfiler();
					RefreshProfilerTime = time;
				}

			}

		}


		private static void PlayModeStateChanged (PlayModeStateChange mode) {
			RequireRefresh = true;
			RefreshProfilerTime = double.MinValue;
			GizmosIndex = -1;
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


		private static void OnBeforeRender () {

			if (!EditorApplication.isPlaying || Game.GameCamera == null) return;

			if (GizmosIndex < 0) {
				ScreenEffect.SetEffectEnable(TintEffect.TYPE_ID, false);
				return;
			}

			if (!ScreenEffect.GetEffectEnable(TintEffect.TYPE_ID)) {
				ScreenEffect.SetEffectEnable(TintEffect.TYPE_ID, true);
				TintEffect.SetTint(Const.WHITE);
			}

			var cameraRect01 = Game.GameCamera.rect;
			var angeCameraRect = CellRenderer.CameraRect;
			var rect = new Rect();
			float thickX = 0.0005f;
			float thickY = 0.0005f * Game.GameCamera.aspect;
			if (GizmosMaterial == null) {
				GizmosMaterial = new Material(Shader.Find("Angelia/Vertex"));
			}

			// Colliders
			if (GizmosIndex == 0) {
				if (CellPhysicsCells.Count == 0) {
					try {
						var layers = Util.GetStaticFieldValue(typeof(CellPhysics), "Layers") as System.Array;
						for (int layerIndex = 0; layerIndex < PhysicsLayer.COUNT; layerIndex++) {
							var layerObj = layers.GetValue(layerIndex);
							CellPhysicsCells.Add(Util.GetFieldValue(layerObj, "Cells") as PhysicsCell[,,]);
						}
					} catch (System.Exception ex) { Debug.LogException(ex); }
					if (CellPhysicsCells.Count == 0) CellPhysicsCells.Add(null);
				}
				if (CellPhysicsCells.Count == CellPhysicsCells.Count) {

					GL.ClearWithSkybox(true, Game.GameCamera);
					GizmosMaterial.SetPass(0);
					GL.LoadOrtho();
					GL.Begin(GL.QUADS);

					for (int layer = 0; layer < CellPhysicsCells.Count; layer++) {
						try {
							var cells = CellPhysicsCells[layer];
							int cellWidth = cells.GetLength(0);
							int cellHeight = cells.GetLength(1);
							int celDepth = cells.GetLength(2);
							GL.Color(COLLIDER_TINTS[layer.Clamp(0, COLLIDER_TINTS.Length - 1)]);
							for (int y = 0; y < cellHeight; y++) {
								for (int x = 0; x < cellWidth; x++) {
									for (int d = 0; d < celDepth; d++) {
										var cell = cells[x, y, d];
										if (cell.Frame != Game.GlobalFrame) { break; }

										rect.x = Util.RemapUnclamped(
											angeCameraRect.x, angeCameraRect.xMax, cameraRect01.x, cameraRect01.xMax, cell.Rect.x
										);
										rect.y = Util.RemapUnclamped(
											angeCameraRect.y, angeCameraRect.yMax, cameraRect01.y, cameraRect01.yMax, cell.Rect.y
										);
										rect.width = Util.RemapUnclamped(
											0, angeCameraRect.width, 0, cameraRect01.width, cell.Rect.width
										);
										rect.height = Util.RemapUnclamped(
											0, angeCameraRect.height, 0, cameraRect01.height, cell.Rect.height
										);

										if (rect.Overlaps(cameraRect01)) {
											DrawFrame(rect, thickX, thickY, !cell.IsTrigger);
										}
									}
								}
							}
						} catch (System.Exception ex) { Debug.LogException(ex); }
					}

					GL.End();
				}
			}

			// Bounds
			if (GizmosIndex == 1) {

				GL.ClearWithSkybox(true, Game.GameCamera);
				GizmosMaterial.SetPass(0);
				GL.LoadOrtho();
				GL.Begin(GL.QUADS);
				GL.Color(Const.BLUE);

				try {
					for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
						var entities = Stage.Entities[layer];
						int count = Stage.EntityCounts[layer];
						for (int i = 0; i < count; i++) {
							var e = entities[i];
							if (!e.Active) continue;

							var bounds = e.GlobalBounds;

							rect.x = Util.RemapUnclamped(
								angeCameraRect.x, angeCameraRect.xMax, cameraRect01.x, cameraRect01.xMax, bounds.x
							);
							rect.y = Util.RemapUnclamped(
								angeCameraRect.y, angeCameraRect.yMax, cameraRect01.y, cameraRect01.yMax, bounds.y
							);
							rect.width = Util.RemapUnclamped(
								0, angeCameraRect.width, 0, cameraRect01.width, bounds.width
							);
							rect.height = Util.RemapUnclamped(
								0, angeCameraRect.height, 0, cameraRect01.height, bounds.height
							);

							DrawFrame(rect, thickX, thickY, false);
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }

				GL.End();

			}


			static void DrawFrame (Rect rect, float thickX, float thickY, bool cross) {

				DrawRect(new Rect(rect.x - thickX, rect.y - thickY, thickX * 2f, rect.height + thickY * 2f));
				DrawRect(new Rect(rect.xMax - thickX, rect.y - thickY, thickX * 2f, rect.height + thickY * 2f));
				DrawRect(new Rect(rect.x, rect.y - thickY, rect.width, thickY * 2f));
				DrawRect(new Rect(rect.x, rect.yMax - thickY, rect.width, thickY * 2f));
				if (cross) {
					GL.Vertex3(rect.x - thickX, rect.y - thickY, 0.5f);
					GL.Vertex3(rect.x - thickX, rect.y + thickY, 0.5f);
					GL.Vertex3(rect.xMax + thickX, rect.yMax + thickY, 0.5f);
					GL.Vertex3(rect.xMax + thickX, rect.yMax - thickY, 0.5f);

					GL.Vertex3(rect.x - thickX, rect.yMax - thickY, 0.5f);
					GL.Vertex3(rect.x - thickX, rect.yMax + thickY, 0.5f);
					GL.Vertex3(rect.xMax + thickX, rect.y + thickY, 0.5f);
					GL.Vertex3(rect.xMax + thickX, rect.y - thickY, 0.5f);
				}
				static void DrawRect (Rect rect) {
					GL.Vertex3(rect.x, rect.y, 0.5f);
					GL.Vertex3(rect.x, rect.yMax, 0.5f);
					GL.Vertex3(rect.xMax, rect.yMax, 0.5f);
					GL.Vertex3(rect.xMax, rect.y, 0.5f);
				}
			}
		}


		#endregion




		#region --- LGC ---


		// Refresh
		public static void RefreshVisualElement () {

			// Root
			var currentGame = GetCurrentGameFromSceneRoot();
			bool isPlaying = EditorApplication.isPlaying;
			bool showRoot = Selection.activeObject == null && (isPlaying || currentGame != null);
			bool showGameStarter = Selection.activeObject == null && !isPlaying && currentGame == null;
			bool compiling = EditorApplication.isCompiling;

			if (Toolbox != null) {
				Toolbox.style.display = showRoot ? DisplayStyle.Flex : DisplayStyle.None;
				if (Toolbox.enabledSelf == compiling) {
					Toolbox.SetEnabled(!compiling);
				}
			}
			if (Profiler != null) {
				Profiler.style.display = showRoot && isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
				if (Profiler.enabledSelf == compiling) {
					Profiler.SetEnabled(!compiling);
				}
			}

			// Game Starter
			if (GameStarter != null) {
				GameStarter.style.display = showGameStarter ? DisplayStyle.Flex : DisplayStyle.None;
			}

			// Sheet Thumbnail
			if (SheetThumbnail != null) SheetThumbnail.style.display = showRoot ? DisplayStyle.Flex : DisplayStyle.None;
			if (SheetThumbnailLabel != null) SheetThumbnailLabel.style.display = showRoot ? DisplayStyle.Flex : DisplayStyle.None;

			// Other
			if (showRoot) {
				// Edittime Only
				foreach (var ve in EdittimeOnlyElements) {
					ve.SetEnabled(!isPlaying);
					//ve.style.display = !isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
				}
				// Runtime Only
				foreach (var ve in RuntimeOnlyElements) {
					ve.SetEnabled(isPlaying);
					//ve.style.display = isPlaying ? DisplayStyle.Flex : DisplayStyle.None;
				}
			}

		}


		public static void RefreshProfiler () {

			// Cell
			if (ProfilerVE_CellContainer != null) {
				int childCount = ProfilerVE_CellContainer.childCount;
				int layerCount = CellRenderer.LayerCount;
				if (childCount != layerCount + 1) {
					// Respawn Child
					ProfilerVE_CellContainer.Clear();
					ProfilerProgressBarTints.Clear();
					for (int i = 0; i < layerCount; i++) {
						ProfilerVE_CellContainer.Add(CreateProfilerProgressBar(out var progress));
						ProfilerProgressBarTints.Add(progress);
					}
					ProfilerVE_CellContainer.Add(CreateProfilerProgressBar(out var tProgress));
					ProfilerProgressBarTints.Add(tProgress);
				}
				// Refresh Info
				childCount = ProfilerVE_CellContainer.childCount;
				for (int i = 0; i < childCount; i++) {
					if (ProfilerVE_CellContainer.ElementAt(i) is ProgressBar bar) {
						int use = CellRenderer.GetUsedCellCount(i);
						int all = CellRenderer.GetLayerCapacity(i);
						string name = i < CellRenderer.LayerCount ?
							CellRenderer.GetLayerName(i) :
							CellRenderer.GetTextLayerName(i - CellRenderer.LayerCount);
						bar.lowValue = 0;
						bar.highValue = all;
						bar.value = use;
						bar.title = $"<size=10><color=#BBBBBB>{name}</color></size>  {use} / {all}";
						if (i < ProfilerProgressBarTints.Count) {
							ProfilerProgressBarTints[i].style.backgroundColor =
								Color.Lerp(PROFILER_GREEN, PROFILER_RED, (float)use / all);
						}
					}
				}
			}

			// Entity
			if (ProfilerVE_EntityContainer != null) {
				if (ProfilerVE_EntityContainer.childCount == 0) {
					for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
						ProfilerVE_EntityContainer.Add(CreateProfilerProgressBar(out ProfilerEntityBars[layer]));
					}
				}
				if (ProfilerVE_EntityContainer.childCount > 0) {
					for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
						if (ProfilerVE_EntityContainer.ElementAt(layer) is not ProgressBar bar) continue;
						int use = Stage.EntityCounts[layer];
						int all = Stage.Entities[layer].Length;
						bar.lowValue = 0;
						bar.highValue = all;
						bar.value = use;
						bar.title = $"<size=10><color=#BBBBBB>{EntityLayer.LAYER_NAMES[layer.Clamp(0, EntityLayer.LAYER_NAMES.Length - 1)]}</color></size>  {use} / {all}";
						if (ProfilerEntityBars[layer] != null) {
							ProfilerEntityBars[layer].style.backgroundColor =
								Color.Lerp(PROFILER_GREEN, PROFILER_RED, (float)use / all);
						}
					}
				}
			}

			// Task
			if (ProfilerVE_TaskContainer != null) {
				if (ProfilerVE_TaskContainer.childCount != 1) {
					ProfilerVE_TaskContainer.Clear();
					for (int i = 0; i < 1; i++) {
						var ve = new VisualElement();
						ve.style.flexDirection = FlexDirection.Column;
						ve.style.minHeight = 12;
						ve.style.backgroundColor = new Color(0, 0, 0, 0.06f);
						ve.style.marginLeft = ve.style.marginRight = 3f;
						ve.style.paddingLeft = ve.style.paddingRight = 3f;
						ve.style.paddingBottom = ve.style.paddingTop = 3f;
						ProfilerVE_TaskContainer.Add(ve);
					}
				}
				var container = ProfilerVE_TaskContainer.ElementAt(0);
				container.style.width = ProfilerVE_TaskContainer.contentRect.width;
				int count = FrameTask.GetWaitingTaskCount();
				var currentTask = FrameTask.GetCurrentTask();
				if (currentTask != null) count++;
				if (container.childCount != count) {
					container.Clear();
					for (int j = 0; j < count; j++) {
						container.Add(new Label());
					}
				}
				for (int j = 0; j < count; j++) {
					var label = container.ElementAt(j) as Label;
					if (j == 0 && currentTask != null) {
						label.text = currentTask.GetType().Name;
					} else {
						label.text = FrameTask.GetTaskAt(j - 1).GetType().Name;
					}
				}
			}

			// Func
			static ProgressBar CreateProfilerProgressBar (out VisualElement progressVE) {
				var bar = new ProgressBar();
				VisualElement pVE = null;
				bar.Query<VisualElement>(className: UNITY_PROGRESS).ForEach((p) => pVE = p);
				bar.style.marginBottom = 3;
				bar.style.height = 16;
				progressVE = pVE;
				return bar;
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
			if (texture == null || forceRefresh) {
				SheetThumbnail.image = texture = AngeUtil.LoadTexture(AngePath.SheetTexturePath);
			}

			// Label
			if (texture != null) {
				var rawBytes = texture.GetRawTextureData();
				float rawSize = rawBytes.Length / 1024f / 1024f;
				SheetThumbnailLabel.text = $"{texture.width}×{texture.height}  |  {AngeliaRefreshEvent.LastSpriteCount.Value} Sprites  |  {rawSize:0.00} MB";
			} else {
				SheetThumbnailLabel.text = "";
			}

		}


		// Inject
		private static void InjectAll (EditorWindow inspector) {
			inspector.minSize = new Float2(345f, inspector.minSize.y);
			EdittimeOnlyElements.Clear();
			RuntimeOnlyElements.Clear();
			//InjectProfiler(inspector, "AngeliaProfiler");
			InjectToolbox(inspector, "AngeliaToolbox");
			InjectGameStarter(inspector);
			RefreshVisibility();
		}


		private static void InjectToolbox (EditorWindow inspector, string assetName) => EditorUtil.InjectVisualTreeToEditorWindow(inspector, assetName, (root) => {

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

					case "Collider":
						btn.clicked += () => GizmosIndex = GizmosIndex == 0 ? -1 : 0;
						RuntimeOnlyElements.Add(btn);
						break;
					case "Bound":
						btn.clicked += () => GizmosIndex = GizmosIndex == 1 ? -1 : 1;
						RuntimeOnlyElements.Add(btn);
						break;
				}
			});

			// Add Callbacks for Links
			root.Query<Button>(className: "Link").ForEach((btn) => {
				switch (btn.name) {
					case "Universe":
						btn.clicked += () => EditorUtility.OpenWithDefaultApp(AngePath.UniverseRoot);
						break;
					case "Persis":
						btn.clicked += () => EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
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


		private static void InjectProfiler (EditorWindow inspector, string assetName) => EditorUtil.InjectVisualTreeToEditorWindow(inspector, assetName, (root) => {

			ProfilerVE_CellContainer = null;
			ProfilerVE_EntityContainer = null;
			ProfilerVE_TaskContainer = null;
			Profiler = root;
			root.Query<VisualElement>(className: "Container").ForEach((ve) => {
				switch (ve.name) {
					case "Cell":
						ProfilerVE_CellContainer = ve;
						break;
					case "Entity":
						ProfilerVE_EntityContainer = ve;
						break;
					case "Task":
						ProfilerVE_TaskContainer = ve;
						break;
				}
			});
		});


		private static void InjectGameStarter (EditorWindow inspector) {

			inspector.rootVisualElement.Insert(0, GameStarter = new VisualElement());
			GameStarter.style.paddingLeft = 12;
			GameStarter.style.paddingRight = 12;
			GameStarter.style.paddingTop = 24;

			// All Buttons
			bool hasSubClass = false;
			foreach (var type in typeof(Game).AllChildClass()) {
				AddButton(type, "ffcc00");
				hasSubClass = true;
			}
			if (!hasSubClass) {
				AddButton(typeof(Game), "ffcc00");
			}

			// Func
			void AddButton (System.Type targetType, string color) {
				var button = new Button();
				GameStarter.Add(button);
				button.text = $"Add <b><size=16><color=#{color}>{targetType.Name}</color></size></b> to Current Scene";
				button.clicked += () => TryAddGameToCurrentScene(targetType);
				button.focusable = false;
				button.style.height = 36;
				button.style.fontSize = 12;
				button.style.marginBottom = 1;
				button.style.marginTop = 1;
				button.style.paddingLeft = 0;
				button.style.paddingRight = 0;
				button.style.paddingBottom = 0;
				button.style.paddingTop = 0;
				button.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 1f);
				button.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
				button.enableRichText = true;
			}
		}


		// Misc
		private static void TryAddGameToCurrentScene (System.Type targetType) {
			var currentGame = GetCurrentGameFromSceneRoot();
			if (targetType == null || currentGame != null) return;
			var game = new GameObject(targetType.Name, targetType);
			Selection.activeGameObject = game;
			var scene = SceneManager.GetActiveScene();
			if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);
			RefreshVisualElement();
		}


		private static Game GetCurrentGameFromSceneRoot () {
			var scene = SceneManager.GetActiveScene();
			if (!scene.IsValid() || scene.rootCount == 0) return null;
			scene.GetRootGameObjects(CacheRootObjects);
			Game result = null;
			foreach (var obj in CacheRootObjects) {
				if (obj.TryGetComponent<Game>(out var game)) {
					result = game;
					break;
				}
			}
			CacheRootObjects.Clear();
			return result;
		}


		#endregion



	}
}
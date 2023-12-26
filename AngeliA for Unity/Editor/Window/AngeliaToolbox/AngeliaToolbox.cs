using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;
using AngeliaFramework;
using Stage = AngeliaFramework.Stage;



namespace AngeliaForUnity.Editor {
	public static class AngeliaToolbox {




		#region --- SUB ---


		private class Refresh : IRefreshEvent {
			void IRefreshEvent.Refresh (bool forceRefresh) => RefreshSheetThumbnail();
		}


		#endregion




		#region --- VAR ---


		// Data
		private static readonly Color[] COLLIDER_TINTS = { Const.RED_BETTER.ToUnityColor(), Const.ORANGE_BETTER.ToUnityColor(), Color.yellow, Const.GREEN.ToUnityColor(), Const.CYAN.ToUnityColor(), Const.BLUE.ToUnityColor(), Const.GREY_128.ToUnityColor(), };
		private static readonly List<VisualElement> EdittimeOnlyElements = new();
		private static readonly List<VisualElement> RuntimeOnlyElements = new();
		private static readonly List<VisualElement> ProfilerProgressBarTints = new();
		private static readonly VisualElement[] ProfilerEntityBars = new VisualElement[EntityLayer.COUNT];
		private static readonly List<PhysicsCell[,,]> CellPhysicsCells = new();
		private static EditorWindow Inspector = null;
		private static VisualElement Toolbox = null;
		private static Image SheetThumbnail = null;
		private static Label SheetThumbnailLabel = null;
		private static Material GizmosMaterial = null;
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

			var camera = Camera.main;
			if (!EditorApplication.isPlaying || camera == null) return;

			var cameraRect01 = camera.rect;
			var angeCameraRect = CellRenderer.CameraRect;
			var rect = new FRect();
			float thickX = 0.0005f;
			float thickY = 0.0005f * camera.aspect;
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

					GL.ClearWithSkybox(true, camera);
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

										if (rect.Overlaps(cameraRect01.ToAngelia())) {
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

				GL.ClearWithSkybox(true, camera);
				GizmosMaterial.SetPass(0);
				GL.LoadOrtho();
				GL.Begin(GL.QUADS);
				GL.Color(Const.BLUE.ToUnityColor());

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


			static void DrawFrame (FRect rect, float thickX, float thickY, bool cross) {

				DrawRect(new FRect(rect.x - thickX, rect.y - thickY, thickX * 2f, rect.height + thickY * 2f));
				DrawRect(new FRect(rect.xMax - thickX, rect.y - thickY, thickX * 2f, rect.height + thickY * 2f));
				DrawRect(new FRect(rect.x, rect.y - thickY, rect.width, thickY * 2f));
				DrawRect(new FRect(rect.x, rect.yMax - thickY, rect.width, thickY * 2f));
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
				static void DrawRect (FRect rect) {
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
			if (texture == null || forceRefresh) {
				SheetThumbnail.image = texture = AngeUtilUnity.LoadTexture(AngePath.SheetTexturePath);
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
			inspector.minSize = new Vector2(345f, inspector.minSize.y);
			EdittimeOnlyElements.Clear();
			RuntimeOnlyElements.Clear();
			InjectToolbox(inspector, "AngeliaToolbox");
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


		#endregion



	}
}
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;



namespace PixelJelly.Editor {
	public partial class PixelJellyWindow : EditorWindow, IHasCustomMenu {



		// SUB
		public class BehaviourSerializeData {
			public Texture2D Icon => _Icon = _Icon != null ? _Icon : AssetPreview.GetMiniThumbnail(Behaviour);
			public Texture2D _Icon = null;
			public JellyBehaviour Behaviour = null;
			public SerializedObject SerializedObject = null;
			public SerializedProperty[] SerializedProperties = null;
			public IEnumerable<CustomAttributeData>[] Attributes = null;
			public string Name;
			public string Group;
			public System.Type Type;
			public SerializedProperty SeedProperty;
			public bool ShowRandomButton = true;
			public bool Favorite = false;
			public bool OneShotAnimation = false;
			public BehaviourSerializeData (JellyBehaviour pBehaviour) => Load(pBehaviour);
			public void Load (JellyBehaviour pBehaviour) {
				SerializedObject = new SerializedObject(pBehaviour);
				SerializedProperties = EditorUtil.GetInspectorProps(pBehaviour.GetType(), SerializedObject, out var attributes, true).ToArray();
				Attributes = attributes.ToArray();
				Behaviour = SerializedObject.targetObject as JellyBehaviour;
				Name = pBehaviour.FinalDisplayName;
				Type = pBehaviour.GetType();
				SeedProperty = SerializedObject.FindProperty("m_Seed");
			}
		}



		[System.Serializable]
		private class AssemblyJson {
			public string rootNamespace = "";
		}



		// Ser
		[SerializeField] string m_SelectingGroup = "";
		[SerializeField] int m_SelectingBehIndex = 0;

		// Data
		private static bool IsPlayingAnimation = false;
		private readonly SortedDictionary<string, List<BehaviourSerializeData>> BehaviourPool = new();
		private readonly Dictionary<(string assembly, string nameSpace, string type), (BehaviourSerializeData data, string group, int poolIndex)> BehaviourPathPool = new();
		private readonly Dictionary<System.Type, JellyInspector> InspectorPool = new();
		private readonly Dictionary<string, bool> BehaviourFoldMap = new();
		private (Color32[,][] colors, int width, int height) ColorBlock = (new Color32[0, 0][], 0, 0);
		private List<CommentData>[,] Comments = new List<CommentData>[0, 0];
		private Texture2D[,] CanvasTextures = new Texture2D[0, 0];
		private Texture2D CheckerFloorTexture = null;
		private Vector2 InspectorScrollPosition = Vector2.zero;
		private Vector2 ScriptScrollPosition = Vector2.zero;
		private bool CanvasDirty = true;
		private bool HasComment = false;
		private double PrevFrameTime = 0f;
		private int CurrentFrame = 0;
		private int UnusedAssetCount = 0;
		//private int SelectingSlotIndex = 0;
		private int SelectingSlotCount = 0;

		// Saving
		private readonly EditorSavingString OpeningConfig = new("PSW.OpeningConfig", "");
		private readonly EditorSavingString SelectingBehaviourName = new("PSW.SelectingCanvasName", "");
		private readonly EditorSavingBool DrawCheckerFloor = new("PSW.DrawCheckerFloor", false);
		private readonly EditorSavingBool ShowComment = new("PSW.ShowComment", true);
		private readonly EditorSavingBool ShowInspectorMsg = new("PSW.ShowInspectorMsg", true);
		private readonly EditorSavingBool ShowSeed = new("PSW.ShowSeed", false);
		private readonly EditorSavingBool ColorfulLogo = new("PSW.ColorfulLogo", false);



		// MSG
		[InitializeOnLoadMethod]
		public static void Init () {

			var window = GetWindowIfHasOpenInstances();
			if (window != null) {
				window.titleContent = new GUIContent(TITLE, window.ColorfulLogo.Value ? WindowIconColorful : WindowIcon);
				window.BehaviourPool.Clear();
			}

			// Update
			EditorApplication.update += () => {
				if (IsPlayingAnimation) {
					var window = GetWindowIfHasOpenInstances();
					if (window == null) { return; }
					var beh = window.GetSelectingBehaviour();
					if (beh != null && beh.Behaviour != null) {
						var behaviour = beh.Behaviour;
						double time = EditorApplication.timeSinceStartup;
						double duration = behaviour.GetFrameDuration(window.CurrentFrame) / 1000f;
						if (time > window.PrevFrameTime + duration) {
							window.PrevFrameTime += duration;
							if (time > window.PrevFrameTime + duration) {
								window.PrevFrameTime = time;
							}
							int nextFrame = Mathf.RoundToInt(Mathf.Repeat(window.CurrentFrame + 1, behaviour.FrameCount - 0.00001f));
							if (nextFrame == 0 && window.CurrentFrame != 0 && beh.OneShotAnimation) {
								window.PauseAnimation(beh);
							}
							window.SeekFrame(nextFrame, behaviour.FrameCount);
							window.Repaint();
						}
					}
				}
			};

			// Undo
			Undo.undoRedoPerformed += () => {
				var window = GetWindowIfHasOpenInstances();
				if (window == null) { return; }
				window.CanvasDirty = true;
				window.Repaint();
				var sBeh = window.GetSelectingBehaviour();
				if (sBeh != null) {
					JellyConfig.Main.ResetDataSlotCache(sBeh.Type);
				}
				sBeh.Load(window.GetRefreshedAsset(sBeh.Type));
				window.SetSelectingBehaviour(window.m_SelectingGroup, window.m_SelectingBehIndex);
			};

			// Scene Load
			EditorSceneManager.activeSceneChangedInEditMode += (sceneA, sceneB) => {
				var window = GetWindowIfHasOpenInstances();
				if (window == null) { return; }
				window.CanvasDirty = true;
			};


		}


		[MenuItem("Tools/Pixel Jelly")]
		public static void Menu_Open () {
			var window = GetWindow<PixelJellyWindow>(TITLE, true, typeof(SceneView));
			window.minSize = new Vector2(480, 480);
			window.titleContent = new GUIContent(TITLE, window.ColorfulLogo.Value ? WindowIconColorful : WindowIcon);
		}


		public void ShowButton (Rect position) {
			if (docked && GUI.Button(position, "", MaximizeButtonStyle)) {
				EditorApplication.delayCall += () => {
					maximized = !maximized;
				};
			}
		}


		void IHasCustomMenu.AddItemsToMenu (GenericMenu menu) {
			menu.AddItem(
				new GUIContent("Show \"Seed\" Property"),
				ShowSeed.Value,
				() => ShowSeed.Value = !ShowSeed.Value
			);
			menu.AddItem(
				new GUIContent("Colorful Logo"),
				ColorfulLogo.Value,
				() => {
					ColorfulLogo.Value = !ColorfulLogo.Value;
					UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(
						UnityEditor.Compilation.RequestScriptCompilationOptions.None
					);
				}
			);
			menu.AddItem(
				new GUIContent("Set Current Data as Default"),
				false,
				() => {
					var beh = GetSelectingBehaviour();
					if (beh != null) {
						SetBehaviourAsDefault(beh.Behaviour);
					}
				}
			);
		}


		private void OnEnable () {
			titleContent = new GUIContent(TITLE, ColorfulLogo.Value ? WindowIconColorful : WindowIcon);
		}


		private void OnGUI () {

			if (BehaviourPool.Count == 0) {
				InitBehaviourPool();
				Repaint();
			}

			ValidateBehaviourAssets();

			EditorGUI.DrawRect(Layout.Rect(0, 1), new Color(0, 0, 0, 0.5f));
			using (new GUILayout.HorizontalScope()) {
				// Behaviour
				using (new GUILayout.VerticalScope(PanelStyle, GUILayout.Width(BEHAVIOUR_PANEL_WIDTH))) {
					GUI_Behaviour();
				}
				ColorLineGUI(2, 0);
				// Canvas
				using (new GUILayout.VerticalScope()) {
					GUI_Canvas();
				}
				ColorLineGUI(2, 0);
				// Inspector
				using (new GUILayout.VerticalScope(PanelStyle, GUILayout.Width(INSPECTOR_PANEL_WIDTH))) {
					GUI_Inspector();
				}
			}

			GUI_Hotkey();

			// Final
			if (Event.current.type == EventType.MouseDown) {
				EditorGUI.FocusTextInControl(null);
				Repaint();
			}
		}


		private void OnFocus () => CanvasDirty = true;


		private void GUI_Behaviour () {
			// Title
			var oldB = GUI.backgroundColor;
			GUI.backgroundColor = TITLE_TINT;
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
				Layout.Space(3);
				GUI.Label(Layout.Rect(0, TITLE_HEIGHT), "Behaviours", EditorStyles.label);
				if (UnusedAssetCount > 0) {
					if (GUI.Button(Layout.Rect(TITLE_HEIGHT + 6, TITLE_HEIGHT), ClearCacheContent, EditorStyles.toolbarButton)) {
						if (EditorUtil.Dialog("Warning", $"Clear unused cache data for behaviours?\n{BehaviourAssetRoot}", "Clear", "Cancel")) {
							RemoveUnusedAssetCache();
							EditorUtil.Dialog("", "Unused cache data removed.", "OK");
						}
					}
				}
				if (GUI.Button(Layout.Rect(TITLE_HEIGHT + 12, TITLE_HEIGHT), NewJellyBehContent, EditorStyles.toolbarPopup)) {
					var menu = new GenericMenu();
					menu.AddItem(
						new GUIContent("New Jelly Behaviour"),
						false,
						() => {
							string _path = EditorUtility.SaveFilePanelInProject("Create New Behaviour", "NewJellyBehaviour", "cs", "Create a New Jelly Behaviour");
							CreateNewBehaviour(_path);
						}
					);
					var beh = GetSelectingBehaviour();
					if (beh == null || !InspectorPool.ContainsKey(beh.Type)) {
						menu.AddItem(
							new GUIContent("New Jelly Inspector"),
							false,
							() => {
								string _dName = "";
								if (beh != null) {
									_dName = beh.Type.Name;
								}
								string _path = EditorUtility.SaveFilePanelInProject("Create New Inspector", $"{_dName}_Inspector", "cs", "Create a New Jelly Inspector");
								CreateNewInspector(_path);
							}
						);
					} else {
						menu.AddDisabledItem(
							new GUIContent("New Jelly Inspector"),
							false
						);
					}
					menu.ShowAsContext();
				}
			}
			GUI.backgroundColor = oldB;
			// Content
			using var scope = new GUILayout.ScrollViewScope(ScriptScrollPosition);
			ScriptScrollPosition = scope.scrollPosition;
			// Fav
			var config = JellyConfig.Main;
			string clickingGroup = "";
			int clickingBehIndex = -1;
			Layout.Space(2);
			bool mouseDown = Event.current.type == EventType.MouseDown;
			var mousePos = Event.current.mousePosition;
			if (config.FavoriteBehCount > 0) {
				bool oldFavOpening = GetBehaviourFolding("Favorite");
				//var oldC = GUI.contentColor;
				//GUI.contentColor = Color.yellow;
				bool favOpening = Layout.Fold("★ Favorite", oldFavOpening);
				//GUI.contentColor = oldC;
				if (favOpening) {
					var enu = config.GetFavoriteEnumerator();
					while (enu.MoveNext()) {
						var fData = enu.Current;
						var bData = GetBehaviourFromPool(fData.Assembly, fData.Namespace, fData.Type, out string group, out int poolIndex);
						if (bData != null) {
							if (BehGUI(
								Layout.Rect(0, BEHAVIOUR_ITEM_HEIGHT),
								bData,
								m_SelectingGroup == group && m_SelectingBehIndex == poolIndex
							)) {
								clickingGroup = group;
								clickingBehIndex = poolIndex;
							}
						}
					}
				}
				if (favOpening != oldFavOpening) {
					SetBehaviourFolding("Favorite", favOpening);
				}
			}
			foreach (var pair in BehaviourPool) {
				var group = pair.Key;
				var pool = pair.Value;
				bool oldOpening = GetBehaviourFolding(group);
				if (string.IsNullOrEmpty(group) || pool is null || pool.Count == 0) { continue; }
				// Group
				bool opening = Layout.Fold(group, oldOpening);
				if (opening) {
					Layout.Space(2);
					bool hasBeh = false;
					// Pool
					for (int poolIndex = 0; poolIndex < pool.Count; poolIndex++) {
						var bData = pool[poolIndex];
						if (!bData.Favorite) {
							if (BehGUI(
								Layout.Rect(0, BEHAVIOUR_ITEM_HEIGHT),
								bData,
								m_SelectingGroup == group && m_SelectingBehIndex == poolIndex
							)) {
								clickingGroup = group;
								clickingBehIndex = poolIndex;
							}
							hasBeh = true;
						}
					}
					// Empty
					if (!hasBeh) {
						Layout.Space(6);
					}
				}
				if (opening != oldOpening) {
					SetBehaviourFolding(group, opening);
				}
			}
			// Clicking
			if (!string.IsNullOrEmpty(clickingGroup) && clickingBehIndex >= 0) {
				if (Event.current.button == 0) {
					// Left
					Undo.RecordObject(this, "Selection Change");
					SetSelectingBehaviour(clickingGroup, clickingBehIndex);
					Repaint();
				} else if (Event.current.button == 1) {
					var beh = GetBehaviourFromPool(clickingGroup, clickingBehIndex);
					if (beh != null) {
						BehaviourMenu(beh);
					}
				}
			}
			// Recompile
			Layout.Rect(1, 0);
			Layout.Space(4);
			var rect = Layout.Rect(0, 18);
			rect.x += rect.width - 70;
			rect.width = 70;
			if (GUI.Button(rect, "Recompile", EditorStyles.linkLabel)) {
				if (EditorUtil.Dialog("Confirm", "Recompile all scripts?", "Recompile", "Cancel")) {
					UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(
						UnityEditor.Compilation.RequestScriptCompilationOptions.None
					);
				}
			}
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
			Layout.Space(4);
			// Func
			bool BehGUI (Rect itemRect, BehaviourSerializeData bData, bool highlight) {
				var behaviour = bData.Behaviour;
				var iconRect = new Rect(itemRect.x + 16, itemRect.y, itemRect.height, itemRect.height);
				var labelRect = new Rect(iconRect.xMax + 6, itemRect.y, itemRect.width, itemRect.height);
				var favRect = new Rect(itemRect.xMax - itemRect.height - 6, itemRect.y + 5, itemRect.height, itemRect.height - 10);
				labelRect.width -= labelRect.x + itemRect.x - favRect.width - 6;
				// Highlight
				if (highlight) {
					EditorGUI.DrawRect(itemRect, HIGHLIGHT_TINT);
				}
				// Icon
				if (bData.Icon != null) {
					GUI.DrawTexture(iconRect, bData.Icon, ScaleMode.ScaleToFit);
				}
				// Label
				GUI.Label(
					labelRect,
					behaviour != null ? behaviour.FinalDisplayName : "",
					highlight ? HighlightLabelStyle : GUI.skin.label
				);
				// Click
				return mouseDown && itemRect.Expand(-24, 0, 0, 0).Contains(mousePos);
			}
		}


		private void GUI_Inspector () {

			// Title
			var oldB = GUI.backgroundColor;
			GUI.backgroundColor = TITLE_TINT;
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
				Layout.Space(3);
				GUI.Label(Layout.Rect(0, TITLE_HEIGHT), "Inspector", EditorStyles.label);
				// Help
				if (GUI.Button(Layout.Rect(TITLE_HEIGHT + 6, TITLE_HEIGHT), HelpContent, EditorStyles.toolbarButton)) {
					PixelJellyHelpWindow.Open();
				}
			}
			GUI.backgroundColor = oldB;
			var selectingBeh = GetSelectingBehaviour();
			if (selectingBeh == null || selectingBeh.Behaviour == null) { return; }
			var behaviour = selectingBeh.Behaviour;
			var bType = behaviour.GetType();

			// Header
			Layout.Space(4);
			using (new GUILayout.HorizontalScope()) {
				Layout.Space(2);
				// Icon
				int iconSize = TITLE_HEIGHT * 2 + 2;
				GUI.DrawTexture(Layout.Rect(iconSize, iconSize), selectingBeh.Icon, ScaleMode.ScaleToFit);
				Layout.Space(4);
				// Labels
				using (new GUILayout.VerticalScope()) {
					GUI.Label(Layout.Rect(0, TITLE_HEIGHT), behaviour.FinalDisplayName);
					Layout.Space(2);
					using (new GUILayout.HorizontalScope()) {
						var oldE = GUI.enabled;
						GUI.enabled = oldE;
						if (GUI.Button(Layout.Rect(100, TITLE_HEIGHT), JellyConfig.Main.GetDataSlot(bType), EditorStyles.popup)) {
							SlotMenu(behaviour);
						}
						//GUI.enabled = SelectingSlotIndex > 0;
						//if (GUI.Button(SelectingSlotCount <= 1 ? default : Layout.Rect(24, 18), ArrowLeftContent, ArrowStyle)) {
						//	SwitchDataSlot(false);
						//}
						//GUI.enabled = SelectingSlotIndex < SelectingSlotCount - 1;
						//if (GUI.Button(SelectingSlotCount <= 1 ? default : Layout.Rect(24, 18), ArrowRightContent, ArrowStyle)) {
						//	SwitchDataSlot(true);
						//}
						GUI.enabled = oldE;
					}
				}
				// Random Button
				if (selectingBeh.ShowRandomButton) {
					//GUI.backgroundColor = BUTTON_DARK_TINT;
					if (GUI.Button(Layout.Rect(iconSize, iconSize).Expand(-4f), new GUIContent(DiceIcon))) {
						RandomSeed(selectingBeh);
					}
					//GUI.backgroundColor = oldB;
				}
				Layout.Space(4);
			}
			Layout.Space(4);
			ColorLineGUI(0, 1, 0.4f);
			Layout.Space(4);

			// Content
			using (var scope = new GUILayout.ScrollViewScope(InspectorScrollPosition)) {
				InspectorScrollPosition = scope.scrollPosition;
				EditorGUI.BeginChangeCheck();
				selectingBeh.SerializedObject.Update();
				Layout.Space(4);
				JellyInspector inspector = null;
				for (
					var type = bType;
					type != null && type != typeof(JellyBehaviour);
					type = type.BaseType
				) {
					if (InspectorPool.ContainsKey(type)) {
						inspector = InspectorPool[type];
						break;
					}
				}
				float oldL = EditorGUIUtility.labelWidth;
				int oldI = EditorGUI.indentLevel;
				bool oldW = EditorGUIUtility.wideMode;
				EditorGUIUtility.labelWidth = 110;
				EditorGUI.indentLevel++;
				EditorGUIUtility.wideMode = true;
				if (inspector != null) {
					inspector.ClearParamHighlight();
					foreach (var msg in JellyBehaviour.Messages) {
						if (!string.IsNullOrEmpty(msg.HighlightParam)) {
							inspector.HighlightParam(msg.HighlightParam, msg.Type);
						}
					}
					inspector.OnPropertySwape(selectingBeh.SerializedObject);
					inspector.SwapeProperty("m_Seed", ShowSeed.Value ? "." : "");
					inspector.SwapeProperty("Seed", ShowSeed.Value ? "." : "");
					inspector.OnInspectorGUI(selectingBeh.SerializedObject, selectingBeh.SerializedProperties);
				} else {
					DEFAULT_SWAPER["m_Seed"] = DEFAULT_SWAPER["Seed"] = ShowSeed.Value ? "." : "";
					JellyInspector.DefaultInspectorGUI(selectingBeh.SerializedProperties, DEFAULT_SWAPER);
				}
				EditorGUIUtility.labelWidth = oldL;
				EditorGUI.indentLevel = oldI;
				EditorGUIUtility.wideMode = oldW;
				selectingBeh.SerializedObject.ApplyModifiedProperties();

				// Dirty Check
				if (EditorGUI.EndChangeCheck()) {
					CanvasDirty = true;
					EditorUtility.SetDirty(selectingBeh.Behaviour);
					EditorSceneManager.MarkAllScenesDirty();
				}

				// Buttons
				using (new GUILayout.VerticalScope(ButtonPanelStyle)) {
					const int BUTTON_HEIGHT = 24;
					Layout.Space(4);
					if (Layout.IconButton(Layout.Rect(0, BUTTON_HEIGHT), "Import Data", ScriptableIcon)) {
						ImportData(behaviour, EditorUtility.OpenFilePanel("Import Data", "Assets", "asset"));
					}
					Layout.Space(2);
					if (Layout.IconButton(Layout.Rect(0, BUTTON_HEIGHT), "Export Data", ScriptableIcon)) {
						ExportData(behaviour, EditorUtility.SaveFilePanelInProject("Export Data", behaviour.FinalDisplayName, "asset", "Export Scriptable Data"));
					}
					Layout.Space(2);
					if (Layout.IconButton(Layout.Rect(0, BUTTON_HEIGHT), "Export Texture", TextureIcon)) {
						ExportTexture(behaviour, EditorUtility.SaveFilePanelInProject("Export Texture", behaviour.FinalDisplayName, "png", "Export Texture"));
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						PixelPostprocessor.Clear();
					}
					if (behaviour.FrameCount > 1) {
						Layout.Space(2);
						if (Layout.IconButton(Layout.Rect(0, BUTTON_HEIGHT), "Export Animation", TextureIcon, AnimationIcon)) {
							ExportAnimation(behaviour, EditorUtility.SaveFilePanelInProject("Export Animation", JellyBehaviour.GetBehaviourAnimationName(behaviour), "anim", "Export Animation"));
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
							PixelPostprocessor.Clear();
						}
					}
					// Custom Buttons
					if (inspector != null) {
						inspector.OnCustomButtonGUI();
					}
					Layout.Space(8);
				}

				// Messages
				bool hasSolidMsg = false;
				foreach (var message in JellyBehaviour.Messages) {
					if (string.IsNullOrEmpty(message.Message)) { continue; }
					hasSolidMsg = true;
					if (!ShowInspectorMsg.Value) { continue; }
					EditorGUILayout.HelpBox(message.Message, (UnityEditor.MessageType)message.Type, true);
					Layout.Space(2);
				}
				// Show Ins Msg
				if (hasSolidMsg) {
					using (new GUILayout.HorizontalScope()) {
						Layout.Rect(0, 1);
						if (GUI.Button(
							Layout.Rect(TITLE_HEIGHT + 12, TITLE_HEIGHT),
							ShowInspectorMsg.Value ? HideInsMsgContent : ShowInsMsgContent,
							GUI.skin.label
						)) {
							ShowInspectorMsg.Value = !ShowInspectorMsg.Value;
						}
						EditorGUIUtility.AddCursorRect(Layout.LastRect(), MouseCursor.Link);
						Layout.Space(4);
					}
				}
				Layout.Space(62);
			}
			// Reset
			Layout.Rect(1, 0);
			Layout.Space(4);
			var rect = Layout.Rect(0, 18);
			rect.x += rect.width - 52;
			rect.width = 52;
			if (GUI.Button(rect, "Reset", EditorStyles.linkLabel)) {
				ResetBehaviour(behaviour);
			}
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
			Layout.Space(4);
		}


		private void GUI_Canvas () {

			var selectingBeh = GetSelectingBehaviour();
			JellyBehaviour.BeforePopulatePixels(selectingBeh?.Behaviour);

			var behaviour = selectingBeh?.Behaviour;
			int bWidth = behaviour != null ? behaviour.Width : 0;
			int bHeight = behaviour != null ? behaviour.Height : 0;
			int bFrameCount = behaviour != null ? behaviour.FrameCount : 0;
			int bSpriteCount = behaviour != null ? behaviour.SpriteCount : 0;
			int currentFrame = CurrentFrame = Mathf.Clamp(CurrentFrame, 0, bFrameCount > 0 ? bFrameCount - 1 : 0);
			if (IsPlayingAnimation && bFrameCount <= 1) {
				PauseAnimation(selectingBeh);
			}

			// Title
			var oldB = GUI.backgroundColor;
			GUI.backgroundColor = TITLE_TINT;
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
				Layout.Space(12);
				// Timeline
				if (behaviour != null && bFrameCount > 1) {
					// Slider
					var _rect = Layout.Rect(0, 17);
					_rect.y += 2;
					int newFrame = Mathf.RoundToInt(GUI.HorizontalSlider(_rect, currentFrame, 0, bFrameCount - 1));
					Layout.Space(12);
					newFrame = Mathf.Clamp(EditorGUI.DelayedIntField(Layout.Rect(32, 17).Expand(0f, 0f, -2f, 2f), newFrame), 0, bFrameCount - 1);
					if (newFrame != currentFrame) {
						CurrentFrame = currentFrame = newFrame;
					}
					Layout.Space(12);
					// Play/Pause
					var oldC = GUI.color;
					GUI.color = IsPlayingAnimation ? new Color(1f, 0.6f, 0.6f) : oldC;
					if (GUI.Button(Layout.Rect(TITLE_HEIGHT + 12, TITLE_HEIGHT), IsPlayingAnimation ? PauseContent : PlayContent, EditorStyles.toolbarButton)) {
						if (IsPlayingAnimation) {
							PauseAnimation(selectingBeh);
						} else {
							PlayAnimation();
						}
					}
					GUI.color = oldC;
				} else {
					Layout.Rect(0, 17);
				}
				// Show Comment
				ShowComment.Value = GUI.Toggle(
					HasComment ? Layout.Rect(TITLE_HEIGHT + 12, TITLE_HEIGHT) : new Rect(),
					ShowComment.Value,
					new GUIContent("//", "Show Comments [C]"),
					EditorStyles.toolbarButton
				);
				// Checker Floor
				DrawCheckerFloor.Value = GUI.Toggle(
					Layout.Rect(TITLE_HEIGHT + 12, TITLE_HEIGHT),
					DrawCheckerFloor.Value,
					CheckerFloorContent,
					EditorStyles.toolbarButton
				);
			}
			GUI.backgroundColor = oldB;

			// Null Check
			if (behaviour == null) { return; }
			bool noFrame = bFrameCount <= 0;
			bool noSprite = bSpriteCount <= 0;
			bool tooManyFrame = bFrameCount > MAX_FRAME_SIZE;
			bool tooManySprite = bSpriteCount > MAX_SPRITE_SIZE;
			bool canvasTooSmall = bWidth <= 0 || bHeight <= 0;
			bool canvasTooLarge = bWidth > MAX_CANVAS_SIZE || bHeight > MAX_CANVAS_SIZE;
			// Size Check
			if (noFrame || noSprite || tooManyFrame || tooManySprite || canvasTooSmall || canvasTooLarge) {
				Layout.Rect(0, 0);
				using (new GUILayout.HorizontalScope()) {
					Layout.Rect(120, 0);
					EditorGUILayout.HelpBox(
						noFrame ? $" FrameCount should be larger than 0." :
						noSprite ? $" SpriteCount should be larger than 0." :
						tooManyFrame ? $"Too many frames. FrameCount should be smaller than {MAX_FRAME_SIZE}." :
						tooManySprite ? $"Too many sprites. SpriteCount should be smaller than {MAX_SPRITE_SIZE}." :
						canvasTooSmall ? "Width and Height should be larger than 0." :
						canvasTooLarge ? $"Canvas too large. Width and Height should be smaller than {MAX_CANVAS_SIZE}" :
						"Failed to render canvas.",
						UnityEditor.MessageType.Warning,
						true
					);
					Layout.Rect(120, 0);
				}
				Layout.Rect(0, 0);
				Layout.Rect(0, 0);
				return;
			}

			// Canvas
			if (bFrameCount != ColorBlock.colors.GetLength(0) ||
				bSpriteCount != ColorBlock.colors.GetLength(1) ||
				bWidth != ColorBlock.width ||
				bHeight != ColorBlock.height
			) {
				CanvasDirty = true;
			}
			if (CanvasDirty) {
				// Check
				if (ColorBlock.colors.GetLength(0) != bFrameCount || ColorBlock.colors.GetLength(1) != bSpriteCount) {
					ResizeColorBlock(bWidth, bHeight, bFrameCount, bSpriteCount);
				}
				for (int i = 0; i < bFrameCount; i++) {
					for (int j = 0; j < bSpriteCount; j++) {
						if (ColorBlock.colors[i, j].Length != bWidth * bHeight) {
							ResizeColorBlock(bWidth, bHeight, bFrameCount, bSpriteCount);
						}
					}
				}
				if (CanvasTextures.GetLength(0) != bFrameCount || CanvasTextures.GetLength(1) != bSpriteCount) {
					CanvasTextures = new Texture2D[bFrameCount, bSpriteCount];
				}
				// Textures
				JellyBehaviour.Messages.Clear();
				HasComment = false;
				for (int frame = 0; frame < bFrameCount; frame++) {
					for (int sprite = 0; sprite < bSpriteCount; sprite++) {
						/////////////////// DO THE MAGIC ///////////////////
						JellyBehaviour.PopulatePixels(
							behaviour,
							bWidth, bHeight,
							frame, bFrameCount,
							sprite, bSpriteCount,
							out _, out _
						);
						/////////////////// MAGIC DONE ///////////////////
						Comments[frame, sprite].Clear();
						Comments[frame, sprite].AddRange(JellyBehaviour.Comments);
						JellyBehaviour.CopyPixelsTo(ColorBlock.colors[frame, sprite]);
						if (Comments[frame, sprite].Count > 0) {
							HasComment = true;
						}
						if (CanvasTextures[frame, sprite] == null) {
							CanvasTextures[frame, sprite] = new Texture2D(bWidth, bHeight, TextureFormat.ARGB32, false) {
								filterMode = FilterMode.Point,
								alphaIsTransparency = true,
							};
						}
						var texture = CanvasTextures[frame, sprite];
						if (texture.width != bWidth || texture.height != bHeight) {
							texture.Reinitialize(bWidth, bHeight);
						}
						texture.SetPixels32(ColorBlock.colors[frame, sprite]);
						texture.Apply();
					}
				}
				JellyBehaviour.AfterPopulatePixels(behaviour);
				// Final
				CanvasDirty = false;
				Repaint();
			}
			// Content
			var textureRect = Layout.Rect(0, 0);
			const float GAP = 24;
			textureRect.x += GAP;
			textureRect.y += GAP;
			textureRect.width -= GAP * 2;
			textureRect.height -= GAP * 2;
			var bgColor = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.03f) : new Color(0, 0, 0, 0.1f);
			// Canvas GUI
			{
				float aspect = (float)bWidth / bHeight;
				float targetAspect = textureRect.width / textureRect.height;
				int countX = Mathf.RoundToInt(
					Mathf.Sqrt(bSpriteCount) * (MAGICAL_GRID_CURVE_ALT.Evaluate(targetAspect) / aspect) * MAGICAL_GRID_CURVE.Evaluate(aspect)
				);
				countX = Mathf.Max(countX, 1);
				int countY = Mathf.CeilToInt((float)bSpriteCount / countX);
				const float CELL_GAP = 8f;
				Vector2 cellSize = GetFitInSize(
					(textureRect.width - CELL_GAP * (countX - 1)) / countX,
					(textureRect.height - CELL_GAP * (countY - 1)) / countY,
					aspect
				);
				float offsetX = textureRect.x + (textureRect.width - countX * cellSize.x - CELL_GAP * (countX - 1)) / 2f;
				float offsetY = textureRect.y + (textureRect.height - countY * cellSize.y - CELL_GAP * (countY - 1)) / 2f;
				if (cellSize.x > CELL_GAP && cellSize.y > CELL_GAP) {
					int sprite = 0;
					for (int y = 0; y < countY && sprite < bSpriteCount; y++) {
						for (int x = 0; x < countX && sprite < bSpriteCount; x++, sprite++) {
							var cellRect = new Rect(
								offsetX + x * (cellSize.x + CELL_GAP),
								offsetY + y * (cellSize.y + CELL_GAP),
								cellSize.x,
								cellSize.y
							);
							var _realRect = cellRect.Fit((float)bWidth / bHeight);
							GUITextureBack(_realRect, bWidth, bHeight, bgColor);
							GUI.DrawTexture(cellRect, CanvasTextures[currentFrame, sprite], ScaleMode.ScaleToFit);
						}
					}
					sprite = 0;
					for (int y = 0; y < countY && sprite < bSpriteCount; y++) {
						for (int x = 0; x < countX && sprite < bSpriteCount; x++, sprite++) {
							var cellRect = new Rect(
								offsetX + x * cellSize.x,
								offsetY + y * cellSize.y,
								cellSize.x - CELL_GAP,
								cellSize.y - CELL_GAP
							);
							var _realRect = cellRect.Fit((float)bWidth / bHeight);
							GUIComments(_realRect, bWidth, bHeight, Comments[currentFrame, sprite]);
						}
					}
				}
			}
			// Func
			static Vector2 GetFitInSize (float boxX, float boxY, float aspect) => aspect > boxX / boxY ? new Vector2(boxX, boxX / aspect) : new Vector2(boxY * aspect, boxY);
		}


		private void GUI_Hotkey () {
			if (Event.current.type != EventType.KeyDown) { return; }
			bool performed = false;
			var selectingBeh = GetSelectingBehaviour();
			var behaviour = selectingBeh?.Behaviour;
			bool ctrl = Event.current.control;
			bool alt = Event.current.alt;
			bool shift = Event.current.shift;
			if (!ctrl && !alt && !shift) {
				switch (Event.current.keyCode) {
					case KeyCode.Comma: {
						if (behaviour != null && behaviour.FrameCount > 1) {
							SeekFrame(CurrentFrame - 1, behaviour.FrameCount);
						}
						performed = true;
						break;
					}
					case KeyCode.Period: {
						if (behaviour != null && behaviour.FrameCount > 1) {
							SeekFrame(CurrentFrame + 1, behaviour.FrameCount);
						}
						performed = true;
						break;
					}
					case KeyCode.G: {
						DrawCheckerFloor.Value = !DrawCheckerFloor.Value;
						performed = true;
						break;
					}
					case KeyCode.C: {
						if (HasComment) {
							ShowComment.Value = !ShowComment.Value;
						}
						performed = true;
						break;
					}
					case KeyCode.R: {
						RandomSeed(selectingBeh);
						performed = true;
						break;
					}
					case KeyCode.P: {
						if (behaviour != null && behaviour.FrameCount > 1) {
							if (IsPlayingAnimation) {
								PauseAnimation(selectingBeh);
							} else {
								PlayAnimation();
							}
						}
						performed = true;
						break;
					}
					case KeyCode.Minus: {
						SwitchDataSlot(false);
						performed = true;
						break;
					}
					case KeyCode.Equals: {
						SwitchDataSlot(true);
						performed = true;
						break;
					}
				}
			}
			if (performed) {
				Event.current.Use();
				Repaint();
			}
		}


	}
}

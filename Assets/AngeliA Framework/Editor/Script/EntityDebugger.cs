using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class EntityDebugger : EditorWindow {




		#region --- VAR ---

		// Const
		private const string WINDOW_TITLE = "Entity";
		private static readonly int PIXEL_CODE = "Pixel".ACode();
		private static readonly Color32[] COLLIDER_TINT = {
			new (255, 128, 0, 255),
			new (255, 255, 0, 255),
			new (0, 255, 0, 255),
			new (0, 255, 255, 255),
			new (0, 0, 255, 255),
			new (255, 0, 255, 255),
			new (255, 0, 0, 255),
		};
		private const double ARTWORK_ALRT_DURATION = 3d;

		// Short
		private static EntityDebugger Main = null;
		private static GUIContent EIconContent => _EIconContent ??= EditorGUIUtility.IconContent("d_GameObject Icon");
		private static GUIContent _EIconContent = null;
		private static GUIContent SaveIconContent => _SaveIconContent ??= EditorGUIUtility.IconContent("d_SaveAs@2x");
		private static GUIContent _SaveIconContent = null;
		private static Game Game => _Game != null ? _Game : (_Game = FindObjectOfType<Game>());
		private static Game _Game = null;

		// Data
		private static Entity[][] Entities = null;
		private static Entity SelectingEntity = null;
		private static Entity HoveringEntity = null;
		private static EntityInspector SelectingInspector = null;
		private static bool PrevUnityFocused = true;
		private static double RequireAlertTime = double.MinValue;
		private static string AlertMessage = "";
		private Vector2 MasterScrollPos = default;
		private int EntityDirtyFlag = 0;
		private bool PrevUpdateMousePress = false;

		// Saving
		private static readonly EditorSavingBool ShowColliders = new("EntityDebuger.ShowColliders", false);
		private static readonly EditorSavingString EntityLayerVisible = new("EntityDebuger.EntityLayerVisible", "");
		private static readonly EditorSavingString LastSyncTick = new("LdtkToolkit.LastSyncTick", "0");


		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		private static void Init () {

			// State Change
			EditorApplication.playModeStateChanged += (mode) => {

				if (!HasOpenInstances<EntityDebugger>()) { return; }

				// Enter Edit
				if (mode == PlayModeStateChange.EnteredEditMode) {
					Entities = null;
				}

				// Enter Play
				if (mode == PlayModeStateChange.EnteredPlayMode) {
					// Reload Cache
					Entities = null;
					if (Main != null) {
						Main.ClearSelectionInspector();
					}
				}

			};

			// Scene Dirty
			UnityEditor.SceneManagement.EditorSceneManager.sceneDirtied += (scene) => {
				if (Main != null) {
					Main.Repaint();
				}
			};

			// Artwork Dirty Check
			EditorApplication.update += () => {
				bool focused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
				if (focused != PrevUnityFocused) {
					PrevUnityFocused = focused;
					if (focused) {
						// On Back to Unity
						if (!long.TryParse(LastSyncTick.Value, out long lastSyncTickValue)) {
							lastSyncTickValue = 0;
						}
						bool ldtk = false;
						bool ase = false;
						foreach (var file in Util.GetFilesIn(LdtkToAngeliA.LdtkToolkit.LdtkRoot, false, "*.ldtk")) {
							try {
								if (Util.GetModifyDate(file.FullName) > lastSyncTickValue) {
									ldtk = true;
									break;
								}
							} catch (System.Exception ex) { Debug.LogException(ex); }
						}
						foreach (var file in Util.GetFilesIn(Application.dataPath, false, "*.ase", "*.aseprite")) {
							try {
								if (Util.GetModifyDate(file.FullName) > lastSyncTickValue) {
									ase = true;
									break;
								}
							} catch (System.Exception ex) { Debug.LogException(ex); }
						}
						if (ldtk || ase) {
							if (Main != null) {
								Main.SyncArtwork(ldtk, ase);
							}
						}
					}
				}
			};

		}


		[RuntimeInitializeOnLoadMethod]
		private static void RuntimeInit () {
			CellRenderer.BeforeUpdate += () => { if (Main != null) Main.DrawGizmos(); };
		}


		[MenuItem("AngeliA/Entity Debuger")]
		private static void OpenWindow () => GetOrCreateWindow();


		[MenuItem("AngeliA/Command/Reload Sheet Assets")]
		private static void ReloadSheetAssets () {
			foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(SpriteSheet)}")) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var sheet = AssetDatabase.LoadAssetAtPath<SpriteSheet>(path);
				var sprites = new List<Sprite>();
				var tPath = AssetDatabase.GetAssetPath(sheet.Texture);
				var objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(tPath);
				for (int i = 0; i < objs.Length; i++) {
					var obj = objs[i];
					if (obj != null && obj is Sprite sp) {
						sprites.Add(sp);
					}
				}
				if (objs.Length > 0) {
					sheet.SetSprites(sprites.ToArray());
				}
				EditorUtility.SetDirty(sheet);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		private void OnEnable () {
			Main = this;
		}


		private void OnGUI () {
			Main = this;
			if (EditorApplication.isPlaying) {
				// Play Mode
				OnGUI_Runtime();
			} else {
				// Edit Mode
				OnGUI_Edittime();
			}
			Layout.CancelFocusOnClick(this, true);
		}


		private void Update () {
			wantsMouseMove = EditorApplication.isPlaying;
			if (EditorApplication.isPlaying) {
				// Repaint on Entity Dirty
				if (Game != null && Game.EntityDirtyFlag != EntityDirtyFlag) {
					EntityDirtyFlag = Game.EntityDirtyFlag;
					Repaint();
				}
				// Deselect
				if (SelectingEntity != null && !SelectingEntity.Active) {
					SelectingEntity = null;
					Selection.activeObject = null;
					if (SelectingInspector != null) {
						SelectingInspector.SetTarget(null);
						SelectingInspector.InspectorMode = EntityInspector.Mode.Entity;
					}
					Repaint();
				}
				// Click to Select Entity
				bool mousePressing = Input.GetMouseButton(0);
				if (mousePressing && !PrevUpdateMousePress && Entities != null) {
					var cRect = CellRenderer.CameraRect;
					var mousePos = new Vector2Int(
						(int)Mathf.LerpUnclamped(cRect.xMin, cRect.xMax, FrameInput.MousePosition01.x),
						(int)Mathf.LerpUnclamped(cRect.yMin, cRect.yMax, FrameInput.MousePosition01.y)
					);
					for (int layerIndex = Entities.Length - 1; layerIndex >= 0; layerIndex--) {
						var entities = Entities[layerIndex];
						int len = entities.Length;
						for (int i = 0; i < len; i++) {
							var e = entities[i];
							if (e == null) break;
							if (e.Rect.Contains(mousePos)) {
								SetSelectionInspector(e, EntityInspector.Mode.Entity);
								Repaint();
								goto LoopEnd;
							}
						}
					}
				LoopEnd:;
				}
				PrevUpdateMousePress = mousePressing;
			} else {
				PrevUpdateMousePress = false;
			}
			// Repaint when Alert
			if (EditorApplication.timeSinceStartup < RequireAlertTime + ARTWORK_ALRT_DURATION + 1f) {
				Repaint();
			}
		}


		private void OnFocus () => Repaint();


		private void OnLostFocus () => Repaint();


		private void OnGUI_Edittime () {

			// Toolbar
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {

				double time = EditorApplication.timeSinceStartup;

				if (time < RequireAlertTime + ARTWORK_ALRT_DURATION) {
					// Alert
					EditorGUI.DrawRect(
						Layout.Rect(0, 20),
						new Color32(64, 128, 128, (byte)Util.Remap(0f, 0.1f, 200, 255, Mathf.PingPong((float)(time - RequireAlertTime), 0.1f)))
					);
					GUI.Label(Layout.LastRect(), AlertMessage, Layout.CenteredLabel);
				} else {
					// Fake Alert
					Layout.Rect(0, 20);
					EditorGUI.DrawRect(default, Color.clear);
					GUI.Label(default, GUIContent.none);
				}

				// Dirty Mark
				var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
				if (scene.IsValid() && scene.isLoaded && scene.isDirty) {
					EditorGUI.DrawRect(Layout.Rect(20, 20), new Color32(209, 136, 60, 255));
					var oldC = GUI.color;
					GUI.color = new Color32(42, 42, 42, 255);
					GUI.Label(Layout.LastRect(), SaveIconContent);
					GUI.color = oldC;
				}
				Layout.Space(2);


			}

			Layout.Space(12);

			// Sync Artwork
			if (GUI.Button(Layout.Rect(0, 24).Shrink(24, 24, 0, 0), "Sync Artwork")) {
				SyncArtwork(true, true);
			}

			Layout.Space(12);

			// Language Editor
			if (GUI.Button(Layout.Rect(0, 24).Shrink(24, 24, 0, 0), "Language Editor")) {
				LanguageEditor.OpenEditor();
			}



		}


		private void OnGUI_Runtime () {

			// Game
			if (Game == null) return;

			// Entities
			if (Entities == null) {
				Entities = Util.GetFieldValue(Game, "Entities") as Entity[][];
				if (Entities == null) { return; }
			}

			if (SelectingInspector == null) {
				SelectingInspector = CreateInstance<EntityInspector>();
				SelectingInspector.Game = Game;
			}

			// Toolbar
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
				// Layer
				for (int i = 0; i < Const.ENTITY_LAYER_COUNT; i++) {
					bool visible = GetLayerVisible(i);
					string label = ((EntityLayer)i).ToString();
					bool newVisible = GUI.Toggle(
						Layout.Rect(0, 20),
						visible,
						label[..Mathf.Min(label.Length, 5)],
						EditorStyles.toolbarButton
					);
					if (visible) {
						var rect = Layout.LastRect();
						EditorGUI.DrawRect(rect.Shrink(3, 3, rect.height - 2, 1), Layout.HighlightColor1);
					}
					if (newVisible != visible) {
						SetLayerVisible(i, newVisible);
					}
				}
			}

			// Content
			using var scope = new GUILayout.ScrollViewScope(MasterScrollPos);
			MasterScrollPos = scope.scrollPosition;
			HoveringEntity = null;
			for (int i = 0, count = 0; i < Const.ENTITY_LAYER_COUNT; i++) {
				if (GetLayerVisible(i)) {
					if (count != 0) {
						Layout.Space(1);
						EditorGUI.DrawRect(Layout.Rect(0, 1), new Color32(50, 50, 50, 255));
						Layout.Space(1);
					}
					GUI_Entity(i);
					count++;
				}
			}

			// Menu on Empty Space
			if (Event.current.type == EventType.MouseDown && Event.current.button == 1) {
				EntityMenu(null);
				Event.current.Use();
				Repaint();
			}

			// Key
			if (Event.current.type == EventType.KeyDown) {
				switch (Event.current.keyCode) {
					case KeyCode.Delete:
						if (SelectingEntity != null) {
							SelectingEntity.Active = false;
							EditorApplication.delayCall += Repaint;
						}
						break;
				}
			}

			// Clear Selection on Mouse Down
			if (
				(SelectingEntity != null || SelectingInspector.InspectorMode != EntityInspector.Mode.Entity) &&
				Event.current.type == EventType.MouseDown
			) {
				ClearSelectionInspector();
			}

		}


		private void GUI_Entity (int layerIndex) {

			var entities = Entities[layerIndex];
			int capacity = entities.Length;
			const int HEIGHT = 18;

			if (SelectingEntity != null && !SelectingEntity.Active) {
				ClearSelectionInspector();
			}

			bool setSelectionToPrev = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.UpArrow;
			bool setSelectionToNext = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.DownArrow;
			bool focusing = focusedWindow == this;

			// Table
			if (capacity > 0) {

				bool mouseLeftDown = Event.current.type == EventType.MouseDown && Event.current.button == 0;
				bool mouseRightDown = Event.current.type == EventType.MouseDown && Event.current.button == 1;
				bool mouseDown = mouseLeftDown || mouseRightDown;
				bool mouseEvent = Event.current.isMouse;

				// List
				Entity prevE = null;
				for (int i = 0; i < capacity; i++) {

					var entity = entities[i];
					if (entity == null) break;

					var rect = Layout.Rect(0, HEIGHT);
					bool mouseInside = rect.Contains(Event.current.mousePosition);

					GUI.Label(rect, GUIContent.none, EditorStyles.toolbarButton);

					// BG
					if (entity == SelectingEntity && entity != null) {
						EditorGUI.DrawRect(rect, focusing ? Layout.HighlightColor : new Color32(72, 72, 72, 255));
					}

					// Icon
					GUI.Label(rect.Shrink(4, 0, 2, 2), EIconContent, EditorStyles.miniLabel);

					// Mouse Down
					if (mouseDown && mouseInside) {
						if (mouseLeftDown) {
							SetSelectionInspector(entity, EntityInspector.Mode.Entity);
						}
						if (mouseRightDown) {
							EntityMenu(entity);
						}
						Event.current.Use();
						Repaint();
					}

					// Hover
					if (mouseEvent && mouseInside) {
						HoveringEntity = entity;
					}

					// Key
					if (SelectingEntity != null) {
						if (setSelectionToPrev && entity == SelectingEntity && prevE != null) {
							SetSelectionInspector(prevE, EntityInspector.Mode.Entity);
							setSelectionToPrev = false;
							Repaint();
						}
						if (setSelectionToNext && prevE == SelectingEntity) {
							SetSelectionInspector(entity, EntityInspector.Mode.Entity);
							setSelectionToNext = false;
							Repaint();
						}
					}

					// Type Name
					GUI.Label(rect.Shrink(20, 0, 0, 0), entity.GetType().Name);

					// Layer Char
					GUI.Label(
						rect.Shrink(rect.width - 16, 0, 0, 0),
						((EntityLayer)layerIndex).ToString()[0].ToString(),
						EditorStyles.centeredGreyMiniLabel
					);

					prevE = entity;
				}

			}
		}


		private void DrawGizmos () {
			// Colliders
			if (ShowColliders.Value) {
				CellPhysics.Editor_ForAllCells((layer, info) => {
					var tint = COLLIDER_TINT[layer];
					tint.a = (byte)(info.IsTrigger ? 128 : 255);
					CellRenderer.Draw(PIXEL_CODE, info.Rect, tint);
				});
			}
			// Selection
			if (SelectingEntity != null && SelectingEntity.Active) {
				byte alpha = (byte)(Time.time % 0.618f > 0.618f / 2f ? 255 : 128);
				CellGUI.Draw_9Slice(
					SelectingEntity.Rect, new Color32(255, 255, 255, alpha), NineSliceSprites.PIXEL_FRAME_6
				);
				CellGUI.Draw_9Slice(
					SelectingEntity.Rect.Shrink(6), new Color32(0, 0, 0, alpha), NineSliceSprites.PIXEL_FRAME_6
				);
			}
			// Hover
			if (HoveringEntity != null && HoveringEntity.Active) {
				var color = Layout.HighlightColor;
				color.a = Time.time % 0.618f > 0.618f / 2f ? 0.5f : 0.3f;
				CellRenderer.Draw(PIXEL_CODE, HoveringEntity.Rect, color);
			}
		}


		#endregion




		#region --- LGC ---


		private static EntityDebugger GetOrCreateWindow () {
			try {
				var window = GetWindow<EntityDebugger>(WINDOW_TITLE, false);
				window.minSize = new Vector2(275, 400);
				window.maxSize = new Vector2(600, 1000);
				window.titleContent = EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow");
				window.titleContent.text = WINDOW_TITLE;
				return window;
			} catch (System.Exception ex) {
				Debug.LogWarning("Failed to open window.\n" + ex.Message);
			}
			return null;
		}


		private void EntityMenu (Entity entity) {
			var menu = new GenericMenu();
			// View
			menu.AddItem(new GUIContent("Select View"), false, () => {
				SetSelectionInspector(null, EntityInspector.Mode.View);
			});
			// Collider
			menu.AddItem(new GUIContent("Show Colliders"), ShowColliders.Value, () =>
				ShowColliders.Value = !ShowColliders.Value
			);
			// Delete
			if (entity != null) {
				menu.AddItem(new GUIContent("Delete"), false, () => {
					if (entity != null) {
						entity.Active = false;
					}
				});
			} else {
				menu.AddDisabledItem(new GUIContent("Delete"), false);
			}
			// Show
			menu.ShowAsContext();
		}


		private void SetSelectionInspector (Entity entity, EntityInspector.Mode mode) {
			SelectingEntity = entity;
			if (SelectingInspector != null) {
				SelectingInspector.SetTarget(entity);
				SelectingInspector.InspectorMode = mode;
			}
			Selection.activeObject = SelectingInspector;
		}


		private void ClearSelectionInspector () {
			SelectingEntity = null;
			Selection.activeObject = null;
			if (SelectingInspector != null) {
				SelectingInspector.SetTarget(null);
				SelectingInspector.InspectorMode = EntityInspector.Mode.Entity;
			}
		}


		private bool GetLayerVisible (int index) =>
			index < EntityLayerVisible.Value.Length && EntityLayerVisible.Value[index] == '1';


		private void SetLayerVisible (int index, bool visible) {
			if (index < 0 || index >= Const.ENTITY_LAYER_COUNT) return;
			if (EntityLayerVisible.Value.Length != Const.ENTITY_LAYER_COUNT) {
				EntityLayerVisible.Value = new string('0', Const.ENTITY_LAYER_COUNT);
			}
			var builder = new StringBuilder(EntityLayerVisible.Value);
			builder[index] = visible ? '1' : '0';
			EntityLayerVisible.Value = builder.ToString();
		}


		private void SyncArtwork (bool ldtk, bool aseprite) {
			if (aseprite) {
				EditorApplication.ExecuteMenuItem("Tools/Aseprite Toolbox/Create Sprite for All");
				ReloadSheetAssets();
			}
			if (ldtk) {
				LdtkToAngeliA.LdtkToolkit.ReloadAllLevels();
			}
			LastSyncTick.Value = System.DateTime.Now.Ticks.ToString();
			LogAlert($"{(ldtk ? "LDTK " : "")}{(aseprite ? "ASE " : "")}Synced");
		}


		private void LogAlert (string message) {
			AlertMessage = message;
			RequireAlertTime = EditorApplication.timeSinceStartup;
		}


		#endregion




	}



	[PreferBinarySerialization]
	public class EntityInspector : ScriptableObject {
		public enum Mode {
			Entity = 0,
			View = 1,
		}
		public Entity Target { get; private set; } = null;
		public Game Game { get; set; } = null;
		public Mode InspectorMode { get; set; } = Mode.Entity;
		public void SetTarget (Entity target) {
			Target = target;
			name = target != null ? target.GetType().Name : "";
		}
	}



	[CustomEditor(typeof(EntityInspector))]
	public class EntityInspector_Inspector : UnityEditor.Editor {


		private static readonly Dictionary<System.Type, FieldInfo[]> EntityFieldMap = new();
		private static readonly Dictionary<System.Type, bool> AngeliaInspectorOpening = new();



		public override void OnInspectorGUI () {
			switch ((target as EntityInspector).InspectorMode) {
				case EntityInspector.Mode.Entity:
					GUI_Entity();
					break;
				case EntityInspector.Mode.View:
					GUI_View();
					break;
			}
		}


		public override bool RequiresConstantRepaint () => (target as EntityInspector).Target != null;


		private void GUI_View () {
			var game = (target as EntityInspector).Game;
			var viewRect = game.ViewRect;
			var spawnRect = (RectInt)Util.GetFieldValue(game, "SpawnRect");
			var cameraRect = CellRenderer.CameraRect;

			var newView = EditorGUILayout.RectIntField(new GUIContent("View"), viewRect);
			if (newView.IsNotSame(viewRect)) {
				game.SetViewPositionDely(newView.x, newView.y);
				game.SetViewSizeDely(newView.width, newView.height);
			}
			Layout.Space(2);

			bool oldE = GUI.enabled;
			GUI.enabled = false;
			EditorGUILayout.RectIntField(new GUIContent("Spawn"), spawnRect);
			Layout.Space(2);

			EditorGUILayout.RectIntField(new GUIContent("Camera"), cameraRect);
			Layout.Space(2);
			GUI.enabled = oldE;
		}


		private void GUI_Entity () {
			const int HEIGHT = 18;
			var eTarget = target as EntityInspector;
			if (eTarget.Target == null) { return; }
			FieldInfo[] fields = GetFieldsFromPool(eTarget.Target.GetType());
			Layout.Space(4);

			// Instance ID
			using (new EditorGUI.DisabledGroupScope(true)) {
				EditorGUI.TextField(Layout.Rect(0, HEIGHT), "Instance ID", eTarget.Target.InstanceID.ToString());
			}
			Layout.Space(2);

			// X
			eTarget.Target.X = EditorGUI.IntField(Layout.Rect(0, HEIGHT), "X", eTarget.Target.X);
			Layout.Space(2);

			// Y
			eTarget.Target.Y = EditorGUI.IntField(Layout.Rect(0, HEIGHT), "Y", eTarget.Target.Y);
			Layout.Space(2);

			// Width
			eTarget.Target.Width = EditorGUI.IntField(Layout.Rect(0, HEIGHT), "Width", eTarget.Target.Width);
			Layout.Space(2);

			// Height
			eTarget.Target.Height = EditorGUI.IntField(Layout.Rect(0, HEIGHT), "Height", eTarget.Target.Height);
			Layout.Space(2);

			// Fields
			foreach (var field in fields) {
				if (field.GetCustomAttribute<AngeliaInspectorAttribute>(false) != null) {
					bool open = GetOpeningFromPool(field.FieldType);
					if (Layout.Fold(field.Name, ref open)) {
						using (new EditorGUI.IndentLevelScope(1)) {
							DrawEntityInspector(field.GetValue(eTarget.Target), field.FieldType);
						}
						Layout.Space(4);
					}
					SetOpeningToPool(field.FieldType, open);
					continue;
				}
				if (!field.IsPublic && field.GetCustomAttribute<SerializeField>(false) == null) { continue; }
				using (new GUILayout.HorizontalScope()) {
					field.SetValue(eTarget.Target, Field(
						Util.GetDisplayName(field.Name),
						field.GetValue(eTarget.Target),
						field.FieldType
					));
				}
				Layout.Space(2);
			}
		}


		private void DrawEntityInspector (object target, System.Type type) {
			var fields = GetFieldsFromPool(type);
			foreach (var field in fields) {
				var cAtt = field.GetCustomAttribute<CompilerGeneratedAttribute>();
				if (
					field.IsPublic || field.GetCustomAttribute<SerializeField>(false) != null ||
					cAtt != null
				) {
					string fName = Util.GetDisplayName(field.Name);
					if (cAtt != null && fName.StartsWith('<')) {
						int _index = fName.IndexOf('>');
						if (_index >= 0) {
							fName = fName[1.._index];
						}
					}
					using (new GUILayout.HorizontalScope()) {
						field.SetValue(target, Field(
							fName,
							field.GetValue(target),
							field.FieldType
						));
					}
					Layout.Space(2);
				}
			}
		}


		private object Field (string label, object value, System.Type type) {
			var _type = type;
			if (_type.IsSubclassOf(typeof(Object))) {
				_type = typeof(Object);
			}
			if (_type.IsSubclassOf(typeof(System.Enum))) {
				_type = typeof(System.Enum);
			}

			return true switch {

				bool when _type == typeof(sbyte) => (sbyte)Mathf.Clamp(EditorGUILayout.IntField(label, (sbyte)value), sbyte.MinValue, sbyte.MaxValue),
				bool when _type == typeof(byte) => (byte)Mathf.Clamp(EditorGUILayout.IntField(label, (byte)value), byte.MinValue, byte.MaxValue),
				bool when _type == typeof(ushort) => (ushort)Mathf.Clamp(EditorGUILayout.IntField(label, (ushort)value), ushort.MinValue, ushort.MaxValue),
				bool when _type == typeof(short) => (short)Mathf.Clamp(EditorGUILayout.IntField(label, (short)value), short.MinValue, short.MaxValue),
				bool when _type == typeof(int) => EditorGUILayout.IntField(label, (int)value),
				bool when _type == typeof(long) => EditorGUILayout.LongField(label, (long)value),
				bool when _type == typeof(ulong) => (ulong)EditorGUILayout.LongField(label, (long)(ulong)value),
				bool when _type == typeof(float) => EditorGUILayout.FloatField(label, (float)value),
				bool when _type == typeof(double) => (double)EditorGUILayout.DoubleField(label, (double)value),
				bool when _type == typeof(string) => EditorGUILayout.DelayedTextField(label, (string)value),
				bool when _type == typeof(bool) => EditorGUILayout.Toggle(label, (bool)value),

				bool when _type == typeof(System.Enum) => type.GetCustomAttribute<System.FlagsAttribute>() == null ?
					EditorGUILayout.EnumPopup(label, (System.Enum)value) :
					EditorGUILayout.EnumFlagsField(label, (System.Enum)value),

				bool when _type == typeof(Vector2) => EditorGUILayout.Vector2Field(label, (Vector2)value),
				bool when _type == typeof(Vector3) => EditorGUILayout.Vector3Field(label, (Vector3)value),
				bool when _type == typeof(Vector4) => EditorGUILayout.Vector4Field(label, (Vector4)value),
				bool when _type == typeof(Vector2Int) => EditorGUILayout.Vector2IntField(label, (Vector2Int)value),
				bool when _type == typeof(Vector3Int) => EditorGUILayout.Vector3IntField(label, (Vector3Int)value),
				bool when _type == typeof(Color32) => (Color32)EditorGUILayout.ColorField(new GUIContent(label), (Color32)value, false, true, false),
				bool when _type == typeof(Color) => EditorGUILayout.ColorField(new GUIContent(label), (Color)value, false, true, false),

				bool when _type == typeof(Object) => EditorGUILayout.ObjectField(new GUIContent(label), (Object)value, type, false),

				_ => null,
			};
		}


		private FieldInfo[] GetFieldsFromPool (System.Type type) {
			if (EntityFieldMap.ContainsKey(type)) {
				return EntityFieldMap[type];
			} else {
				var fList = new List<FieldInfo>();
				for (
					var _type = type;
					_type != null && _type != typeof(Entity) && _type != typeof(object);
					_type = _type.BaseType
				) {
					fList.InsertRange(0, _type.GetFields(
						BindingFlags.Public | BindingFlags.NonPublic |
						BindingFlags.Instance | BindingFlags.DeclaredOnly
					));
				}
				return fList.ToArray();
			}
		}


		private bool GetOpeningFromPool (System.Type type) {
			if (!AngeliaInspectorOpening.ContainsKey(type)) {
				AngeliaInspectorOpening.Add(type, false);
			}
			return AngeliaInspectorOpening[type];
		}


		private void SetOpeningToPool (System.Type type, bool opening) {
			AngeliaInspectorOpening.SetOrAdd(type, opening);
		}


	}



}
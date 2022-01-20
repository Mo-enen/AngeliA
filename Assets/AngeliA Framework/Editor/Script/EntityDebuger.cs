using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;
using AngeliaFramework.Entities;
using AngeliaFramework.Physics;
using AngeliaFramework.Rendering;


namespace AngeliaFramework.Editor {
	public class EntityDebuger : EditorWindow {




		#region --- VAR ---

		// Const
		private const string WINDOW_TITLE = "Entity";
		private static readonly int PIXEL_CODE = "Pixel".ACode();
		private static readonly Color32[] COLLIDER_TINT = {
			new (255, 0, 0, 255),
			new (255, 255, 0, 255),
			new (0, 255, 0, 255),
			new (0, 255, 255, 255),
			new (0, 0, 255, 255),
			new (255, 0, 255, 255),
		};

		// Short
		private EntityLayer CurrentEntityLayer {
			get => (EntityLayer)EntityLayerIndex.Value;
			set => EntityLayerIndex.Value = (int)value;
		}
		private static GUIStyle CMDTextAreaStyle => _CMDTextAreaStyle ??= new GUIStyle(GUI.skin.textArea) {
			fontSize = 14,
			contentOffset = new Vector2(2, 4),
		};
		private static GUIStyle _CMDTextAreaStyle = null;
		private static GUIContent EIconContent => _EIconContent ??= EditorGUIUtility.IconContent("d_GameObject Icon");
		private static GUIContent _EIconContent = null;
		private static GUIContent GlobalIconContent => _GlobalIconContent ??= EditorGUIUtility.IconContent("d_ToolHandleGlobal@2x");
		private static GUIContent _GlobalIconContent = null;
		private static GUIContent ColIconContent => _ColIconContent ??= EditorGUIUtility.IconContent("d_Physics2DRaycaster Icon");
		private static GUIContent _ColIconContent = null;
		private static GUIContent CameraIconContent => _CameraIconContent ??= EditorGUIUtility.IconContent("d_SceneViewCamera");
		private static GUIContent _CameraIconContent = null;

		// Data
		private static Game Game = null;
		private static Entity[][] Entities = null;
		private static Entity SelectingEntity = null;
		private static EntityInspector SelectingInspector = null;
		private Vector2 MasterScrollPos = default;

		// Saving
		private static readonly EditorSavingInt EntityLayerIndex = new("EntityDebuger.EntityLayerIndex", 0);
		private static readonly EditorSavingString EntityInitContent = new("EntityDebuger.EntityInitContent", "");
		private static readonly EditorSavingBool ShowColliders = new("EntityDebuger.ShowColliders", false);


		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		private static void Init () {
			// State Change
			EditorApplication.playModeStateChanged += (mode) => {

				if (!HasOpenInstances<EntityDebuger>()) { return; }

				// Enter Edit
				if (mode == PlayModeStateChange.EnteredEditMode) {
					// Reload Game
					Game game = null;
					foreach (var guid in AssetDatabase.FindAssets("t:Game")) {
						game = AssetDatabase.LoadAssetAtPath<Game>(AssetDatabase.GUIDToAssetPath(guid));
						if (game != null) { break; }
					}
					if (game != null) {
						AssetDatabase.ForceReserializeAssets(
							new string[] { AssetDatabase.GetAssetPath(game) },
							ForceReserializeAssetsOptions.ReserializeAssets
						);
					}
					// Clear Cache
					Game = null;
					Entities = null;
				}

				// Enter Play
				if (mode == PlayModeStateChange.EnteredPlayMode) {
					// Reload Cache
					Game = TryGetGame();
					Entities = null;
					ClearSelectionInspector();
					// CMD
					PerformCMD(EntityInitContent.Value);
				}

			};
		}


		[RuntimeInitializeOnLoadMethod]
		private static void RuntimeInit () {
			CellRenderer.BeforeUpdate += DrawGizmos;
		}


		[MenuItem("Tools/Entity Debuger")]
		private static void OpenWindow () => GetOrCreateWindow();


		private void OnGUI () {
			if (EditorApplication.isPlaying) {
				// Play Mode
				OnGUI_Runtime();
			} else {
				// Edit Mode
				OnGUI_Edittime();
			}
			if (Event.current.type == EventType.MouseDown) {
				Selection.activeObject = null;
				GUI.FocusControl("");
				Repaint();
			}
		}


		private void OnGUI_Edittime () {
			// Buttons
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
				// Language Editor
				if (GUI.Button(Layout.Rect(24, 20), GlobalIconContent, EditorStyles.toolbarButton)) {
					var gPer = FindObjectOfType<GamePerformer>();
					if (gPer != null && gPer.Game != null) {
						LanguageEditor.OpenEditor(gPer.Game);
					}
				}
				Layout.Rect(0, 20);
			}
			// CMD Text
			var oldBC = GUI.backgroundColor;
			GUI.backgroundColor = Color.clear;
			EntityInitContent.Value = GUI.TextArea(
				Layout.Rect(0, 0),
				EntityInitContent.Value,
				CMDTextAreaStyle
			);
			GUI.backgroundColor = oldBC;
			EditorGUIUtility.AddCursorRect(Layout.LastRect(), MouseCursor.Text);
		}


		private void OnGUI_Runtime () {

			// Game
			if (Game == null) {
				Game = TryGetGame();
				if (Game == null) { return; }
			}

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
				CurrentEntityLayer = (EntityLayer)Mathf.Clamp(
					(int)(EntityLayer)EditorGUI.EnumPopup(Layout.Rect(0, 20), CurrentEntityLayer, EditorStyles.toolbarPopup), 0, Entities.Length
				);
			}

			using var scope = new GUILayout.ScrollViewScope(MasterScrollPos);
			MasterScrollPos = scope.scrollPosition;
			var entities = Entities[(int)CurrentEntityLayer];
			int capacity = entities.Length;
			const int HEIGHT = 18;

			if (SelectingEntity != null && !SelectingEntity.Active) {
				ClearSelectionInspector();
			}

			// Table
			if (capacity > 0) {

				bool mouseLeftDown = Event.current.type == EventType.MouseDown && Event.current.button == 0;
				bool mouseRightDown = Event.current.type == EventType.MouseDown && Event.current.button == 1;
				bool mouseDown = mouseLeftDown || mouseRightDown;

				// List
				for (int i = 0; i < capacity; i++) {
					var entity = entities[i];
					if (entity == null) { continue; }

					var rect = Layout.Rect(0, HEIGHT);

					GUI.Label(rect, GUIContent.none, EditorStyles.toolbarButton);

					// BG
					if (entity == SelectingEntity && entity != null) {
						EditorGUI.DrawRect(rect, new Color32(44, 93, 135, 255));
					}

					// Icon
					GUI.Label(rect.Shrink(4, 0, 2, 2), EIconContent, EditorStyles.miniLabel);

					// Mouse Down
					if (mouseDown && rect.Contains(Event.current.mousePosition)) {
						if (mouseLeftDown) {
							SetSelectionInspector(entity, EntityInspector.Mode.Entity);
						}
						if (mouseRightDown) {
							EntityMenu(entity);
						}
						Event.current.Use();
						Repaint();
					}

					// Type
					GUI.Label(rect.Shrink(20, 0, 0, 0), entity.GetType().Name);

				}

			} else {
				// No Entity Capacity
				EditorGUILayout.HelpBox("Not Available for This Layer", MessageType.Info, true);
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


		public static void DrawGizmos () {
			// Colliders
			if (ShowColliders.Value) {
				CellPhysics.Editor_ForAllCells((layer, info) => {
					var tint = COLLIDER_TINT[layer];
					tint.a = (byte)(info.IsTrigger ? 128 : 255);
					CellRenderer.Draw(PIXEL_CODE, info.Rect, tint);
				});
			}
			// Selection
			if (SelectingEntity != null) {
				CellRenderer.Draw(PIXEL_CODE, new RectInt(
					SelectingEntity.X - 24,
					SelectingEntity.Y - 24,
					48, 48
				), new Color32(0, 0, 0, 255));
				CellRenderer.Draw(PIXEL_CODE, new RectInt(
					SelectingEntity.X - 16,
					SelectingEntity.Y - 16,
					32, 32
				), new Color32(255, 255, 255, (byte)(Time.time % 0.618f > 0.618f / 2f ? 255 : 128)));
			}
		}


		#endregion




		#region --- LGC ---


		private static Game TryGetGame () {
			Game result = null;
			foreach (var guid in AssetDatabase.FindAssets("t:Game")) {
				result = AssetDatabase.LoadAssetAtPath<Game>(AssetDatabase.GUIDToAssetPath(guid));
				if (result != null) { break; }
			}
			return result;
		}


		private void EntityMenu (Entity entity) {
			var menu = new GenericMenu();
			// Create
			var typePool = Util.GetFieldValue(Game, "EntityTypePool") as Dictionary<int, System.Type>;
			foreach (var pair in typePool) {
				var type = pair.Value;
				if (type.GetConstructor(new System.Type[0]) != null) {
					// Normal
					menu.AddItem(
						new GUIContent($"Create/{type.Name}"),
						false,
						() => {
							var spawnRect = (RectInt)Util.GetFieldValue(Game, "SpawnRect");
							var e = System.Activator.CreateInstance(type) as Entity;
							Game.AddEntity(e, CurrentEntityLayer);
							e.X = spawnRect.center.x.RoundToInt();
							e.Y = spawnRect.center.y.RoundToInt();
						}
					);
				} else {
					// No Constructor
					menu.AddDisabledItem(new GUIContent(type.Name), false);
				}
			}
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


		private static EntityDebuger GetOrCreateWindow () {
			try {
				var window = GetWindow<EntityDebuger>(WINDOW_TITLE, false);
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


		private static void SetSelectionInspector (Entity entity, EntityInspector.Mode mode) {
			SelectingEntity = entity;
			SelectingInspector.SetTarget(entity);
			SelectingInspector.InspectorMode = mode;
			Selection.activeObject = SelectingInspector;
		}


		private static void ClearSelectionInspector () {
			SelectingEntity = null;
			Selection.activeObject = null;
			SelectingInspector.SetTarget(null);
			SelectingInspector.InspectorMode = EntityInspector.Mode.Entity;
		}


		private static void PerformCMD (string cmd) {
			if (!string.IsNullOrEmpty(cmd)) {
				if (Game == null) {
					Game = TryGetGame();
					if (Game == null) { return; }
				}
				var lines = cmd.Replace("\r", "").Split('\n');
				Entity prevEntity = null;
				var typePool = Util.GetFieldValue(Game, "EntityTypePool") as Dictionary<int, System.Type>;
				foreach (var line in lines) {
					if (line.StartsWith("//")) { continue; }
					if (line.StartsWith("#")) {
						if (line.StartsWith("#lowframerate", System.StringComparison.OrdinalIgnoreCase)) {
							Game.SetFramerate(false);
						} else if (line.StartsWith("#highframerate", System.StringComparison.OrdinalIgnoreCase)) {
							Game.SetFramerate(true);
						}
						continue;
					}
					if (line.StartsWith("::")) {
						if (prevEntity != null) {
							int _eIndex = line.IndexOf('=');
							if (_eIndex < 0) { continue; }
							string fieldName = line[2.._eIndex];
							var type = Util.GetFieldType(prevEntity, fieldName);
							if (type == typeof(int)) {
								if (int.TryParse(line[(_eIndex + 1)..], out int _value)) {
									Util.SetFieldValue(prevEntity, fieldName, _value);
								}
							} else if (type == typeof(float)) {
								if (float.TryParse(line[(_eIndex + 1)..], out float _value)) {
									Util.SetFieldValue(prevEntity, fieldName, _value);
								}
							} else if (type == typeof(string)) {
								Util.SetFieldValue(prevEntity, fieldName, line[(_eIndex + 1)..]);
							} else if (type == typeof(bool)) {
								if (bool.TryParse(line[(_eIndex + 1)..], out bool _value)) {
									Util.SetFieldValue(prevEntity, fieldName, _value);
								}
							} else {
								Debug.LogWarning($"[Entity Debuger] type {type} not support/");
							}
						}
						continue;
					}
					var _params = line.Replace(" ", "").Split(',');
					if (
						_params != null && _params.Length >= 2 &&
						System.Enum.TryParse(_params[1], true, out EntityLayer layer)
					) {
						// X
						int x = 0;
						if (_params.Length >= 3) {
							int.TryParse(_params[2], out x);
						}
						// Y
						int y = 0;
						if (_params.Length >= 4) {
							int.TryParse(_params[3], out y);
						}
						// Final
						var type = typePool.SingleOrDefault((pair) => pair.Value.Name == _params[0]).Value;
						if (type != null) {
							var e = System.Activator.CreateInstance(type) as Entity;
							Game.AddEntity(e, layer);
							e.X = x;
							e.Y = y;
							prevEntity = e;
						}
					}
				}
			}

		}


		#endregion




	}



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


		private Dictionary<System.Type, FieldInfo[]> EntityFieldMap = new();


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


		private void GUI_View () {
			var game = (target as EntityInspector).Game;
			var viewRect = (RectInt)Util.GetFieldValue(game, "ViewRect");
			var spawnRect = (RectInt)Util.GetFieldValue(game, "SpawnRect");
			var cameraRect = CellRenderer.CameraRect;

			var newView = EditorGUILayout.RectIntField(new GUIContent("View"), viewRect);
			if (newView.IsNotSame(viewRect)) {
				Util.SetFieldValue(game, "ViewRect", newView);
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
			var type = eTarget.Target.GetType();
			var fields = EntityFieldMap.ContainsKey(type) ?
				EntityFieldMap[type] :
				EntityFieldMap[type] = type.GetFields(
					BindingFlags.Public | BindingFlags.NonPublic |
					BindingFlags.Instance | BindingFlags.DeclaredOnly
				);
			Layout.Space(4);

			// X
			eTarget.Target.X = EditorGUI.IntField(Layout.Rect(0, HEIGHT), "X", eTarget.Target.X);
			Layout.Space(2);

			// Y
			eTarget.Target.Y = EditorGUI.IntField(Layout.Rect(0, HEIGHT), "Y", eTarget.Target.Y);
			Layout.Space(2);

			// Fields
			foreach (var field in fields) {
				if (!field.IsPublic && field.GetCustomAttribute<SerializeField>(false) == null) { continue; }
				using (new GUILayout.HorizontalScope()) {
					field.SetValue(eTarget.Target, Field(
						Layout.Rect(0, HEIGHT),
						Util.GetDisplayName(field.Name),
						field.GetValue(eTarget.Target),
						field.FieldType
					));
				}
				Layout.Space(2);
			}
		}


		private static object Field (Rect rect, string label, object value, System.Type type) {
			var _type = type;
			if (_type.IsSubclassOf(typeof(Object))) {
				_type = typeof(Object);
			}
			if (_type.IsSubclassOf(typeof(System.Enum))) {
				_type = typeof(System.Enum);
			}
			return true switch {

				bool when _type == typeof(sbyte) => (sbyte)Mathf.Clamp(EditorGUI.IntField(rect, label, (sbyte)value), sbyte.MinValue, sbyte.MaxValue),
				bool when _type == typeof(byte) => (byte)Mathf.Clamp(EditorGUI.IntField(rect, label, (byte)value), byte.MinValue, byte.MaxValue),
				bool when _type == typeof(ushort) => (ushort)Mathf.Clamp(EditorGUI.IntField(rect, label, (ushort)value), ushort.MinValue, ushort.MaxValue),
				bool when _type == typeof(short) => (short)Mathf.Clamp(EditorGUI.IntField(rect, label, (short)value), short.MinValue, short.MaxValue),
				bool when _type == typeof(int) => EditorGUI.IntField(rect, label, (int)value),
				bool when _type == typeof(long) => EditorGUI.LongField(rect, label, (long)value),
				bool when _type == typeof(ulong) => (ulong)EditorGUI.LongField(rect, label, (long)(ulong)value),
				bool when _type == typeof(float) => EditorGUI.FloatField(rect, label, (float)value),
				bool when _type == typeof(double) => (double)EditorGUI.DoubleField(rect, label, (double)value),
				bool when _type == typeof(string) => EditorGUI.DelayedTextField(rect, label, (string)value),
				bool when _type == typeof(bool) => EditorGUI.Toggle(rect, label, (bool)value),

				bool when _type == typeof(System.Enum) => EditorGUI.EnumPopup(rect, label, (System.Enum)value),

				bool when _type == typeof(Vector2) => EditorGUI.Vector2Field(rect, label, (Vector2)value),
				bool when _type == typeof(Vector3) => EditorGUI.Vector3Field(rect, label, (Vector3)value),
				bool when _type == typeof(Vector4) => EditorGUI.Vector4Field(rect, label, (Vector4)value),
				bool when _type == typeof(Vector2Int) => EditorGUI.Vector2IntField(rect, label, (Vector2Int)value),
				bool when _type == typeof(Vector3Int) => EditorGUI.Vector3IntField(rect, label, (Vector3Int)value),
				bool when _type == typeof(Color32) => (Color32)EditorGUI.ColorField(rect, new GUIContent(label), (Color32)value, false, true, false),
				bool when _type == typeof(Color) => EditorGUI.ColorField(rect, new GUIContent(label), (Color)value, false, true, false),

				bool when _type == typeof(Object) => EditorGUI.ObjectField(rect, new GUIContent(label), (Object)value, type, false),

				_ => null,
			};
		}


	}



}
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
		private static readonly AlignmentInt PixelFrame = new(
			"Pixel".ACode(),
			"Pixel".ACode(),
			"Pixel".ACode(),
			"Pixel".ACode(),
			0,
			"Pixel".ACode(),
			"Pixel".ACode(),
			"Pixel".ACode(),
			"Pixel".ACode()
		);

		// Short
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
		private int EntityDirtyFlag = 0;

		// Saving
		private static readonly EditorSavingString EntityInitContent = new("EntityDebuger.EntityInitContent", "");
		private static readonly EditorSavingBool ShowColliders = new("EntityDebuger.ShowColliders", false);
		private static readonly EditorSavingString EntityLayerVisible = new("EntityDebuger.EntityLayerVisible", "");


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
					Game game = TryGetGame();
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


		[MenuItem("AngeliA/Entity Debuger")]
		private static void OpenWindow () => GetOrCreateWindow();


		private void OnGUI () {
			if (EditorApplication.isPlaying) {
				// Play Mode
				OnGUI_Runtime();
			} else {
				// Edit Mode
				OnGUI_Edittime();
			}
			Layout.CancelFocusOnClick(this, true);
		}


		private void OnGUI_Edittime () {

			// Toolbar
			using (new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
				// Language Editor
				if (GUI.Button(Layout.Rect(24, 20), GlobalIconContent, EditorStyles.toolbarButton)) {
					var game = TryGetGame();
					if (game != null) {
						LanguageEditor.OpenEditor(game);
					}
				}
				Layout.Rect(0, 20);
			}

			using var scope = new GUILayout.ScrollViewScope(MasterScrollPos);
			MasterScrollPos = scope.scrollPosition;

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
						EditorGUI.DrawRect(rect.Shrink(3, 3, rect.height - 2, 1), Layout.HighlightColor_Alt);
					}
					if (newVisible != visible) {
						SetLayerVisible(i, newVisible);
					}
				}
			}

			// Content
			using var scope = new GUILayout.ScrollViewScope(MasterScrollPos);
			MasterScrollPos = scope.scrollPosition;
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
						EditorGUI.DrawRect(rect, Layout.HighlightColor);
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

			}
		}


		public void Update () {
			if (EditorApplication.isPlaying) {
				if (Game != null && Game.EntityDirtyFlag != EntityDirtyFlag) {
					EntityDirtyFlag = Game.EntityDirtyFlag;
					Repaint();
				}
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
				byte alpha = (byte)(Time.time % 0.618f > 0.618f / 2f ? 255 : 128);
				CellGUI.Draw_9Slice(
					SelectingEntity.Rect, new(6, 6, 6, 6), new Color32(255, 255, 255, alpha), PixelFrame
				);
				CellGUI.Draw_9Slice(
					SelectingEntity.Rect.Shrink(6), new(6, 6, 6, 6), new Color32(0, 0, 0, alpha), PixelFrame
				);
			}
		}


		#endregion




		#region --- LGC ---


		private static Game TryGetGame () {
			Game result = null;
			foreach (var guid in AssetDatabase.FindAssets("t:Game")) {
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var game = AssetDatabase.LoadAssetAtPath<Game>(path);
				if (game != null) {
					result = game;
					break;
				}
			}
			return result;
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
			if (SelectingInspector != null) {
				SelectingInspector.SetTarget(entity);
				SelectingInspector.InspectorMode = mode;
			}
			Selection.activeObject = SelectingInspector;
		}


		private static void ClearSelectionInspector () {
			SelectingEntity = null;
			Selection.activeObject = null;
			if (SelectingInspector != null) {
				SelectingInspector.SetTarget(null);
				SelectingInspector.InspectorMode = EntityInspector.Mode.Entity;
			}
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
							try {
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
								} else if (type.IsSubclassOf(typeof(System.Enum))) {
									if (System.Enum.TryParse(type, line[(_eIndex + 1)..], out var _value)) {
										Util.SetFieldValue(prevEntity, fieldName, _value);
									}
								} else {
									Debug.LogWarning($"[Entity Debuger] type {type} not support/");
								}
							} catch { }
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
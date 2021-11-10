#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;



namespace AngeliaFramework.Editor {


	public class DebugEntity : Entity {


		public PhysicsLayer Layer = PhysicsLayer.Item;
		public int Width = Const.CELL_SIZE;
		public int Height = Const.CELL_SIZE;
		public Color32 Color = new(255, 255, 255, 255);
		public string SpriteName = "Pixel";
		public bool PhysicsCheck = false;


		public override void FillPhysics () => CellPhysics.Fill(
			Layer, new RectInt(X, Y, Width, Height), this
		);


		public override void FrameUpdate () {
			CellRenderer.Draw(
				GetSpriteGlobalID(SpriteName),
				X, Y, 0, 0,
				0, Width, Height, Color
			);
			if (PhysicsCheck) {
				CellPhysics.ForAllOverlaps(Layer, new RectInt(X, Y, Width, Height), (_rect, _entity) => {
					if (_entity != this && _entity is DebugEntity dEntity) {
						CellRenderer.Draw(
							GetSpriteGlobalID("Pixel"),
							dEntity.X, dEntity.Y, 0, 0,
							0, dEntity.Width, dEntity.Height, new Color(0, 1, 0, 1f)
						);
					}
					return true;
				});
			}
		}


		public bool LoadFromString (string str) {
			var lines = str.Split(',');
			if (lines != null && lines.Length > 1) {
				int len = lines.Length;

				if (len > 0 && int.TryParse(lines[0], out int x)) { X = x; }
				if (len > 1 && int.TryParse(lines[1], out int y)) { Y = y; }
				if (len > 2 && int.TryParse(lines[2], out int w)) { Width = w; }
				if (len > 3 && int.TryParse(lines[3], out int h)) { Height = h; }

				if (len > 4 && byte.TryParse(lines[4], out byte cr)) { Color.r = cr; }
				if (len > 5 && byte.TryParse(lines[5], out byte cg)) { Color.g = cg; }
				if (len > 6 && byte.TryParse(lines[6], out byte cb)) { Color.b = cb; }
				if (len > 7 && byte.TryParse(lines[7], out byte ca)) { Color.a = ca; }

				if (len > 8 && int.TryParse(lines[8], out int l)) { Layer = (PhysicsLayer)l; }
				if (len > 9) { SpriteName = lines[9]; }
				if (len > 10) { PhysicsCheck = lines[10] == "1"; }

				return true;
			}
			return false;
		}


		public string SaveToString () =>
			$"{X},{Y},{Width},{Height}," +
			$"{Color.r},{Color.g},{Color.b},{Color.a}," +
			$"{(int)Layer},{SpriteName},{(PhysicsCheck ? 1 : 0)}";


	}


	public class EntityDebuger : EditorWindow {




		#region --- VAR ---

		// Short
		private EntityLayer CurrentEntityLayer {
			get => (EntityLayer)EntityLayerIndex.Value;
			set => EntityLayerIndex.Value = (int)value;
		}
		private PhysicsLayer CurrentPhysicsLayer {
			get => (PhysicsLayer)PhysicsLayerIndex.Value;
			set => PhysicsLayerIndex.Value = (int)value;
		}
		private static GUIStyle ScrollStyle => _ScrollStyle ??= new GUIStyle() {
			padding = new RectOffset(6, 6, 2, 2),
		};
		private static GUIStyle TextArea => _TextArea ??= new GUIStyle(GUI.skin.textArea) {
			fontSize = 16,
			contentOffset = new Vector2(2, 4),
		};

		// Data
		private static GUIStyle _ScrollStyle = null;
		private static GUIStyle _TextArea = null;
		private Game Game = null;
		private Entity[][] Entities = null;
		private Entity FocusingEntity = null;
		private List<System.Type> EntityTypes = new();
		private Dictionary<System.Type, FieldInfo[]> EntityFieldMap = new();
		private Vector2 MasterScrollPos = default;
		private int PageIndex = 0;

		// Saving
		private static EditorSavingInt EntityLayerIndex = new("EntityDebuger.EntityLayerIndex", 0);
		private static EditorSavingInt PhysicsLayerIndex = new("EntityDebuger.PhysicsLayerIndex", 0);
		private static EditorSavingString EntityInitContent = new("EntityDebuger.EntityInitContent", "");
		private static EditorSavingString DebugEntities = new("EntityDebuger.DebugEntities", "");


		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		private static void Init () {

			EditorApplication.playModeStateChanged += (mode) => {

				if (!HasOpenInstances<EntityDebuger>()) { return; }
				var window = GetOrCreateWindow();

				// Reload Cache
				if (mode == PlayModeStateChange.EnteredEditMode || mode == PlayModeStateChange.EnteredPlayMode) {
					window.Game = null;
					window.Entities = null;
					window.EntityTypes.Clear();
					window.EntityFieldMap.Clear();
					window.FocusingEntity = null;
					window.InitCaches();
				}

				// Load Entity Init Content
				if (mode == PlayModeStateChange.EnteredPlayMode) {
					if (!string.IsNullOrEmpty(EntityInitContent.Value)) {
						var lines = EntityInitContent.Value.Replace("\r", "").Replace(" ", "").Split('\n');
						foreach (var line in lines) {
							if (line.StartsWith("//")) { continue; }
							var _params = line.Split(',');
							if (
								_params != null && _params.Length >= 3 &&
								System.Enum.TryParse(_params[1], true, out EntityLayer layer) &&
								int.TryParse(_params[2], out int count)
							) {
								var type = window.EntityTypes.Single(
									(t) => t.Name == _params[0]
								);
								if (type != null) {
									for (int i = 0; i < count; i++) {
										Util.InvokeMethod(window.Game, "CreateEntity", type, layer);
									}
								}
							}
						}
					}
				}

				// Debug Entities
				if (mode == PlayModeStateChange.EnteredPlayMode) {
					window.LoadDebugEntities();
				}

				if (mode == PlayModeStateChange.ExitingPlayMode) {
					window.SaveDebugEntities();
				}

			};
		}


		[MenuItem("Tools/Entity Debuger")]
		private static void OpenWindow () => GetOrCreateWindow();


		private void OnGUI () {
			if (!EditorApplication.isPlaying) {
				// Edit Mode
				OnGUI_EntityInit();
			} else {
				// Play Mode
				if ((Game == null || Entities == null) && !InitCaches()) { return; }

				// Content
				using var scope = new GUILayout.ScrollViewScope(MasterScrollPos, ScrollStyle);
				Layout.Space(12);
				MasterScrollPos = scope.scrollPosition;
				OnGUI_EntityView();

			}
			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl("");
				Repaint();
			}
		}


		private void OnGUI_EntityView () {

			Layout.Space(6);

			// Toolbar
			using (new GUILayout.HorizontalScope()) {
				Layout.Space(4);
				// New Entity
				if (GUI.Button(Layout.Rect(72, 18), "+ Entity", EditorStyles.popup)) {
					CreateEntityMenu();
				}
				Layout.Space(4);
				// Layer
				CurrentEntityLayer = (EntityLayer)Mathf.Clamp(
					(int)(EntityLayer)EditorGUI.EnumPopup(Layout.Rect(0, 18), CurrentEntityLayer), 0, Entities.Length
				);
			}
			Layout.Space(8);

			var entities = Entities[(int)CurrentEntityLayer];
			int capacity = entities.Length;
			const int HEIGHT = 18;

			if (FocusingEntity != null && !FocusingEntity.Active) {
				FocusingEntity = null;
			}

			// Table
			if (capacity > 0) {

				bool mouseDown = Event.current.type == EventType.MouseDown;

				// Title Bar
				using (new GUILayout.HorizontalScope()) {
					GUI.Label(Layout.Rect(24, HEIGHT), "#", Layout.MiniGreyLabel);
					GUI.Label(Layout.Rect(0, HEIGHT), "type", Layout.MiniGreyLabel);
					GUI.Label(Layout.Rect(0, HEIGHT), "x", Layout.MiniGreyLabel);
					Layout.Space(4);
					GUI.Label(Layout.Rect(0, HEIGHT), "y", Layout.MiniGreyLabel);
					Layout.Space(4);
					GUI.Label(Layout.Rect(64, HEIGHT), "rot", Layout.MiniGreyLabel);
					Layout.Space(4);
					Layout.Space(24);
				}

				// List
				PageIndex = Layout.PageList(PageIndex, 16, capacity, (index, rect) => {

					var entity = entities[index];

					// BG
					if (mouseDown && rect.Contains(Event.current.mousePosition)) {
						FocusingEntity = entity;
						Repaint();
					}

					// Highlight
					if (entity == FocusingEntity && entity != null) {
						EditorGUI.DrawRect(rect.Expand(-24, 0, 0, 0), new Color32(44, 93, 135, 255));
					}

					// Entity
					if (entity != null) {

						// Type
						var _rect = Layout.Rect(0, HEIGHT);
						var oldC = GUI.color;
						GUI.color = new Color(0.4f, 1f, 0.9f, 1f);
						GUI.Label(_rect, entity.GetType().Name);
						GUI.color = oldC;

						float snap = Const.CELL_SIZE / 4;

						// X
						_rect = Layout.Rect(0, HEIGHT);
						entity.X = EditorGUI.IntField(_rect.Shrink(18, 18, 0, 0), entity.X);
						if (GUI.Button(new Rect(_rect) { width = 18, }, "◀")) {
							entity.X = (int)(Mathf.Round((entity.X - snap) / snap) * snap);
						}
						if (GUI.Button(new Rect(_rect) { x = _rect.xMax - 18, width = 18, }, "▶")) {
							entity.X = (int)(Mathf.Round((entity.X + snap) / snap) * snap);
						}
						Layout.Space(4);

						// Y
						_rect = Layout.Rect(0, HEIGHT);
						entity.Y = EditorGUI.IntField(_rect.Shrink(18, 18, 0, 0), entity.Y);
						if (GUI.Button(new Rect(_rect) { width = 18, }, "◀")) {
							entity.Y = (int)(Mathf.Round((entity.Y - snap) / snap) * snap);
						}
						if (GUI.Button(new Rect(_rect) { x = _rect.xMax - 18, width = 18, }, "▶")) {
							entity.Y = (int)(Mathf.Round((entity.Y + snap) / snap) * snap);
						}
						Layout.Space(4);

						// Destroy
						if (GUI.Button(Layout.Rect(24, HEIGHT), "×")) {
							entity.Destroy();
						}

					}
				});



				// Focusing
				using (new GUILayout.VerticalScope(GUI.skin.box)) {
					if (FocusingEntity != null) {

						// Type
						var type = FocusingEntity.GetType();
						Layout.Space(4);
						var oldC = GUI.color;
						GUI.color = new Color(0.4f, 1f, 0.9f, 1f);
						GUI.Label(Layout.Rect(0, 18), Util.GetDisplayName(type.Name));
						GUI.color = oldC;
						Layout.Space(2);

						// Fields
						var fields = EntityFieldMap.ContainsKey(type) ?
							EntityFieldMap[type] :
							EntityFieldMap[type] = type.GetFields(
								BindingFlags.Public | BindingFlags.NonPublic |
								BindingFlags.Instance | BindingFlags.DeclaredOnly
							);
						foreach (var field in fields) {
							if (!field.IsPublic && field.GetCustomAttribute<SerializeField>(false) == null) { continue; }
							using (new GUILayout.HorizontalScope()) {
								// Name (type)
								GUI.Label(
									Layout.Rect(0, HEIGHT),
									$"{Util.GetDisplayName(field.Name)} <color=#666666>{Util.GetDisplayNameForTypes(field.FieldType.Name)}</color>",
									Layout.RichLabel
								);
								// Value
								field.SetValue(FocusingEntity, Field(
									Layout.Rect(0, HEIGHT),
									"",
									field.GetValue(FocusingEntity)
								));
							}
							Layout.Space(2);
						}

					} else {
						Layout.Rect(0, 1);
					}
				}

			} else {
				// No Entity Capacity
				EditorGUILayout.HelpBox("Not Available for This Layer", MessageType.Info, true);
			}
		}


		private void OnGUI_EntityInit () {
			Layout.Space(6);

			GUI.Label(Layout.Rect(0, 18).Expand(-6, 0, 0, 0), "Create Entity");
			Layout.Space(6);
			EditorGUI.HelpBox(
				Layout.Rect(0, 24).Expand(-16, -6, 0, 0),
				" Type, Layer, Count",
				MessageType.Info
			);
			Layout.Space(6);
			EntityInitContent.Value = GUI.TextArea(
				Layout.Rect(0, 320).Expand(-16, -6, 0, 0),
				EntityInitContent.Value,
				TextArea
			);
			EditorGUIUtility.AddCursorRect(Layout.LastRect(), MouseCursor.Text);
			Layout.Space(22);





		}


		#endregion




		#region --- LGC ---


		private bool InitCaches () {

			// Game
			Game = null;
			foreach (var guid in AssetDatabase.FindAssets("t:Game")) {
				Game = AssetDatabase.LoadAssetAtPath<Game>(AssetDatabase.GUIDToAssetPath(guid));
				if (Game != null) { break; }
			}
			if (Game == null) { return false; }

			// Entities
			Entities = Util.GetFieldValue(Game, "Entities") as Entity[][];
			if (Entities == null) {
				Game.Init();
			}
			Entities = Util.GetFieldValue(Game, "Entities") as Entity[][];

			// EntityTypes
			var typePool = Util.GetFieldValue(Game, "EntityTypePool") as Dictionary<ushort, System.Type>;
			EntityTypes.Clear();
			EntityTypes.AddRange(typePool.Values);

			return Entities != null;
		}


		private void CreateEntityMenu () {
			var menu = new GenericMenu();
			foreach (var type in EntityTypes) {
				menu.AddItem(new GUIContent(type.Name), false, () => Util.InvokeMethod(
					Game, "CreateEntity", type, CurrentEntityLayer
				));
			}
			menu.ShowAsContext();
		}


		private void SaveDebugEntities () {
			if (Entities == null) { return; }
			var builder = new StringBuilder();
			for (int i = 0; i < Const.ENTITY_LAYER_COUNT; i++) {
				foreach (var e in Entities[i]) {
					if (e is DebugEntity d) {
						builder.Append(d.SaveToString());
						builder.Append('\n');
					}
				}
			}
			DebugEntities.Value = builder.ToString();
		}


		private void LoadDebugEntities () {
			if (Entities == null) { return; }
			var lines = DebugEntities.Value.Split('\n');
			if (lines != null) {
				foreach (var line in lines) {
					var entity = Util.InvokeMethod(
						Game, "CreateEntity", typeof(DebugEntity), EntityLayer.Item
					) as DebugEntity;
					entity.Active = entity.LoadFromString(line);
				}
			}
		}


		private static EntityDebuger GetOrCreateWindow () {
			try {
				var inspector = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
				var window = inspector != null ?
					GetWindow<EntityDebuger>("Entity Debuger", false, inspector) :
					GetWindow<EntityDebuger>("Entity Debuger", false);
				window.minSize = new Vector2(275, 400);
				window.maxSize = new Vector2(600, 1000);
				window.titleContent = EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow");
				window.titleContent.text = "Entity Debuger";
				return window;
			} catch (System.Exception ex) {
				Debug.LogWarning("Failed to open window.\n" + ex.Message);
			}
			return null;
		}


		private static object Field (Rect rect, string label, object value) => value switch {

			sbyte sbValue => (sbyte)Mathf.Clamp(EditorGUI.IntField(rect, label, sbValue), sbyte.MinValue, sbyte.MaxValue),
			byte bValue => (byte)Mathf.Clamp(EditorGUI.IntField(rect, label, bValue), byte.MinValue, byte.MaxValue),
			ushort usValue => (ushort)Mathf.Clamp(EditorGUI.IntField(rect, label, usValue), ushort.MinValue, ushort.MaxValue),
			short sValue => (short)Mathf.Clamp(EditorGUI.IntField(rect, label, sValue), short.MinValue, short.MaxValue),
			int iValue => EditorGUI.IntField(rect, label, iValue),
			long lValue => EditorGUI.LongField(rect, label, lValue),
			ulong ulValue => (ulong)EditorGUI.LongField(rect, label, (long)ulValue),
			float fValue => EditorGUI.FloatField(rect, label, fValue),
			double dValue => (double)EditorGUI.DoubleField(rect, label, dValue),
			string sValue => EditorGUI.DelayedTextField(rect, label, sValue),
			bool boolValue => EditorGUI.Toggle(rect, label, boolValue),

			System.Enum eValue => EditorGUI.EnumPopup(rect, label, eValue),

			Vector2 v2Value => EditorGUI.Vector2Field(rect, label, v2Value),
			Vector3 v3Value => EditorGUI.Vector3Field(rect, label, v3Value),
			Vector4 v4Value => EditorGUI.Vector4Field(rect, label, v4Value),
			Vector2Int v2iValue => EditorGUI.Vector2IntField(rect, label, v2iValue),
			Vector3Int v3iValue => EditorGUI.Vector3IntField(rect, label, v3iValue),
			Color32 c32Value => (Color32)EditorGUI.ColorField(rect, new GUIContent(label), c32Value, false, true, false),
			Color cValue => EditorGUI.ColorField(rect, new GUIContent(label), cValue, false, true, false),

			_ => null,
		};


		#endregion




	}
}
#endif
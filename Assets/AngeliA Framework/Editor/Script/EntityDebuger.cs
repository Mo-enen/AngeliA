#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;


namespace AngeliaFramework.Editor {
	public class CellPhysicsDebuger : EditorWindow {




		#region --- VAR ---


		// Const
		private const int PAGE_SIZE = 32;

		// Short
		private Layer CurrentLayer {
			get => (Layer)LayerIndex.Value;
			set => LayerIndex.Value = (int)value;
		}
		private static GUIStyle ScrollStyle => _ScrollStyle ??= new GUIStyle() {
			padding = new RectOffset(6, 6, 2, 2),
		};

		// Data
		private static GUIStyle _ScrollStyle = null;
		private Game Game = null;
		private Entity[][] Entities = null;
		private readonly List<System.Type> EntityTypes = new List<System.Type>();
		private readonly Dictionary<System.Type, FieldInfo[]> EntityFieldMap = new Dictionary<System.Type, FieldInfo[]>();
		private Entity FocusingEntity = null;
		private Vector2 MasterScrollPos = default;
		private int PageIndex = 0;

		// Saving
		private static EditorSavingInt LayerIndex = new EditorSavingInt("CPD.LayerIndex", 0);


		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		private static void Init () {
			EditorApplication.playModeStateChanged += (mode) => {
				if (mode == PlayModeStateChange.EnteredEditMode || mode == PlayModeStateChange.EnteredPlayMode) {
					if (!HasOpenInstances<CellPhysicsDebuger>()) { return; }
					var window = GetOrCreateWindow();
					window.Game = null;
					window.Entities = null;
					window.EntityTypes.Clear();
					window.EntityFieldMap.Clear();
					window.FocusingEntity = null;
				}
			};
		}


		[MenuItem("Tools/Entity Debuger")]
		private static void OpenWindow () => GetOrCreateWindow();


		private void OnGUI () {

			if (!EditorApplication.isPlaying) { return; }
			if (CellRenderer.DebugLayer == null) { InitLayer(); }
			if (CellRenderer.DebugLayer == null) { return; }
			if ((Game == null || Entities == null) && !InitCaches()) { return; }

			using (var scope = new GUILayout.ScrollViewScope(MasterScrollPos, ScrollStyle)) {
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
				CurrentLayer = (Layer)Mathf.Clamp(
					(int)(Layer)EditorGUI.EnumPopup(Layout.Rect(0, 18), CurrentLayer), 0, Entities.Length
				);

			}
			Layout.Space(8);

			var entities = Entities[(int)CurrentLayer];
			int capacity = entities.Length;
			int pageCount = Mathf.CeilToInt((float)capacity / PAGE_SIZE);
			const int HEIGHT = 18;
			const int BUTTON_HEIGHT = 22;

			PageIndex = Mathf.Clamp(PageIndex, 0, pageCount - 1);

			// Table
			if (capacity > 0) {
				int from = Mathf.Clamp(PageIndex * PAGE_SIZE, 0, capacity - 1);
				int to = Mathf.Clamp((PageIndex + 1) * PAGE_SIZE, 0, capacity);
				var oldE = GUI.enabled;
				var oldC = GUI.color;
				float bgWidth = Layout.Rect(0, 1).width;
				bool mouseDown = Event.current.type == EventType.MouseDown;

				// Title Bar
				using (new GUILayout.HorizontalScope()) {
					GUI.Label(Layout.Rect(24, HEIGHT), "#", Layout.MiniGreyLabel);
					GUI.Label(Layout.Rect(0, HEIGHT), "type", Layout.MiniGreyLabel);
					GUI.Label(Layout.Rect(0, HEIGHT), "pos", Layout.MiniGreyLabel);
					GUI.Label(Layout.Rect(0, HEIGHT), "pivot", Layout.MiniGreyLabel);
					GUI.Label(Layout.Rect(0, HEIGHT), "rot", Layout.MiniGreyLabel);

				}

				// Content
				for (int i = 0; i < PAGE_SIZE; i++) {
					int index = i + from;
					if (index >= from && index < to) {
						using (new GUILayout.HorizontalScope()) {

							var entity = entities[index];
							GUI.enabled = entity != null;
							var _rect = Layout.Rect(24, HEIGHT);

							// BG
							var bgRect = new Rect(_rect.x, _rect.y, bgWidth, HEIGHT);
							EditorGUI.DrawRect(
								bgRect,
								FocusingEntity != null && FocusingEntity == entity ? (Color)new Color32(44, 93, 135, 255) :
								index % 2 == 0 ? Color.clear : new Color(0f, 0f, 0f, 0.1f)
							);

							if (mouseDown && bgRect.Contains(Event.current.mousePosition)) {
								FocusingEntity = entity;
								Repaint();
							}

							// Index
							GUI.color = oldC;
							GUI.Label(_rect, index.ToString("00"));
							Layout.Space(2);

							// Entity
							if (entity != null) {

								// Type
								GUI.color = new Color(0.4f, 1f, 0.9f, 1f);
								GUI.Label(Layout.Rect(0, HEIGHT), entity.GetType().Name);

								// Pos
								GUI.color = oldC;
								GUI.Label(Layout.Rect(0, HEIGHT), $"{entity.X}, {entity.Y}");

								// Pivot
								GUI.color = oldC;
								GUI.Label(Layout.Rect(0, HEIGHT), $"{entity.PivotX}, {entity.PivotY}");

								// Rot
								GUI.color = oldC;
								GUI.Label(Layout.Rect(0, HEIGHT), entity.Rotation.ToString());

							}
						}
					} else {
						// Out of Range
						Layout.Space(HEIGHT);
					}
				}
				GUI.enabled = oldE;
				GUI.color = oldC;
				Layout.Space(4);

				// Page Switch
				using (new GUILayout.HorizontalScope()) {
					Layout.Rect(0, BUTTON_HEIGHT);
					if (GUI.Button(Layout.Rect(42, BUTTON_HEIGHT), "◀")) {
						PageIndex = Mathf.Clamp(PageIndex - 1, 0, pageCount - 1);
					}
					GUI.Label(
						Layout.Rect(36, BUTTON_HEIGHT),
						$"{PageIndex + 1}/{pageCount}",
						Layout.CenteredLabel
					);
					if (GUI.Button(Layout.Rect(42, BUTTON_HEIGHT), "▶")) {
						PageIndex = Mathf.Clamp(PageIndex + 1, 0, pageCount - 1);
					}
					Layout.Space(8);
				}

				// Focusing
				using (new GUILayout.VerticalScope(GUI.skin.box, GUILayout.Height(128))) {
					if (FocusingEntity != null) {

						// Type
						var type = FocusingEntity.GetType();
						Layout.Space(4);
						GUI.color = new Color(0.4f, 1f, 0.9f, 1f);
						GUI.Label(Layout.Rect(0, 18), type.Name);
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
									$"{field.Name} <color=#666666>{Util.GetDisplayNameForTypes(field.FieldType.Name)}</color>",
									Layout.RichLabel
								);
								// Value
								var value = field.GetValue(FocusingEntity);
								GUI.Label(
									Layout.Rect(0, HEIGHT),
									value switch {
										Vector2 _vec => _vec.ToString("0.00"),
										_ => value.ToString(),
									}
								);
							}
							Layout.Space(2);
						}

					} else {
						Layout.Space(1);
					}
				}

			} else {
				// No Entity Capacity
				EditorGUILayout.HelpBox("Not Available for This Layer", MessageType.Info, true);
			}
		}


		#endregion




		#region --- LGC ---


		private void InitLayer () {
			CellRenderer.DebugLayer = new CellRenderer.Layer() {
				Cells = new CellRenderer.Cell[1024],
				CellCount = 1024,
				Material = new Material(Shader.Find("Cell")) { mainTexture = Texture2D.whiteTexture },
				UVs = new Rect[1] { new Rect(0, 0, 1, 1) },
				UVCount = 1,
			};
			CellRenderer.DebugLayer.Cells[0].ID = -1;
		}


		private bool InitCaches () {

			EntityTypes.Clear();
			Game = null;
			foreach (var guid in AssetDatabase.FindAssets("t:Game")) {
				Game = AssetDatabase.LoadAssetAtPath<Game>(AssetDatabase.GUIDToAssetPath(guid));
				if (Game != null) { break; }
			}
			if (Game == null) { return false; }

			Entities = Util.GetField(Game, "Entities") as Entity[][];
			if (Entities == null) {
				Game.Init();
			}
			Entities = Util.GetField(Game, "Entities") as Entity[][];

			var typePool = Util.GetField(Game, "EntityTypePool") as Dictionary<ushort, System.Type>;
			EntityTypes.AddRange(typePool.Values);

			return Entities != null;
		}


		private void CreateEntityMenu () {
			var menu = new GenericMenu();
			foreach (var type in EntityTypes) {
				menu.AddItem(new GUIContent(type.Name), false, () => Util.InvokeMethod(
					Game, "CreateEntity", type, CurrentLayer
				));
			}
			menu.ShowAsContext();
		}


		private static CellPhysicsDebuger GetOrCreateWindow () {
			try {
				var inspector = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
				var window = inspector != null ?
					GetWindow<CellPhysicsDebuger>("Entity Debuger", true, inspector) :
					GetWindow<CellPhysicsDebuger>("Entity Debuger", true);
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


		#endregion




	}
}
#endif
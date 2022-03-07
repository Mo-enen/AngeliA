using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEditor;
using Moenen.Standard;
using LdtkToAngeliA;


namespace AngeliaFramework.Editor {
	public class AngeliaToolbox : EditorWindow, IHasCustomMenu, IInitialize {




		#region --- VAR ---


		// Const
		private const string WINDOW_TITLE = "Angelia";
		private const double ALRT_DURATION = 3d;
		private static readonly int PIXEL_CODE = "Pixel".AngeHash();
		private static readonly Color32[] COLLIDER_TINT = { new(255, 128, 0, 255), new(255, 255, 0, 255), new(0, 255, 0, 255), new(0, 255, 255, 255), new(0, 0, 255, 255), new(255, 0, 255, 255), new(255, 0, 0, 255), };

		// Short
		private static AngeliaToolbox Main = null;
		private static GUIContent EIconContent => _EIconContent ??= EditorGUIUtility.IconContent("d_GameObject Icon");
		private static GUIContent _EIconContent = null;
		private static Game Game => _Game != null ? _Game : (_Game = FindObjectOfType<Game>());
		private static Game _Game = null;

		// Data 
		private static Entity[][] Entities = null;
		private static int[] EntityLength = null;
		private static Entity SelectingEntity = null;
		private static Entity HoveringEntity = null;
		private static EntityInspector SelectingInspector = null;
		private static bool PrevUnityFocused = true;
		private static double RequireAlertTime = double.MinValue;
		private static string AlertMessage = "";
		private int EntityDirtyFlag = 0;
		private bool PrevUpdateMousePress = false;
		private Vector2 MasterScrollPos = default;

		// Saving
		private static readonly EditorSavingBool ShowColliders = new("EntityDebuger.ShowColliders", false);
		private static readonly EditorSavingString EntityLayerVisible = new("EntityDebuger.EntityLayerVisible", "");
		private static readonly EditorSavingString LastSyncTick = new("EntityDebuger.LastSyncTick", "0");
		private static readonly EditorSavingBool ClickToSelectEntity = new("EntityDebuger.ClickToSelectEntity", true);
	

		#endregion




		#region --- MSG ---


		[InitializeOnLoadMethod]
		private static void Initialize_Editor () {

			// State Change
			EditorApplication.playModeStateChanged += (mode) => {

				if (!HasOpenInstances<AngeliaToolbox>()) { return; }

				// Enter Edit
				if (mode == PlayModeStateChange.EnteredEditMode) {
					Entities = null;
					EntityLength = null;
				}

				// Enter Play
				if (mode == PlayModeStateChange.EnteredPlayMode) {
					// Reload Cache
					Entities = null;
					EntityLength = null;
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
						foreach (var file in Util.GetFilesIn(Application.dataPath, false, "*.ldtk")) {
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
								Main.SyncArtwork();
								Main.CheckSpriteNameDuplication();
							}
						}
					}
				}
			};

		}


		[MenuItem("AngeliA/Angelia Console")]
		private static void OpenWindow () {
			try {
				var window = GetWindow<AngeliaToolbox>(WINDOW_TITLE, false);
				window.minSize = new Vector2(275, 400);
				window.maxSize = new Vector2(600, 1000);
				window.titleContent = EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow");
				window.titleContent.text = WINDOW_TITLE;
			} catch (System.Exception ex) {
				Debug.LogWarning("Failed to open window.\n" + ex.Message);
			}
		}


		void IHasCustomMenu.AddItemsToMenu (GenericMenu menu) {
			// View
			if (EditorApplication.isPlaying) {
				menu.AddItem(new GUIContent("Select View"), false, () => {
					SetSelectionInspector(null, EntityInspector.Mode.View);
				});
			} else {
				menu.AddDisabledItem(new GUIContent("Select View"));
			}
			// Collider
			menu.AddItem(new GUIContent("Show Colliders"), ShowColliders.Value, () =>
				ShowColliders.Value = !ShowColliders.Value
			);
			// Framerate
			menu.AddItem(new GUIContent("High Framerate"), Game.HighFramerate, () => {
				Game.SetFramerate(!Game.HighFramerate);
			});
			// Click to Select Entity
			menu.AddItem(new GUIContent("Click to Select Entity"), ClickToSelectEntity.Value, () => {
				ClickToSelectEntity.Value = !ClickToSelectEntity.Value;
			});
		}


		public static void Initialize () => CellRenderer.BeforeUpdate += () => { if (Main != null) Main.DrawGizmos(); };


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
				if (ClickToSelectEntity.Value) {
					bool mousePressing = Input.GetMouseButton(0);
					if (mousePressing && !PrevUpdateMousePress && Entities != null) {
						var mousePos = FrameInput.MouseGlobalPosition;
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
						SetSelectionInspector(null, EntityInspector.Mode.Entity);
					LoopEnd:;
					}
					PrevUpdateMousePress = mousePressing;
				} else {
					PrevUpdateMousePress = false;
				}
			} else {
				PrevUpdateMousePress = false;
			}
			// Repaint when Alert
			if (EditorApplication.timeSinceStartup < RequireAlertTime + ALRT_DURATION + 1f) {
				Repaint();
			}
		}


		private void OnFocus () => Repaint();


		private void OnLostFocus () => Repaint();


		private void OnGUI_Runtime () {

			// Game
			if (Game == null) return;

			// Entities
			if (Entities == null) {
				Entities = Util.GetFieldValue(Game, "Entities") as Entity[][];
				if (Entities == null) return;
			}

			if (EntityLength == null) {
				EntityLength = Util.GetFieldValue(Game, "EntityLength") as int[];
				if (EntityLength == null) return;
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

					/// Toggle
					bool newVisible = GUI.Toggle(
						Layout.Rect(0, 20),
						visible,
						GUIContent.none,
						EditorStyles.toolbarButton
					);

					// Fill Amount
					int currentLen = EntityLength[i];
					int totalLen = Entities[i].Length;
					var fillRect = Layout.LastRect().Shrink(1);
					fillRect.width = fillRect.width * currentLen / totalLen;
					EditorGUI.DrawRect(fillRect, Color.Lerp(
						new Color(0.5f, 1f, 0.4f, 0.3f),
						new Color(1f, 0f, 0f, 0.3f),
						(float)currentLen / totalLen
					));

					// Label
					GUI.Label(Layout.LastRect(), label[..Mathf.Min(label.Length, 5)], Layout.CenteredMiniLabel);

					// Mark
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


		private void OnGUI_Edittime () {

			Layout.Space(12);

			using (new GUILayout.HorizontalScope()) {
				Layout.Space(12);
				// Language Editor
				if (GUI.Button(Layout.Rect(0, 48), "Language\nEditor")) {
					LanguageEditor.OpenEditor();
				}
				Layout.Space(6);
				// Dialogue Editor
				if (GUI.Button(Layout.Rect(0, 48), "Dialogue\nEditor")) {
					DialogueEditor.OpenEditor();
				}
				Layout.Space(12);
			}

			Layout.Space(6);

			using (new GUILayout.HorizontalScope()) {
				Layout.Space(12);
				// Sync Artwork
				if (GUI.Button(Layout.Rect(0, 48), "Sync\nArtwork")) {
					SyncArtwork();
					CheckSpriteNameDuplication();
				}
				Layout.Space(6);
				Layout.Rect(0, 48);
				Layout.Space(12);
			}

			Layout.Rect(0, 0);

			// Bottom Bar
			using (new GUILayout.HorizontalScope()) {

				double time = EditorApplication.timeSinceStartup;

				if (time < RequireAlertTime + ALRT_DURATION) {
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


		private void SetSelectionInspector (Entity entity, EntityInspector.Mode mode) {
			SelectingEntity = entity;
			if (SelectingInspector != null) {
				SelectingInspector.SetTarget(entity);
				SelectingInspector.InspectorMode = mode;
			}
			Selection.activeObject =
				mode != EntityInspector.Mode.Entity || entity != null ?
				SelectingInspector : null;
		}


		private void EntityMenu (Entity entity) {
			var menu = new GenericMenu();

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


		// Artwork
		private void SyncArtwork () {
			try {

				EditorUtil.ProgressBar("", "Create Sprites", 0f);

				// Create Sprites
				Selection.objects = Util.GetFilesIn("Assets", false, "*.ase", "*.aseprite").Select(
					file => AssetDatabase.LoadAssetAtPath<Object>(EditorUtil.FixedRelativePath(file.FullName))
				).Where(obj => obj.name.ToLower() != "entity icon").ToArray();
				EditorApplication.ExecuteMenuItem("Tools/Aseprite Toolbox/Create Sprite for Selection");
				Selection.objects = null;

				EditorUtil.ProgressBar("", "Create Sheets", 0.25f);

				// Sprite Sheet
				ReloadSheetAssets();

				EditorUtil.ProgressBar("", "Create Maps from LDtk", 0.5f);

				// Ldtk Level
				ReloadAllLevels();

				EditorUtil.ProgressBar("", "Custom Events", 0.75f);

				// Custom Events
				PerformArtworkEvent();

				EditorUtil.ProgressBar("Finish", "Custom Events", 1f);

				// Finish
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				LastSyncTick.Value = System.DateTime.Now.Ticks.ToString();
				LogAlert("Artwork Synced");
			} catch (System.Exception ex) { Debug.LogException(ex); }

			EditorUtil.ClearProgressBar();

		}


		private void ReloadSheetAssets () {
			var allSheets = EditorUtil.ForAllAssets<SpriteSheet>();
			foreach (var sheet in allSheets) {
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
			// Check Sheet in Game
			if (Game != null) {
				var data = Util.GetFieldValue(Game, "m_Data") as GameData;
				if (data != null && data.Sheets.Length != allSheets.Count()) {
					Debug.LogWarning($"There are {data.Sheets.Length} sheets in GameAsset but {allSheets.Count()} in total.");
				}
			}
		}


		private void ReloadAllLevels () {

			// Delete Old Maps
			var mapRoot = AUtil.GetMapRoot();
			Util.DeleteFolder(mapRoot);
			Util.CreateFolder(mapRoot);

			// Get Sprite Pool
			var tilesetPool = new Dictionary<string, Dictionary<Vector2Int, (int blockID, Int4 border)>>();
			foreach (var sheet in EditorUtil.ForAllAssets<SpriteSheet>()) {
				if (sheet.Texture == null || !sheet.Texture.isReadable) continue;
				var tPath = AssetDatabase.GetAssetPath(sheet.Texture);
				var spritePool = new Dictionary<Vector2Int, (int blockID, Int4 border)>();
				int tHeight = sheet.Texture.height;
				foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(tPath)) {
					if (obj is Sprite sp) {
						var _rect = sp.rect.ToRectInt();
						var _pos = _rect.position;
						_pos.y = tHeight - (_pos.y + _rect.height - 1) - 1;
						bool hasCol =
							(sp.border.x + sp.border.z).LessOrAlmost(sp.rect.width) &&
							(sp.border.w + sp.border.y).LessOrAlmost(sp.rect.height);
						var border = hasCol ? new Int4() {
							Left = (int)sp.border.x * Const.CELL_SIZE / (int)sp.rect.width,
							Right = (int)sp.border.z * Const.CELL_SIZE / (int)sp.rect.width,
							Up = (int)sp.border.w * Const.CELL_SIZE / (int)sp.rect.height,
							Down = (int)sp.border.y * Const.CELL_SIZE / (int)sp.rect.height
						} : new Int4() { Left = -1, Right = -1, Up = -1, Down = -1, };
						spritePool.TryAdd(_pos, (sp.name.AngeHash(), border));
					}
				}
				tilesetPool.TryAdd(sheet.Texture.name, spritePool);
			}

			// Load Levels
			int successCount = 0;
			int errorCount = 0;
			foreach (var file in Util.GetFilesIn(Application.dataPath, false, "*.ldtk")) {
				try {
					var json = Util.FileToText(file.FullName);
					var ldtk = JsonUtility.FromJson<LdtkProject>(json);
					bool success = LoadLdtkLevel(ldtk, tilesetPool);
					if (success) successCount++;
				} catch (System.Exception ex) {
					Debug.LogException(ex);
					errorCount++;
				}
			}

			// Dialog
			if (successCount + errorCount == 0) {
				EditorUtility.DisplayDialog("Done", "No Level Processesed.", "OK");
			} else {
				string message = "All Maps Reloaded. ";
				if (successCount > 0) {
					message += successCount + " success, ";
				}
				if (errorCount > 0) {
					message += errorCount + " failed.";
				}
				if (errorCount > 0) {
					Debug.LogWarning(message);
				}
			}
		}


		private bool LoadLdtkLevel (LdtkProject project, Dictionary<string, Dictionary<Vector2Int, (int blockID, Int4 border)>> spritePool) {
			// Ldtk >> Custom Data Pool
			var customDataPool = new Dictionary<int, (bool trigger, int tag)>();
			foreach (var tileset in project.defs.tilesets) {
				if (tileset.identifier.StartsWith("_")) continue;
				string tName = Util.GetNameWithoutExtension(tileset.relPath);
				if (!spritePool.ContainsKey(tName)) continue;
				var sPool = spritePool[tName];
				int gridX = tileset.__cWid;
				int gridY = tileset.__cHei;
				int space = tileset.spacing;
				int padding = tileset.padding;
				int gSize = tileset.tileGridSize;
				foreach (var data in tileset.customData) {
					int id = data.tileId;
					var tilePos = new Vector2Int(
						padding + (id % gridX) * (gSize + space),
						padding + (id / gridY) * (gSize + space)
					);
					if (sPool.TryGetValue(tilePos, out (int blockID, Int4 border) _value)) {
						var lines = data.data.Split('\n');

						// Is Trigger
						bool isTrigger = false;
						if (lines.Length > 0 && bool.TryParse(lines[0], out bool value)) {
							isTrigger = value;
						}

						// Tag String
						int tag = 0;
						if (lines.Length > 1) {
							tag = lines[1].AngeHash();
						}

						customDataPool.TryAdd(_value.blockID, (isTrigger, tag));
					}
				}
			}

			// Ldtk >> World Pool
			var worldPool = new Dictionary<(int x, int y), World>();
			foreach (var level in project.levels) {
				int levelPosX = level.worldX;
				int levelPosY = level.worldY;
				foreach (var layer in level.layerInstances) {
					int gridSize = layer.__gridSize;
					int offsetX = levelPosX + layer.__pxTotalOffsetX;
					int offsetY = levelPosY + layer.__pxTotalOffsetY;
					bool isLevel = layer.__identifier.ToLower().StartsWith("level");
					var tName = Util.GetNameWithoutExtension(layer.__tilesetRelPath);
					if (!spritePool.ContainsKey(tName) && layer.__type != "Entities") continue;
					TileInstance[] tiles = null;
					EntityInstance[] entities = null;
					switch (layer.__type) {
						case "Tiles":
							tiles = layer.gridTiles;
							break;
						case "IntGrid":
						case "AutoLayer":
							tiles = layer.autoLayerTiles;
							break;
						case "Entities":
							entities = layer.entityInstances;
							break;
					}
					if (tiles != null) {
						var sPool = spritePool[tName];
						foreach (var tile in tiles) {
							var srcPos = new Vector2Int(tile.src[0], tile.src[1]);
							if (sPool.ContainsKey(srcPos)) {
								ForLdtkTile(
									tile.px[0] + offsetX,
									tile.px[1] + offsetY,
									(_localX, _localY, world) => SetBlock(world, _localX, _localY, srcPos)
								);
							}
						}
					} else if (entities != null) {
						foreach (var entity in entities) {
							ForLdtkTile(
								entity.px[0] - (int)(entity.__pivot[0] * entity.width) + offsetX,
								entity.px[1] - (int)(entity.__pivot[1] * entity.height) + offsetY,
								(_localX, _localY, world) => {
									ref var e = ref world.Entities[
										_localY * Const.WORLD_MAP_SIZE + _localX
									];
									e.TypeID = entity.__identifier.AngeHash();
									e.Data = 0;
									foreach (var field in entity.fieldInstances) {
										switch (field.__identifier.ToLower()) {
											case "data":
												e.Data = field.__value;
												break;
										}
									}
								}
							);
						}
					}
					// Func
					void ForLdtkTile (int pixelX, int pixelY, System.Action<int, int, World> action) {
						int globalX = pixelX * Const.CELL_SIZE / gridSize;
						int globalY = -pixelY * Const.CELL_SIZE / gridSize - Const.CELL_SIZE;
						int unitX = globalX.AltDivide(Const.CELL_SIZE);
						int unitY = globalY.AltDivide(Const.CELL_SIZE);
						int worldX = unitX.AltDivide(Const.WORLD_MAP_SIZE);
						int worldY = unitY.AltDivide(Const.WORLD_MAP_SIZE);
						if (!worldPool.ContainsKey((worldX, worldY))) {
							worldPool.Add((worldX, worldY), new());
						}
						action(
							unitX.AltMod(Const.WORLD_MAP_SIZE),
							unitY.AltMod(Const.WORLD_MAP_SIZE),
							worldPool[(worldX, worldY)]
						);
					}
					void SetBlock (World world, int _localX, int _localY, Vector2Int srcPos) {
						if (isLevel) {
							var blocks = world.Level;
							ref var block = ref blocks[_localY * Const.WORLD_MAP_SIZE + _localX];
							var (blockID, border) = spritePool[tName][srcPos];
							block.TypeID = blockID;
							block.ColliderBorder = border;
							if (customDataPool.TryGetValue(block.TypeID, out (bool _isT, int _tag) _value)) {
								block.IsTrigger = _value._isT;
								block.Tag = _value._tag;
							} else {
								block.IsTrigger = false;
								block.Tag = 0;
							}
						} else {
							var blocks = world.Background;
							ref var block = ref blocks[_localY * Const.WORLD_MAP_SIZE + _localX];
							var (blockID, border) = spritePool[tName][srcPos];
							block.TypeID = blockID;
						}

					}
				}
			}

			// World Pool >> Maps (add into, no replace)
			int insID = 1;
			foreach (var pair in worldPool) {
				insID = pair.Value.EditorOnly_SaveToDisk(pair.Key.x, pair.Key.y, insID);
			}

			// Check Point File




			return true;
		}


		private void CheckSpriteNameDuplication () {
			string errorMsg = "";
			foreach (var sheet in EditorUtil.ForAllAssets<SpriteSheet>()) {
				var spriteSheetMap = new Dictionary<string, (SpriteSheet sheet, int index)>();
				try {
					var texture = sheet.Texture;
					if (texture == null) continue;
					var sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture));
					for (int i = 0; i < sprites.Length; i++) {
						var sprite = sprites[i];
						if (spriteSheetMap.ContainsKey(sprite.name)) {
							errorMsg += $"<color=#FFCC00>{sheet.name}</color> and <color=#FFCC00>{spriteSheetMap[sprite.name].sheet.name}</color> is having duplicate sprite <color=#FFCC00>{sprite.name}</color>\n";
						} else {
							spriteSheetMap.Add(sprite.name, (sheet, i));
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
			if (!string.IsNullOrEmpty(errorMsg)) {
				Debug.LogError(errorMsg);
			}
		}


		// Util
		private void LogAlert (string message) {
			AlertMessage = message;
			RequireAlertTime = EditorApplication.timeSinceStartup;
		}


		private void PerformArtworkEvent () {
			foreach (var type in typeof(IArtworkEvent).AllClassImplemented()) {
				if (System.Activator.CreateInstance(type) is IArtworkEvent e) e.Invoke();
			}
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
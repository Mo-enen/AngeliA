using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Game : MonoBehaviour {




		#region --- VAR ---


		// Dele
		public delegate Entity EntityHandler ();

		// Const
#if UNITY_EDITOR
		private readonly int[] ENTITY_CAPACITY = { 256, 128, 128, 1024, 128, 128, };
#else
		private readonly int[] ENTITY_CAPACITY = { 256, 128, 128, 1024, 128, 1, };
#endif
		private readonly int[] ENTITY_BUFFER_CAPACITY = { 128, 128, 128, 128, 128, 128 };
		private const int FRAME_RATE_LOW = 60;
		private const int FRAME_RATE_HIGHT = 120;

		// Api
		public Language CurrentLanguage { get; private set; } = null;
		public Dialogue CurrentDialogue { get; private set; } = null;
		public WorldSquad WorldSquad { get; } = new();
		public RectInt ViewRect { get; private set; } = new(0, 0, Mathf.Clamp(Const.DEFAULT_VIEW_WIDTH, Const.MIN_VIEW_WIDTH, Const.MAX_VIEW_WIDTH), Mathf.Clamp(Const.DEFAULT_VIEW_HEIGHT, Const.MIN_VIEW_HEIGHT, Const.MAX_VIEW_HEIGHT));
		public int EntityDirtyFlag { get; private set; } = 0;
		public int GlobalFrame { get; private set; } = 0;
		public bool DebugMode { get; set; } = false;

		// Ser
		[SerializeField] GameData m_Data = null;

		// Data
		private readonly Dictionary<int, EntityHandler> EntityHandlerPool = new();
		private readonly Dictionary<int, ScriptableObject> AssetPool = new();
		private readonly Stack<Object> UnloadAssetStack = new();
		private readonly HashSet<long> StagedEntityHash = new();
		private readonly Dictionary<int, Color32> MinimapColorPool = new();
		private readonly Dictionary<int, int> EntityThumbnailPool = new();
		private readonly Entity[][] Entities = new Entity[Const.ENTITY_LAYER_COUNT][];
		private readonly int[] EntityLength = new int[Const.ENTITY_LAYER_COUNT];
		private RectInt LoadedUnitRect = default;
		private RectInt SpawnRect = default;
		private RectInt DespawnRect = default;
		private RectInt? NewViewRect = null;
		private int ViewLerpRate = 1000;
		private bool Initialized = false;

		// Saving
		private readonly SavingInt LanguageIndex = new("Game.LanguageIndex", -1);
		private readonly SavingBool UseHighFramerate = new("Game.UseHighFramerate", true);


		#endregion




		#region --- MSG ---


		private void FixedUpdate () {
#if !UNITY_EDITOR
			DebugMode = false;
#endif
			if (!Initialized) {
				Initialized = true;
				Initialize();
			}
			FrameInput.FrameUpdate();
			CellRenderer.BeginDraw();
			FrameUpdate_View();
			CellPhysics.BeginFill(SpawnRect.x, SpawnRect.y);
			FrameUpdate_World();
			FrameUpdate_Entity();
			FrameUpdate_Misc();
			CellGUI.PerformFrame(GlobalFrame);
			CellRenderer.FrameUpdate();
			GlobalFrame++;
		}


		// Init
		private void Initialize () {


#if UNITY_EDITOR
			// Const Array Count Check
			if (
				Const.BLOCK_LAYER_COUNT != System.Enum.GetNames(typeof(BlockLayer)).Length ||
				Const.ENTITY_LAYER_COUNT != System.Enum.GetNames(typeof(EntityLayer)).Length ||
				Const.PHYSICS_LAYER_COUNT != System.Enum.GetNames(typeof(PhysicsLayer)).Length ||
				Const.PHYSICS_LAYER_COUNT != System.Enum.GetNames(typeof(PhysicsMask)).Length - 1 ||
				ENTITY_BUFFER_CAPACITY.Length != Const.ENTITY_LAYER_COUNT ||
				ENTITY_CAPACITY.Length != Const.ENTITY_LAYER_COUNT
			) {
				Debug.LogError("Const Array Size is wrong.");
				UnityEditor.EditorApplication.ExitPlaymode();
				return;
			}
			DebugMode = false;
#endif
			// Pipeline
			Init_System();
			Init_Entity();
			Init_Renderer();
			Init_Physics();
			Init_Audio();
			Init_Language();
			Init_Misc();

			WorldSquad.Init();

		}


		private void Init_System () {
			SetFramerate(UseHighFramerate.Value);
		}


		private void Init_Entity () {

			ViewRect = new(
				0, 0,
				Mathf.Clamp(Const.DEFAULT_VIEW_WIDTH, 0, Const.MAX_VIEW_WIDTH),
				Mathf.Clamp(Const.DEFAULT_VIEW_HEIGHT, 0, Const.MAX_VIEW_HEIGHT)
			);
			LoadedUnitRect = default;
			GlobalFrame = 0;

			// Entity
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				Entities[layerIndex] = new Entity[ENTITY_CAPACITY[layerIndex]];
				EntityLength[layerIndex] = 0;
			}

			// Handler Pool
			foreach (var eType in typeof(Entity).GetAllChildClass()) {
				int id = eType.ACode();
				var handler = CreateEntityHandler(eType);
				if (handler != null && !EntityHandlerPool.ContainsKey(id)) {
					EntityHandlerPool.Add(id, handler);
					EntityThumbnailPool.Add(id, handler().Thumbnail);
				}
#if UNITY_EDITOR
				else {
					Debug.LogError($"{eType} has same global id with {EntityHandlerPool[id]}");
				}
#endif
			}

			// Handler
			Entity.AddNewEntity = AddEntity;
			Entity.GetAsset = (id) => AssetPool.TryGet(id);
		}


		private void Init_Renderer () {
			// Cell Renderer
			var rendererRoot = new GameObject("Renderer", typeof(Camera)).transform;
			rendererRoot.SetParent(null);
			rendererRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			rendererRoot.localScale = Vector3.one;
			var camera = rendererRoot.GetComponent<Camera>();
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.backgroundColor = new Color32(34, 34, 34, 0);
			camera.cullingMask = -1;
			camera.orthographic = true;
			camera.orthographicSize = 1f;
			camera.nearClipPlane = 0f;
			camera.farClipPlane = 2f;
			camera.rect = new Rect(0f, 0f, 1f, 1f);
			camera.depth = 0f;
			camera.renderingPath = RenderingPath.UsePlayerSettings;
			camera.useOcclusionCulling = false;
			camera.allowHDR = false;
			camera.allowMSAA = false;
			camera.allowDynamicResolution = false;
			camera.targetDisplay = 0;
			CellRenderer.Init(m_Data.Sheets.Length, camera);
			for (int i = 0; i < m_Data.Sheets.Length; i++) {
				var sheet = m_Data.Sheets[i];
				// Mesh Renderer
				var tf = new GameObject(sheet.name, typeof(MeshFilter), typeof(MeshRenderer)).transform;
				tf.SetParent(rendererRoot);
				tf.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
				tf.localScale = Vector3.one;
				var mf = tf.GetComponent<MeshFilter>();
				var mr = tf.GetComponent<MeshRenderer>();
				mf.sharedMesh = new Mesh() { name = sheet.name, };
				mr.material = sheet.GetMaterial();
				mr.receiveShadows = false;
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				mr.staticShadowCaster = false;
				mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
				mr.sortingOrder = i;
				// Renderer
				CellRenderer.SetupLayer(i, sheet, mf);
			}
		}


		private void Init_Physics () {
			// Physics
			for (int i = 0; i < Const.PHYSICS_LAYER_COUNT; i++) {
				CellPhysics.SetupLayer(i);
			}
		}


		private void Init_Audio () {
			// Audio
			AudioPool.Initialize();
			foreach (var music in m_Data.Musics) {
				AudioPool.AddMusic(music);
			}
			foreach (var sound in m_Data.Sounds) {
				AudioPool.AddSound(sound);
			}
		}


		private void Init_Language () {
			bool success;
			if (LanguageIndex.Value < 0) {
				// First Time
				success = SetLanguage(Application.systemLanguage);
				if (!success) {
					switch (Application.systemLanguage) {
						case SystemLanguage.Chinese:
							success = SetLanguage(SystemLanguage.ChineseTraditional);
							if (!success) {
								success = SetLanguage(SystemLanguage.ChineseSimplified);
							}
							break;
						case SystemLanguage.ChineseSimplified:
							success = SetLanguage(SystemLanguage.ChineseTraditional);
							break;
						case SystemLanguage.ChineseTraditional:
							success = SetLanguage(SystemLanguage.ChineseSimplified);
							break;
						default:
							success = SetLanguage(SystemLanguage.English);
							break;
					}
				}
			} else {
				// From Saving
				success = SetLanguage((SystemLanguage)LanguageIndex.Value);
			}

			// Failback
			if (!success) {
				SetLanguage(SystemLanguage.English);
			}

		}


		private void Init_Misc () {
			WorldData.OnMapFilled += (obj) => {
				UnloadAssetStack.Push(obj);
			};
			WorldData.AllowWorldGenerator += () => !DebugMode;
			WorldSquad.BeforeWorldShift += () => { };
			// Asset Pool
			foreach (var asset in m_Data.Assets) {
				AssetPool.TryAdd(asset.name.ACode(), asset);
			}
			// Minimap
			foreach (var block in m_Data.MiniMap.Blocks) {
				MinimapColorPool.TryAdd(block.Name.ACode(), block.Color);
			}
			foreach (var entity in m_Data.MiniMap.Entities) {
				MinimapColorPool.TryAdd(entity.TypeFullName.ACode(), entity.Color);
			}
		}


		// Update
		private void FrameUpdate_View () {

			// Move View Rect
			if (NewViewRect.HasValue) {
				if (ViewLerpRate >= 1000) {
					ViewRect = NewViewRect.Value;
					NewViewRect = null;
				} else {
					ViewRect = new(
						ViewRect.x.LerpTo(NewViewRect.Value.x, ViewLerpRate),
						ViewRect.y.LerpTo(NewViewRect.Value.y, ViewLerpRate),
						ViewRect.width.LerpTo(NewViewRect.Value.width, ViewLerpRate),
						ViewRect.height.LerpTo(NewViewRect.Value.height, ViewLerpRate)
					);
					if (ViewRect.IsSame(NewViewRect.Value)) {
						NewViewRect = null;
					}
				}
			}
			CellRenderer.ViewRect = ViewRect;

			// Spawn Rect
			SpawnRect.width = ViewRect.width + Const.SPAWN_GAP * 2;
			SpawnRect.height = ViewRect.height + Const.SPAWN_GAP * 2;
			SpawnRect.x = ViewRect.x + (ViewRect.width - SpawnRect.width) / 2;
			SpawnRect.y = ViewRect.y + (ViewRect.height - SpawnRect.height) / 2;

			// Despawn Rect
			DespawnRect.x = SpawnRect.x - Const.DESPAWN_GAP;
			DespawnRect.y = SpawnRect.y - Const.DESPAWN_GAP;
			DespawnRect.width = SpawnRect.width + Const.DESPAWN_GAP * 2;
			DespawnRect.height = SpawnRect.height + Const.DESPAWN_GAP * 2;

		}


		private void FrameUpdate_World () {

			// World Squad
			WorldSquad.FrameUpdate(SpawnRect.CenterInt());

			if (WorldSquad.IsReady) {

				var spawnUnitRect = SpawnRect.Divide(Const.CELL_SIZE);

				// BG
				var rect = new RectInt(0, 0, Const.CELL_SIZE, Const.CELL_SIZE);
				foreach (var (block, x, y, _) in WorldSquad.ForAllBlocksInside(spawnUnitRect, BlockLayer.Background)) {
					rect.x = x;
					rect.y = y;
					CellRenderer.Draw(block.TypeID, rect, new Color32(255, 255, 255, 255));
				}

				// Level
				foreach (var (block, x, y, _) in WorldSquad.ForAllBlocksInside(spawnUnitRect.Expand(Const.BLOCK_SPAWN_PADDING), BlockLayer.Level)) {
					rect.x = x;
					rect.y = y;
					CellPhysics.FillBlock(PhysicsLayer.Level, rect, block.IsTrigger, block.Tag);
					CellRenderer.Draw(block.TypeID, rect, new Color32(255, 255, 255, 255));
				}

				// Entities
				if (!DebugMode) {
					// Real Entity
					foreach (var (entity, x, y) in WorldSquad.ForAllEntitiesInside(spawnUnitRect)) {
						if (!EntityHandlerPool.ContainsKey(entity.TypeID)) continue;
						int unitX = x.Divide(Const.CELL_SIZE);
						int unitY = y.Divide(Const.CELL_SIZE);
						if (LoadedUnitRect.Contains(unitX, unitY)) continue;
						if (!spawnUnitRect.Contains(unitX, unitY)) continue;
						var e = EntityHandlerPool[entity.TypeID].Invoke();
						e.InstanceID = entity.InstanceID;
						e.X = x;
						e.Y = y;
						AddEntity(e);
					}
				} else {
					// Thumbnail Only
					var _rect = rect;
					foreach (var (entity, x, y) in WorldSquad.ForAllEntitiesInside(spawnUnitRect)) {
						if (EntityThumbnailPool.TryGetValue(entity.TypeID, out int thumbnail)) {
							_rect.x = x;
							_rect.y = y;
							_rect.width = rect.width;
							_rect.height = rect.height;
							var uv = CellRenderer.GetUVRect(thumbnail);
							if ((uv.Width * uv.Height).NotAlmostZero()) {
								_rect = _rect.Fit((int)(uv.Width * 100000), (int)(uv.Height * 100000));
							}
							CellRenderer.Draw(thumbnail, _rect, new Color32(255, 255, 255, 255));
						}
					}
				}

				LoadedUnitRect = spawnUnitRect;
			}

		}


		private void FrameUpdate_Entity () {

			Entity.SpawnRect = SpawnRect;
			Entity.ViewRect = ViewRect;
			Entity.CameraRect = CellRenderer.CameraRect;
			Entity.MousePosition = new(
				CellRenderer.CameraRect.x.LerpTo(CellRenderer.CameraRect.xMax, FrameInput.MousePosition01.x),
				CellRenderer.CameraRect.y.LerpTo(CellRenderer.CameraRect.yMax, FrameInput.MousePosition01.y)
			);

			bool changed = false;

			// Remove Inactive and Outside Spawnrect
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				ref int eLen = ref EntityLength[layerIndex];
				for (int i = 0; i < eLen; i++) {
					var entity = entities[i];
					if (
#if UNITY_EDITOR
						(DebugMode && layerIndex != (int)EntityLayer.Debug) ||
#endif
						!entity.Active ||
						(entity.Despawnable && !DespawnRect.Contains(entity.X, entity.Y))
					) {
						changed = true;
						entity.OnDespawn(GlobalFrame);
						StagedEntityHash.Remove(entity.InstanceID);
						entities[i] = entities[eLen - 1];
						entities[eLen - 1] = null;
						if (eLen > 0) {
							eLen--;
							i--;
						}
					}
				}
			}

			// Fill Physics
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = EntityLength[layerIndex];
				for (int i = 0; i < len; i++) {
					entities[i].FillPhysics(GlobalFrame);
				}
			}

			// Physics Update
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = EntityLength[layerIndex];
				for (int i = 0; i < len; i++) {
					entities[i].PhysicsUpdate(GlobalFrame);
				}
			}

			// FrameUpdate
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = EntityLength[layerIndex];
				for (int i = 0; i < len; i++) {
					entities[i].FrameUpdate(GlobalFrame);
				}
			}

			if (changed) {
				EntityDirtyFlag++;
			}

		}


		private void FrameUpdate_Misc () {
			// Unload Assets
			while (UnloadAssetStack.Count > 0) {
				var asset = UnloadAssetStack.Pop();
				if (asset != null) Resources.UnloadAsset(asset);
			}
		}


		#endregion




		#region --- API ---


		// System
		public bool SetLanguage (SystemLanguage language) {
			bool success = false;
			var lAsset = Resources.Load<Language>($"Language/{language}");
			if (lAsset != null) {
				LanguageIndex.Value = (int)language;
				CurrentLanguage = lAsset;
				CurrentDialogue = Resources.Load<Dialogue>($"Dialogue/{language}");
				lAsset.Active();
				success = true;
			}
			return success;
		}


		public void SetFramerate (bool high) {
			UseHighFramerate.Value = high;
			Application.targetFrameRate = UseHighFramerate.Value ? FRAME_RATE_HIGHT : FRAME_RATE_LOW;
		}


		// Entity
		public void AddEntity (Entity entity) {
#if UNITY_EDITOR
			if (DebugMode && entity.Layer != EntityLayer.Debug) return;
#endif
			if (entity.InstanceID == 0) {
				entity.InstanceID = Entity.NewDynamicInstanceID();
			}
			if (StagedEntityHash.Contains(entity.InstanceID)) return;
			StagedEntityHash.Add(entity.InstanceID);
			int layer = (int)entity.Layer;
			var entities = Entities[layer];
			ref int len = ref EntityLength[layer];
			if (len < entities.Length) {
				entities[len] = entity;
				len++;
			}
		}


		public T FindEntityOfType<T> () where T : Entity {
			for (int i = 0; i < Const.ENTITY_LAYER_COUNT; i++) {
				var e = FindEntityOfType<T>((EntityLayer)i);
				if (e != null) {
					return e;
				}
			}
			return null;
		}


		public T FindEntityOfType<T> (EntityLayer layer) where T : Entity {
			var entities = Entities[(int)layer];
			int len = EntityLength[(int)layer];
			for (int i = 0; i < len; i++) {
				var e = entities[i];
				if (e is T) {
					return e as T;
				}
			}
			return null;
		}


		// View
		public void SetViewPositionDely (int x, int y, int lerp = 1000) {
			NewViewRect = NewViewRect.HasValue ?
				new RectInt(x, y, NewViewRect.Value.width, NewViewRect.Value.height) :
				new RectInt(x, y, ViewRect.width, ViewRect.height);
			ViewLerpRate = lerp;
		}


		public void SetViewSizeDely (int width, int height, int lerp = 1000) {
			NewViewRect = NewViewRect.HasValue ?
				new RectInt(NewViewRect.Value.x, NewViewRect.Value.y, width, height) :
				new RectInt(ViewRect.x, ViewRect.y, width, height);
			ViewLerpRate = lerp;
		}


		public void StopViewDely () {
			NewViewRect = null;
			ViewLerpRate = 1000;
		}


		// Minimap
		public Color32 GetMinimapColor (int id) => GetMinimapColor(id, new Color32(255, 255, 255, 255));


		public Color32 GetMinimapColor (int id, Color32 defaultValue) => MinimapColorPool.ContainsKey(id) ? MinimapColorPool[id] : defaultValue;


		#endregion



		#region --- LGC ---


		private static EntityHandler CreateEntityHandler (System.Type type) {
			ConstructorInfo emptyConstructor = type.GetConstructor(System.Type.EmptyTypes);
			var dynamicMethod = new DynamicMethod("CreateInstance", type, System.Type.EmptyTypes, true);
			ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
			ilGenerator.Emit(OpCodes.Nop);
			ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
			ilGenerator.Emit(OpCodes.Ret);
			return (EntityHandler)dynamicMethod.CreateDelegate(typeof(EntityHandler));
		}


		#endregion




	}
}
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
		public int EntityDirtyFlag { get; private set; } = 0;
		public int GlobalFrame { get; private set; } = 0;
#if UNITY_EDITOR
		public bool DebugMode { get; set; } = false;
#endif

		// Ser
		[SerializeField] GameData m_Data = null;

		// Data
		private readonly Dictionary<int, EntityHandler> EntityHandlerPool = new();
		private readonly Dictionary<int, ScriptableObject> AssetPool = new();
		private readonly WorldSquad WorldSquad = new();
		private readonly Stack<Object> UnloadAssetStack = new();
		private readonly HashSet<int> StagedEntityHash = new();
		private Entity[][] Entities = null;
		private (Entity[] entity, int length)[] EntityBuffers = null;
		private RectInt ViewRect = new(0, 0, Mathf.Clamp(Const.DEFAULT_VIEW_WIDTH, 0, Const.MAX_VIEW_WIDTH), Mathf.Clamp(Const.DEFAULT_VIEW_HEIGHT, 0, Const.MAX_VIEW_HEIGHT));
		private RectInt LoadedUnitRect = default;
		private RectInt SpawnRect = default;
		private bool Initialized = false;

		// Saving
		private readonly SavingInt LanguageIndex = new("Game.LanguageIndex", -1);
		private readonly SavingBool UseHighFramerate = new("Game.UseHighFramerate", true);


		#endregion




		#region --- MSG ---


		private void FixedUpdate () {
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
			Entities = new Entity[Const.ENTITY_LAYER_COUNT][];
			EntityBuffers = new (Entity[], int)[Const.ENTITY_LAYER_COUNT];
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				Entities[layerIndex] = new Entity[ENTITY_CAPACITY[layerIndex]];
				EntityBuffers[layerIndex] = (new Entity[ENTITY_BUFFER_CAPACITY[layerIndex]], 0);
			}

			// ID Map
			foreach (var eType in typeof(Entity).GetAllChildClass()) {
				int id = eType.FullName.ACode();
				var handler = CreateEntityHandler(eType);
				if (handler != null && !EntityHandlerPool.ContainsKey(id)) {
					EntityHandlerPool.Add(id, handler);
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
			// Asset Pool
			foreach (var asset in m_Data.Assets) {
				AssetPool.TryAdd(asset.name.ACode(), asset);
			}
		}


		// Update
		private void FrameUpdate_View () {

			// Move View Rect
			CellRenderer.ViewRect = ViewRect;

			// Spawn Rect
			SpawnRect.width = ViewRect.width + Const.SPAWN_GAP * 2;
			SpawnRect.height = ViewRect.height + Const.SPAWN_GAP * 2;
			SpawnRect.x = ViewRect.x + (ViewRect.width - SpawnRect.width) / 2;
			SpawnRect.y = ViewRect.y + (ViewRect.height - SpawnRect.height) / 2;

		}


		private void FrameUpdate_World () {

			int bgLayerIndex = (int)BlockLayer.Background;
			int levelLayerIndex = (int)BlockLayer.Level;

			// World Squad
			WorldSquad.FrameUpdate(SpawnRect.CenterInt());

			if (WorldSquad.IsReady) {
				var spawnUnitRect = new RectInt(
					SpawnRect.x / Const.CELL_SIZE,
					SpawnRect.y / Const.CELL_SIZE,
					SpawnRect.width / Const.CELL_SIZE,
					SpawnRect.height / Const.CELL_SIZE
				);
				for (int worldI = 0; worldI <= 2; worldI++) {
					for (int worldJ = 0; worldJ <= 2; worldJ++) {
						TrySpawnAllUnits(WorldSquad.Worlds[worldI, worldJ], spawnUnitRect);
					}
				}
				LoadedUnitRect = spawnUnitRect;
			}

			// Func
			void TrySpawnAllUnits (WorldData world, RectInt spawnUnitRect) {
				if (world.IsFilling) return;
				var worldUnitRect = world.FilledUnitRect;
				if (!worldUnitRect.Overlaps(spawnUnitRect)) return;
				int unitL = Mathf.Max(spawnUnitRect.x, worldUnitRect.x);
				int unitR = Mathf.Min(spawnUnitRect.xMax, worldUnitRect.xMax);
				int unitD = Mathf.Max(spawnUnitRect.y, worldUnitRect.y);
				int unitU = Mathf.Min(spawnUnitRect.yMax, worldUnitRect.yMax);
				// Spawn BG/Entities for World
				for (int j = unitD; j < unitU; j++) {
					for (int i = unitL; i < unitR; i++) {
						TrySpawnBlocksForBackground(world, i - worldUnitRect.x, j - worldUnitRect.y, i, j);
						TrySpawnEntitiesForAllLayers(world, spawnUnitRect, i - worldUnitRect.x, j - worldUnitRect.y, i, j);
					}
				}
				// Spawn Level Blocks for World
				unitL = Mathf.Max(unitL - Const.BLOCK_SPAWN_PADDING, worldUnitRect.x);
				unitR = Mathf.Min(unitR + Const.BLOCK_SPAWN_PADDING, worldUnitRect.xMax);
				unitD = Mathf.Max(unitD - Const.BLOCK_SPAWN_PADDING, worldUnitRect.y);
				unitU = Mathf.Min(unitU + Const.BLOCK_SPAWN_PADDING, worldUnitRect.yMax);
				for (int j = unitD; j < unitU; j++) {
					for (int i = unitL; i < unitR; i++) {
						TrySpawnBlocksForLevel(world, i - worldUnitRect.x, j - worldUnitRect.y, i, j);
					}
				}
			}
			void TrySpawnBlocksForBackground (WorldData world, int localX, int localY, int globalUnitX, int globalUnitY) {
				var block = world.Blocks[localX, localY, bgLayerIndex];
				if (block.TypeID == 0) return;
				var rect = new RectInt(
					globalUnitX * Const.CELL_SIZE, globalUnitY * Const.CELL_SIZE,
					Const.CELL_SIZE, Const.CELL_SIZE
				);
				// Physics
				if (bgLayerIndex != (int)BlockLayer.Background) {
					CellPhysics.FillBlock(PhysicsLayer.Level, rect, block.IsTrigger, block.Tag);
				}
				// Draw
				CellRenderer.Draw(block.TypeID, rect, new Color32(255, 255, 255, 128));
			}
			void TrySpawnBlocksForLevel (WorldData world, int localX, int localY, int globalUnitX, int globalUnitY) {
				var block = world.Blocks[localX, localY, levelLayerIndex];
				if (block.TypeID == 0) return;
				var rect = new RectInt(
					globalUnitX * Const.CELL_SIZE, globalUnitY * Const.CELL_SIZE,
					Const.CELL_SIZE, Const.CELL_SIZE
				);
				// Physics
				CellPhysics.FillBlock(PhysicsLayer.Level, rect, block.IsTrigger, block.Tag);
				// Draw
				CellRenderer.Draw(block.TypeID, rect, new Color32(255, 255, 255, 128));
			}
			void TrySpawnEntitiesForAllLayers (WorldData world, RectInt spawnUnitRect, int localX, int localY, int globalUnitX, int globalUnitY) {
				for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
					var entity = world.Entities[localX, localY, layerIndex];
					if (entity.TypeID == 0 || !EntityHandlerPool.ContainsKey(entity.TypeID)) continue;
					if (LoadedUnitRect.Contains(globalUnitX, globalUnitY)) continue;
					if (!spawnUnitRect.Contains(globalUnitX, globalUnitY)) continue;
					if (StagedEntityHash.Contains(entity.InstanceID)) continue;
					var e = EntityHandlerPool[entity.TypeID].Invoke();
					e.InstanceID = entity.InstanceID;
					e.X = globalUnitX * Const.CELL_SIZE;
					e.Y = globalUnitY * Const.CELL_SIZE;
					AddEntity(e, (EntityLayer)layerIndex);
				}
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
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity == null) { continue; }
					if (
#if UNITY_EDITOR
						(DebugMode && layerIndex != (int)EntityLayer.Debug) ||
#endif
						!entity.Active ||
						(entity.Despawnable && !SpawnRect.Contains(entity.X, entity.Y))
					) {
						entity.OnDespawn(GlobalFrame);
						StagedEntityHash.Remove(entity.InstanceID);
						entities[i] = null;
						changed = true;
					}
				}
			}

			// Add New Entities
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				if (ENTITY_BUFFER_CAPACITY[layerIndex] <= 0) { continue; }
				var entities = Entities[layerIndex];
				var (buffers, bufferLen) = EntityBuffers[layerIndex];
				int entityLen = ENTITY_CAPACITY[layerIndex];
				int emptyIndex = 0;
				for (int i = 0; i < bufferLen; i++) {
					while (emptyIndex < entityLen && entities[emptyIndex] != null) {
						emptyIndex++;
					}
					if (emptyIndex < entityLen) {
						var e = entities[emptyIndex] = buffers[i];
						if (e.InstanceID == 0) {
							e.InstanceID = Entity.NewDynamicInstanceID();
						}
						StagedEntityHash.TryAdd(e.InstanceID);
						e.OnCreate(GlobalFrame);
						buffers[i] = null;
						changed = true;
					} else {
						System.Array.Clear(buffers, 0, buffers.Length);
					}
				}
				EntityBuffers[layerIndex].length = 0;
			}

			// Fill Physics
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity != null) {
						entity.FillPhysics(GlobalFrame);
					}
				}
			}

			// Physics Update
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity != null) {
						entity.PhysicsUpdate(GlobalFrame);
					}
				}
			}

			// FrameUpdate
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity != null) {
						entity.FrameUpdate(GlobalFrame);
					}
				}
			}

			if (changed) {
				EntityDirtyFlag++;
			}

		}


		private void FrameUpdate_Misc () {
			// Unload Assets
			while (UnloadAssetStack.Count > 0) {
				Resources.UnloadAsset(UnloadAssetStack.Pop());
			}
		}


		#endregion




		#region --- API ---


		public void AddEntity (Entity entity, EntityLayer layer) {
			int layerIndex = (int)layer;
			ref var buffer = ref EntityBuffers[layerIndex];
			if (buffer.length < ENTITY_BUFFER_CAPACITY[layerIndex]) {
				buffer.entity[buffer.length] = entity;
				buffer.length++;
			}
#if UNITY_EDITOR
			else {
				Debug.LogWarning($"[Entity] Entity buffer is full. (layer:{layer} capacity:{ENTITY_BUFFER_CAPACITY[layerIndex]})");
			}
#endif
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
			foreach (var e in entities) {
				if (e is T) {
					return e as T;
				}
			}
			return null;
		}


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
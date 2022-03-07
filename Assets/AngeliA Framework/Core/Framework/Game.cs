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
		public delegate WorldGenerator WorldGeneratorHandler ();

		// Const
		private readonly int[] ENTITY_CAPACITY = { 256, 512, 256, 256, 1024, };

		// Api
		public Language CurrentLanguage { get; private set; } = null;
		public Dialogue CurrentDialogue { get; private set; } = null;
		public WorldSquad WorldSquad { get; } = new();
		public RectInt ViewRect { get; private set; } = new(0, 0, Mathf.Clamp(Const.DEFAULT_VIEW_WIDTH, Const.MIN_VIEW_WIDTH, Const.MAX_VIEW_WIDTH), Mathf.Clamp(Const.DEFAULT_VIEW_HEIGHT, Const.MIN_VIEW_HEIGHT, Const.MAX_VIEW_HEIGHT));
		public int EntityDirtyFlag { get; private set; } = 0;
		public int GlobalFrame { get; private set; } = 0;
		public bool HighFramerate {
			get => UseHighFramerate.Value;
			set => SetFramerate(value);
		}

		// Ser
		[SerializeField] GameData m_Data = null;

		// Data
		private readonly Dictionary<int, EntityHandler> EntityHandlerPool = new();
		private readonly Dictionary<int, WorldGeneratorHandler> WorldGeneratorHandlerPool = new();
		private readonly Dictionary<Vector2Int, string> WorldFilePathPool = new();
		private readonly HashSet<int> StagedEntityHash = new();
		private readonly Entity[][] Entities = new Entity[Const.ENTITY_LAYER_COUNT][];
		private readonly int[] EntityLength = new int[Const.ENTITY_LAYER_COUNT];
		private RectInt EntityUpdateRect = default;
		private RectInt SpawnRect = default;
		private RectInt PrevSpawnRect = default;
		private RectInt DespawnRect = default;
		private RectInt? ViewRectDelay = null;
		private int ViewLerpRate = 1000;
		private int ViewDelayPriority = int.MinValue;
		private int CurrentDynamicInstanceID = 0;
		private bool Initialized = false;

		// Saving
		private readonly SavingInt LanguageIndex = new("Game.LanguageIndex", -1);
		private readonly SavingBool UseHighFramerate = new("Game.UseHighFramerate", true);
		private readonly SavingBool UseVSync = new("Game.UseVSync", true);


		#endregion




		#region --- MSG ---


		private void FixedUpdate () {
			if (!Initialized) {
				Initialized = true;
				Initialize();
			}
			CellRenderer.CameraUpdate(ViewRect);
			FrameInput.FrameUpdate(CellRenderer.CameraRect);
			CellRenderer.BeginDraw();
			FrameUpdate_View();
			CellPhysics.BeginFill(SpawnRect.x, SpawnRect.y);
			FrameUpdate_World();
			FrameUpdate_Entity();
			CellGUI.PerformFrame(GlobalFrame);
			CellRenderer.FrameUpdate();
			GlobalFrame++;
		}


		// Init
		private void Initialize () {

#if UNITY_EDITOR
			// Const Array Count Check
			if (
				ENTITY_CAPACITY.Length != Const.ENTITY_LAYER_COUNT
			) {
				Debug.LogError("Const Array Size is wrong.");
				UnityEditor.EditorApplication.ExitPlaymode();
				return;
			}
#endif

			// Pipeline
			CellRenderer.Initialize(m_Data.Sheets);
			FrameInput.Initialize();
			Init_System();
			Init_Entity();
			Init_Physics();
			Init_Audio();
			Init_Language();
			Init_World();
			Init_Custom();
			WorldSquad.Initialize();

		}


		private void Init_System () {
			SetFramerate(UseHighFramerate.Value);
			SetUseVSync(UseVSync.Value);
			Util.CreateFolder(AUtil.GetMapRoot());
			Util.CreateFolder(AUtil.GetDialogueRoot());
		}


		private void Init_Entity () {

			ViewRect = new(
				0, 0,
				Mathf.Clamp(Const.DEFAULT_VIEW_WIDTH, 0, Const.MAX_VIEW_WIDTH),
				Mathf.Clamp(Const.DEFAULT_VIEW_HEIGHT, 0, Const.MAX_VIEW_HEIGHT)
			);
			GlobalFrame = 0;

			// Entity
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				Entities[layerIndex] = new Entity[ENTITY_CAPACITY[layerIndex]];
				EntityLength[layerIndex] = 0;
			}

			// Handler Pool
			foreach (var eType in typeof(Entity).AllChildClass()) {
				int id = eType.AngeHash();
				var handler = CreateHandler<EntityHandler>(eType);
				if (handler == null) continue;
				if (!EntityHandlerPool.ContainsKey(id)) {
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
			Entity.SetViewPosition = SetViewPositionDely;
			Entity.SetViewSize = SetViewSizeDely;
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


		private void Init_World () {

			// Path
			World.GetWorldPath = (pos) => WorldFilePathPool.ContainsKey(pos) ? WorldFilePathPool[pos] : "";
			foreach (var file in Util.GetFilesIn(AUtil.GetMapRoot(), true, $"*.{Const.MAP_FILE_EXT}")) {
				try {
					string name = Util.GetNameWithoutExtension(file.Name);
					int _index = name.IndexOf('_');
					int worldX = int.Parse(name[.._index]);
					int worldY = int.Parse(name[(_index + 1)..]);
					WorldFilePathPool.TryAdd(new(worldX, worldY), file.FullName);
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}

			// Generator Pool
			foreach (var gType in typeof(WorldGenerator).AllChildClass()) {
				int id = gType.AngeHash();
				var handler = CreateHandler<WorldGeneratorHandler>(gType);
				if (handler != null && !WorldGeneratorHandlerPool.ContainsKey(id)) {
					WorldGeneratorHandlerPool.Add(id, handler);
				}
#if UNITY_EDITOR
				else {
					Debug.LogError($"{gType} has same global id with {WorldGeneratorHandlerPool[id]}");
				}
#endif
			}
			World.CreateGenerator = (id) => WorldGeneratorHandlerPool.ContainsKey(id) ? WorldGeneratorHandlerPool[id].Invoke() : null;
		}


		private void Init_Custom () {
			foreach (var type in typeof(IInitialize).AllClassImplemented()) {
				Util.InvokeStaticMethod(type, nameof(IInitialize.Initialize));
			}
		}


		// Update
		private void FrameUpdate_View () {

			// Move View Rect
			if (ViewRectDelay.HasValue) {
				if (ViewLerpRate >= 1000) {
					ViewRect = ViewRectDelay.Value;
					ViewRectDelay = null;
				} else {
					ViewRect = new(
						ViewRect.x.LerpTo(ViewRectDelay.Value.x, ViewLerpRate),
						ViewRect.y.LerpTo(ViewRectDelay.Value.y, ViewLerpRate),
						ViewRect.width.LerpTo(ViewRectDelay.Value.width, ViewLerpRate),
						ViewRect.height.LerpTo(ViewRectDelay.Value.height, ViewLerpRate)
					);
					if (ViewRect.IsSame(ViewRectDelay.Value)) {
						ViewRectDelay = null;
					}
				}
			}
			ViewDelayPriority = int.MinValue;

			// Entity Update Rect
			EntityUpdateRect.width = ViewRect.width + Const.ENTITY_UPDATE_GAP * 2;
			EntityUpdateRect.height = ViewRect.height + Const.ENTITY_UPDATE_GAP * 2;
			EntityUpdateRect.x = ViewRect.x + (ViewRect.width - EntityUpdateRect.width) / 2;
			EntityUpdateRect.y = ViewRect.y + (ViewRect.height - EntityUpdateRect.height) / 2;

			// Spawn Rect
			SpawnRect.width = ViewRect.width + Const.SPAWN_PADDING * 2;
			SpawnRect.height = ViewRect.height + Const.SPAWN_PADDING * 2;
			SpawnRect.x = ViewRect.x + (ViewRect.width - SpawnRect.width) / 2;
			SpawnRect.y = ViewRect.y + (ViewRect.height - SpawnRect.height) / 2;

			// Despawn Rect
			DespawnRect.width = ViewRect.width + Const.DESPAWN_PADDING * 2;
			DespawnRect.height = ViewRect.height + Const.DESPAWN_PADDING * 2;
			DespawnRect.x = ViewRect.x + (ViewRect.width - DespawnRect.width) / 2;
			DespawnRect.y = ViewRect.y + (ViewRect.height - DespawnRect.height) / 2;

		}


		private void FrameUpdate_World () {
			WorldSquad.FrameUpdate(SpawnRect.CenterInt());
			var spawnUnitRect = SpawnRect.AltDivide(Const.CELL_SIZE);
			WorldSquad.DrawBlocksInside(spawnUnitRect, false);
			WorldSquad.DrawBlocksInside(spawnUnitRect.Expand(Const.BLOCK_SPAWN_PADDING_UNIT), true);
			WorldSquad.SpawnEntitiesInside(spawnUnitRect, this);
			PrevSpawnRect = spawnUnitRect;
		}


		private void FrameUpdate_Entity () {

			Entity.SpawnRect = SpawnRect;
			Entity.ViewRect = ViewRect;
			Entity.CameraRect = CellRenderer.CameraRect;
			bool changed = false;

			// Remove Inactive and Outside Spawnrect
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				ref int eLen = ref EntityLength[layerIndex];
				for (int i = 0; i < eLen; i++) {
					var entity = entities[i];
					if (!entity.Active || (entity.Despawnable && !DespawnRect.Contains(entity.X, entity.Y))) {
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
					var e = entities[i];
					if (e.ForceUpdate || e.Updated || EntityUpdateRect.Contains(e.X, e.Y)) {
						e.PhysicsUpdate(GlobalFrame);
					}
				}
			}

			// FrameUpdate
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = EntityLength[layerIndex];
				for (int i = 0; i < len; i++) {
					var e = entities[i];
					if (e.ForceUpdate || EntityUpdateRect.Contains(e.X, e.Y)) {
						e.FrameUpdate(GlobalFrame);
					}
				}
			}

			if (changed) EntityDirtyFlag++;

		}


		#endregion




		#region --- API ---


		// System
		public bool SetLanguage (SystemLanguage language) {
			bool success = false;
			Language lAsset = null;
			foreach (var lan in m_Data.Languages) {
				if (lan.LanguageID == language) {
					lAsset = lan;
					break;
				}
			}
			if (lAsset != null) {
				LanguageIndex.Value = (int)language;
				CurrentLanguage = lAsset;
				CurrentDialogue = Dialogue.LoadFromDisk(language);
				lAsset.Active();
				success = true;
			}
			return success;
		}


		public void SetFramerate (bool high) {
			UseHighFramerate.Value = high;
			Application.targetFrameRate = UseHighFramerate.Value ? 120 : 60;
		}


		public void SetUseVSync (bool vsync) {
			UseVSync.Value = vsync;
			QualitySettings.vSyncCount = vsync ? 1 : 0;
		}


		// Entity
		public void TrySpawnEntity (RectInt spawnUnitRect, World.Entity entity, int x, int y) {
			if (StagedEntityHash.Contains(entity.InstanceID)) return;
			if (!EntityHandlerPool.TryGetValue(entity.TypeID, out var eHandler)) return;
			int unitX = x.AltDivide(Const.CELL_SIZE);
			int unitY = y.AltDivide(Const.CELL_SIZE);
			if (PrevSpawnRect.Contains(unitX, unitY)) return;
			if (!spawnUnitRect.Contains(unitX, unitY)) return;
			var e = eHandler.Invoke();
			e.InstanceID = entity.InstanceID;
			e.Data = entity.Data;
			e.X = x;
			e.Y = y;
			AddEntity(e);
		}


		public void AddEntity (Entity entity) {
			if (entity.InstanceID == 0) {
				CurrentDynamicInstanceID--;
				entity.InstanceID = CurrentDynamicInstanceID;
			}
			if (StagedEntityHash.Contains(entity.InstanceID)) return;
			StagedEntityHash.Add(entity.InstanceID);
			int layer = (int)entity.Layer;
			var entities = Entities[layer];
			ref int len = ref EntityLength[layer];
			if (len < entities.Length) {
				entities[len] = entity;
				entity.OnCreate(GlobalFrame);
				len++;
			}
			EntityDirtyFlag++;
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
		public void SetViewPositionDely (int x, int y, int lerp = 1000, int priority = int.MinValue) {
			if (priority >= ViewDelayPriority) {
				ViewRectDelay = ViewRectDelay.HasValue ?
					new RectInt(x, y, ViewRectDelay.Value.width, ViewRectDelay.Value.height) :
					new RectInt(x, y, ViewRect.width, ViewRect.height);
				ViewLerpRate = lerp;
				ViewDelayPriority = priority;
			}
		}


		public void SetViewSizeDely (int width, int height, int lerp = 1000, int priority = int.MinValue) {
			if (priority >= ViewDelayPriority) {
				ViewRectDelay = ViewRectDelay.HasValue ?
				new RectInt(ViewRectDelay.Value.x, ViewRectDelay.Value.y, width, height) :
				new RectInt(ViewRect.x, ViewRect.y, width, height);
				ViewLerpRate = lerp;
				ViewDelayPriority = priority;
			}
		}


		#endregion




		#region --- LGC ---


		private static T CreateHandler<T> (System.Type type) where T : System.Delegate {
			ConstructorInfo emptyConstructor = type.GetConstructor(System.Type.EmptyTypes);
			if (emptyConstructor == null) return null;
			var dynamicMethod = new DynamicMethod("CreateInstance", type, System.Type.EmptyTypes, true);
			ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
			ilGenerator.Emit(OpCodes.Nop);
			ilGenerator.Emit(OpCodes.Newobj, emptyConstructor);
			ilGenerator.Emit(OpCodes.Ret);
			return (T)dynamicMethod.CreateDelegate(typeof(T));
		}


		#endregion




	}
}
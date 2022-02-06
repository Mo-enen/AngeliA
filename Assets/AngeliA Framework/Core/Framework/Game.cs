using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.World;
using AngeliaFramework.Entities;
using AngeliaFramework.Physics;
using AngeliaFramework.Rendering;
using AngeliaFramework.Audio;
using AngeliaFramework.Input;
using AngeliaFramework.Text;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Game", menuName = "бя AngeliA/Game", order = 99)]
	[PreferBinarySerialization]
	public class Game : ScriptableObject {




		#region --- VAR ---


		// Const
		private const int MAX_VIEW_WIDTH = 72 * Const.CELL_SIZE;
		private const int MAX_VIEW_HEIGHT = 56 * Const.CELL_SIZE;
		private const int SPAWN_GAP = 6 * Const.CELL_SIZE;
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
#if UNITY_EDITOR
		public bool DebugMode { get; set; } = false;
#endif

		// Ser
		[SerializeField] SpriteSheet[] m_Sheets = null;
		[SerializeField] AudioClip[] m_Musics = null;
		[SerializeField] AudioClip[] m_Sounds = null;
		[SerializeField] Language[] m_Languages = null;
		[SerializeField] ScriptableObject[] m_Assets = null;

		// Data
		private readonly Dictionary<int, System.Type> EntityTypePool = new();
		private readonly Dictionary<int, ScriptableObject> AssetPool = new();
		private readonly WorldSquad WorldSquad = new();
		private readonly Stack<Object> UnloadAssetStack = new();
		private readonly HashSet<int> StagedEntityHash = new();
		private Entity[][] Entities = null;
		private (Entity[] entity, int length)[] EntityBuffers = null;
		private RectInt ViewRect = new(0, 0, Mathf.Clamp(Const.DEFAULT_VIEW_WIDTH, 0, MAX_VIEW_WIDTH), Mathf.Clamp(Const.DEFAULT_VIEW_HEIGHT, 0, MAX_VIEW_HEIGHT));
		private RectInt PrevSpawnRect = default;
		private RectInt SpawnRect = default;
		private int GlobalFrame = 0;

		// Saving
		private readonly SavingInt LanguageIndex = new("Game.LanguageIndex", -1);
		private readonly SavingBool UseHighFramerate = new("Game.UseHighFramerate", true);


		#endregion




		#region --- MSG ---


		// Init
		public void Initialize () {

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
				Mathf.Clamp(Const.DEFAULT_VIEW_WIDTH, 0, MAX_VIEW_WIDTH),
				Mathf.Clamp(Const.DEFAULT_VIEW_HEIGHT, 0, MAX_VIEW_HEIGHT)
			);
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
				if (!EntityTypePool.ContainsKey(id)) {
					EntityTypePool.Add(id, eType);
				}
#if UNITY_EDITOR
				else {
					Debug.LogError($"{eType} has same global id with {EntityTypePool[id]}");
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
			CellRenderer.Init(m_Sheets.Length, camera);
			for (int i = 0; i < m_Sheets.Length; i++) {
				var sheet = m_Sheets[i];
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
			CellPhysics.Init(
				(MAX_VIEW_WIDTH + SPAWN_GAP * 2) / Const.CELL_SIZE,
				(MAX_VIEW_HEIGHT + SPAWN_GAP * 2) / Const.CELL_SIZE,
				Const.PHYSICS_LAYER_COUNT
			);
			for (int i = 0; i < Const.PHYSICS_LAYER_COUNT; i++) {
				CellPhysics.SetupLayer(i);
			}
		}


		private void Init_Audio () {
			// Audio
			AudioPool.Initialize();
			foreach (var music in m_Musics) {
				AudioPool.AddMusic(music);
			}
			foreach (var sound in m_Sounds) {
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
				SetLanguage(m_Languages[0].LanguageID);
			}

		}


		private void Init_Misc () {
			WorldData.OnMapFilled += (obj) => {
				UnloadAssetStack.Push(obj);
			};
			// Asset Pool
			foreach (var asset in m_Assets) {
				AssetPool.TryAdd(asset.name.ACode(), asset);
			}
		}


		// Update
		public void FrameUpdate () {
			FrameInput.FrameUpdate();
			FrameUpdate_View();
			FrameUpdate_World();
			FrameUpdate_Level();
			FrameUpdate_Entity();
			FrameUpdate_Misc();
			CellGUI.PerformFrame(GlobalFrame);
			CellRenderer.Update();
			PrevSpawnRect = SpawnRect;
			GlobalFrame++;
		}


		private void FrameUpdate_View () {

			// Move View Rect
			CellRenderer.ViewRect = ViewRect;

			// Spawn Rect
			SpawnRect.width = ViewRect.width + SPAWN_GAP * 2;
			SpawnRect.height = ViewRect.height + SPAWN_GAP * 2;
			SpawnRect.x = ViewRect.x + (ViewRect.width - SpawnRect.width) / 2;
			SpawnRect.y = ViewRect.y + (ViewRect.height - SpawnRect.height) / 2;

			// Physics Position
			CellPhysics.PositionX = SpawnRect.x = (int)ViewRect.center.x - SpawnRect.width / 2;
			CellPhysics.PositionY = SpawnRect.y = (int)ViewRect.center.y - SpawnRect.height / 2;

		}


		private void FrameUpdate_World () {

			// World Squad
			WorldSquad.Update(SpawnRect.center.RoundToInt());

			//Load Blocks and Entities




		}


		private void FrameUpdate_Level () {
			// Draw BG/Level, Fill Physics






#if UNITY_EDITOR
			if (DebugMode) {


			}
#endif
		}


		private void FrameUpdate_Entity () {

			Entity.SpawnRect = SpawnRect;
			Entity.ViewRect = ViewRect;
			Entity.CameraRect = CellRenderer.CameraRect;
			Entity.MousePosition = new(
				(int)Mathf.LerpUnclamped(CellRenderer.CameraRect.x, CellRenderer.CameraRect.xMax, FrameInput.MousePosition.x / Screen.width),
				(int)Mathf.LerpUnclamped(CellRenderer.CameraRect.y, CellRenderer.CameraRect.yMax, FrameInput.MousePosition.y / Screen.height)
			);

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
					}
				}
			}

			// Fill Physics
			CellPhysics.BeginFill();
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
			CellRenderer.BeginDraw();
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
					} else {
						System.Array.Clear(buffers, 0, buffers.Length);
					}
				}
				EntityBuffers[layerIndex].length = 0;
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
			foreach (var l in m_Languages) {
				if (l.LanguageID == language) {
					LanguageIndex.Value = (int)language;
					CurrentLanguage = l;
					CurrentDialogue = Resources.Load<Dialogue>($"Dialogue/{l.name}");
					l.Init();
					success = true;
					break;
				}
			}
			if (success) {
				foreach (var l in m_Languages) {
					if (l.LanguageID != language) {
						l.ClearCache();
					}
				}
			}
			return success;
		}


		public void SetFramerate (bool high) {
			UseHighFramerate.Value = high;
			Application.targetFrameRate = UseHighFramerate.Value ? FRAME_RATE_HIGHT : FRAME_RATE_LOW;
		}


		#endregion




	}
}
#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEditor;
	[CustomEditor(typeof(Game))]
	public class Game_Inspector : Editor {
		[InitializeOnLoadMethod]
		private static void Init () {
			int gameCount = 0;
			foreach (var guid in AssetDatabase.FindAssets("t:Game")) {
				if (AssetDatabase.LoadAssetAtPath<Game>(AssetDatabase.GUIDToAssetPath(guid)) != null) {
					gameCount++;
					if (gameCount > 1) {
						Debug.LogError("[Game] only 1 game asset is allowed in the project.");
						break;
					}
				}
			}
		}
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
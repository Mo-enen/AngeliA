using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Game", menuName = "AngeliA/New Game", order = 99)]
	public class Game : ScriptableObject {




		#region --- VAR ---


		// Const
		private const int MAX_VIEW_WIDTH = 72 * Const.CELL_SIZE;
		private const int MAX_VIEW_HEIGHT = 56 * Const.CELL_SIZE;
		private const int SPAWN_GAP = 12 * Const.CELL_SIZE;
		private readonly int[] ENTITY_CAPACITY = { 128, 128, 1024, 128, };
		private readonly int[] ENTITY_BUFFER_CAPACITY = { 64, 64, 64, 64 };

		// Ser
		[SerializeField] SpriteSheet[] m_Sheets = null;
		[SerializeField] AudioClip[] m_Musics = null;
		[SerializeField] AudioClip[] m_Sounds = null;

		// Data
		private Dictionary<int, System.Type> EntityTypePool = new();
		private Entity[][] Entities = null;
		private (Entity[] entity, int length)[] EntityBuffers = null;
		private RectInt ViewRect = default;
		private RectInt SpawnRect = default;
		private uint PhysicsFrame = uint.MinValue + 1;


		#endregion




		#region --- MSG ---


		// Init
		public void Initialize () {

#if UNITY_EDITOR
			// Const Array Count Check
			if (
				Const.ENTITY_LAYER_COUNT != System.Enum.GetNames(typeof(EntityLayer)).Length ||
				Const.PHYSICS_LAYER_COUNT != System.Enum.GetNames(typeof(PhysicsLayer)).Length ||
				ENTITY_BUFFER_CAPACITY.Length != Const.ENTITY_LAYER_COUNT ||
				ENTITY_CAPACITY.Length != Const.ENTITY_LAYER_COUNT
			) {
				Debug.LogError("Const Array Size is wrong.");
				UnityEditor.EditorApplication.ExitPlaymode();
				return;
			}
#endif

			// System
			Application.targetFrameRate = Application.platform == RuntimePlatform.WindowsEditor ? 10000 : 120;

			// Pipeline
			Init_Entity();
			Init_CellRenderer();
			Init_Physics();
			Init_Audio();

		}


		private void Init_Entity () {
			// Entity
			Entities = new Entity[Const.ENTITY_LAYER_COUNT][];
			EntityBuffers = new (Entity[], int)[Const.ENTITY_LAYER_COUNT];
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				Entities[layerIndex] = new Entity[ENTITY_CAPACITY[layerIndex]];
				EntityBuffers[layerIndex] = (new Entity[ENTITY_BUFFER_CAPACITY[layerIndex]], 0);
			}
			// ID Map
			foreach (var eType in typeof(Entity).GetAllChildClass()) {
				int id = eType.FullName.GetAngeliaHashCode();
				if (!EntityTypePool.ContainsKey(id)) {
					EntityTypePool.Add(id, eType);
				}
#if UNITY_EDITOR
				else {
					Debug.LogError($"{eType} has same global id with {EntityTypePool[id]}");
				}
#endif
			}
		}


		private void Init_CellRenderer () {
			// Cell Renderer
			var rendererRoot = new GameObject("Renderer").transform;
			rendererRoot.SetParent(null);
			rendererRoot.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			rendererRoot.localScale = Vector3.one;
			CellRenderer.Init(m_Sheets.Length);
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
				(MAX_VIEW_WIDTH + SPAWN_GAP) / Const.CELL_SIZE,
				(MAX_VIEW_HEIGHT + SPAWN_GAP) / Const.CELL_SIZE,
				Const.PHYSICS_LAYER_COUNT
			);
			for (int i = 0; i < Const.PHYSICS_LAYER_COUNT; i++) {
				CellPhysics.SetupLayer(i);
			}
		}


		private void Init_Audio () {
			// Audio
			Audio.Initialize();
			foreach (var music in m_Musics) {
				Audio.AddMusic(music);
			}
			foreach (var sound in m_Sounds) {
				Audio.AddMusic(sound);
			}
		}


		// Update
		public void FrameUpdate () {
			FrameInput.FrameUpdate();
			FrameUpdate_View();
			FrameUpdate_Level();
			FrameUpdate_Entity();
			CellRenderer.Update();
		}


		private void FrameUpdate_View () {

			// Move View Rect
			ViewRect.width = Mathf.Clamp(32 * Const.CELL_SIZE, 0, MAX_VIEW_WIDTH);
			ViewRect.height = Mathf.Clamp(16 * Const.CELL_SIZE, 0, MAX_VIEW_HEIGHT);
			ViewRect.x = Const.CELL_SIZE * 128;
			ViewRect.y = Const.CELL_SIZE * 12;
			CellRenderer.ViewRect = ViewRect;

			// Spawn Rect
			SpawnRect.width = ViewRect.width + SPAWN_GAP;
			SpawnRect.height = ViewRect.height + SPAWN_GAP;
			SpawnRect.x = ViewRect.x + (ViewRect.width - SpawnRect.width) / 2;
			SpawnRect.y = ViewRect.y + (ViewRect.height - SpawnRect.height) / 2;
			Entity.SetSpawnRect(SpawnRect);
			Entity.SetViewRect(ViewRect);

			// Physics Position
			CellPhysics.PositionX = SpawnRect.x = (int)ViewRect.center.x - SpawnRect.width / 2;
			CellPhysics.PositionY = SpawnRect.y = (int)ViewRect.center.y - SpawnRect.height / 2;

		}


		private void FrameUpdate_Level () {
			// Draw BG/Level, Fill Physics




		}


		private void FrameUpdate_Entity () {

			// Remove Inactive and Outside Spawnrect
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity == null) { continue; }
					if (
						!entity.Active ||
						(entity.Despawnable && !SpawnRect.Contains(entity.X, entity.Y))
					) {
						entities[i] = null;
					}
				}
			}

			// Fill Physics
			CellPhysics.BeginFill(PhysicsFrame);
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity != null) {
						entity.FillPhysics();
					}
				}
			}
			PhysicsFrame++;

			// Update / Draw
			CellRenderer.BeginDraw();
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity != null) {
						entity.FrameUpdate();
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
						entities[emptyIndex] = buffers[i];
						buffers[i] = null;
					} else {
						System.Array.Clear(buffers, 0, buffers.Length);
					}
				}
				EntityBuffers[layerIndex].length = 0;
			}

		}


		#endregion




		#region --- API ---


		public Entity AddEntity (System.Type type, EntityLayer layer) => AddEntity(
			System.Activator.CreateInstance(type) as Entity,
			layer
		);


		public T AddEntity<T> (T entity, EntityLayer layer) where T : Entity {
			int layerIndex = (int)layer;
			ref var buffer = ref EntityBuffers[layerIndex];
			if (buffer.length < ENTITY_BUFFER_CAPACITY[layerIndex]) {
				buffer.entity[buffer.length] = entity;
				buffer.length++;
				return entity;
			}
			return null;
		}


		#endregion




	}
}




#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEditor;
	[CustomEditor(typeof(Game))]
	public class Game_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
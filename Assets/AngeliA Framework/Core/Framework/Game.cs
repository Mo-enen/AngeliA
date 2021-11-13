using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Game", menuName = "AngeliA/New Game", order = 99)]
	public class Game : ScriptableObject {




		#region --- VAR ---


		// Const
		private const int MAX_SPAWN_WIDTH = 72 * Const.CELL_SIZE;
		private const int MAX_SPAWN_HEIGHT = 56 * Const.CELL_SIZE;
		private readonly int[] ENTITY_CAPACITY = { 128, 128, 1024, 128, };
		private readonly int[] ENTITY_BUFFER_CAPACITY = { 64, 64, 64, 64 };

		// Ser
		[SerializeField] SpriteSheet[] m_Sheets = null;

		// Data
		private Dictionary<ushort, System.Type> EntityTypePool = new();
		private Dictionary<string, uint> SpriteSheetIDMaps = new();
		private Entity[][] Entities = null;
		private Entity[][] EntityBuffers = null;
		private RectInt ViewRect = default;
		private RectInt SpawnRect = default;
		private int[] EntityBufferLength = null;
		private uint PhysicsFrame = uint.MinValue + 1;


		#endregion




		#region --- MSG ---


		public void Init () {

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
			}
#endif

			// Entity
			Entity.GetSpriteGlobalID = GetSpriteGlobalID;
			Entity.CreateEntity = CreateEntity;

			// Entity Global ID Map
			foreach (var eType in typeof(Entity).GetAllChildClass()) {
				ushort id = Entity.GetGlobalTypeID(eType);
				if (!EntityTypePool.ContainsKey(id)) {
					EntityTypePool.Add(id, eType);
				} else {
					Debug.LogError($"{eType} has same global id with {EntityTypePool[id]}");
				}
			}

			// Sprite Index Map
			SpriteSheetIDMaps = new Dictionary<string, uint>();
			for (uint sheetIndex = 0; sheetIndex < m_Sheets.Length; sheetIndex++) {
				var sheet = m_Sheets[sheetIndex];
				int len = sheet.Sprites.Length;
				for (uint spIndex = 0; spIndex < len; spIndex++) {
					var sp = sheet.Sprites[spIndex];
					if (!SpriteSheetIDMaps.ContainsKey(sp.name)) {
						SpriteSheetIDMaps.TryAdd(sp.name, sheetIndex * Const.MAX_SPRITE_PER_SHEET + spIndex);
					}
#if UNITY_EDITOR
					else {
						Debug.LogError(
							$"<color=#ffcc00>{sp.name}</color> from " +
							$"<color=#ffcc00>{sheet.name}</color> already exists in " +
							$"<color=#ffcc00>{m_Sheets[SpriteSheetIDMaps[sp.name] / Const.MAX_SPRITE_PER_SHEET].name}</color>"
						);
					}
#endif
				}
			}

			// Cell Renderer
			CellRenderer.InitLayers(m_Sheets.Length);
			for (int i = 0; i < m_Sheets.Length; i++) {
				var sheet = m_Sheets[i];
				CellRenderer.SetupLayer(i, sheet.RendererCapacity, sheet.GetMaterial(), sheet.GetUVs());
			}

			// Entity
			Entities = new Entity[Const.ENTITY_LAYER_COUNT][];
			EntityBuffers = new Entity[Const.ENTITY_LAYER_COUNT][];
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				Entities[layerIndex] = new Entity[ENTITY_CAPACITY[layerIndex]];
				EntityBuffers[layerIndex] = new Entity[ENTITY_BUFFER_CAPACITY[layerIndex]];
			}
			EntityBufferLength = new int[Const.ENTITY_LAYER_COUNT];

			// Physics
			CellPhysics.Init(
				MAX_SPAWN_WIDTH / Const.CELL_SIZE,
				MAX_SPAWN_HEIGHT / Const.CELL_SIZE,
				Const.PHYSICS_LAYER_COUNT
			);
			for (int i = 0; i < Const.PHYSICS_LAYER_COUNT; i++) {
				CellPhysics.SetupLayer(i);
			}

			// FPS
			Application.targetFrameRate = Application.platform == RuntimePlatform.WindowsEditor ? 10000 : 120;

		}


		public void FrameUpdate () {
			FrameUpdate_Input();
			FrameUpdate_View();
			FrameUpdate_Level();
			FrameUpdate_Entity();
		}


		private void FrameUpdate_Input () => FrameInput.FrameUpdate();


		private void FrameUpdate_View () {

			// View
			ViewRect.width = 32 * Const.CELL_SIZE;
			ViewRect.height = 16 * Const.CELL_SIZE;
			ViewRect.x = 0;
			ViewRect.y = 0;
			CellRenderer.ViewRect = ViewRect;

			// Spawn Rect
			SpawnRect.width = Mathf.Clamp(ViewRect.width + 12, 0, MAX_SPAWN_WIDTH);
			SpawnRect.height = Mathf.Clamp(ViewRect.height + 12, 0, MAX_SPAWN_HEIGHT);
			SpawnRect.x = ViewRect.x + (ViewRect.width - SpawnRect.width) / 2;
			SpawnRect.y = ViewRect.y + (ViewRect.height - SpawnRect.height) / 2;

			// Physics
			CellPhysics.PositionX = SpawnRect.x = (int)ViewRect.center.x - SpawnRect.width / 2;
			CellPhysics.PositionY = SpawnRect.y = (int)ViewRect.center.y - SpawnRect.height / 2;

		}


		private void FrameUpdate_Level () {
			// Draw BG/Level, Fill Physics




		}


		private void FrameUpdate_Entity () {

			// Remove Inactive
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity == null) { continue; }
					if (!entity.Active || !SpawnRect.Contains(entity.X, entity.Y)) {
						entities[i] = null;
						continue;
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
					if (entity == null) { continue; }
					entity.FillPhysics();
				}
			}

			// Update / Draw
			CellRenderer.BeginDraw();
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity == null) { continue; }
					entity.FrameUpdate();
				}
			}

			// Add New Entities
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				if (ENTITY_BUFFER_CAPACITY[layerIndex] <= 0) { continue; }
				var entities = Entities[layerIndex];
				var buffers = EntityBuffers[layerIndex];
				int bufferLen = EntityBufferLength[layerIndex];
				int entityLen = ENTITY_CAPACITY[layerIndex];
				int emptyIndex = 0;
				for (int i = 0; i < bufferLen; i++) {
					while (emptyIndex < entityLen && entities[emptyIndex] != null) {
						emptyIndex++;
					}
					if (emptyIndex >= entityLen) {
						System.Array.Clear(buffers, 0, buffers.Length);
						break;
					}
					entities[emptyIndex] = buffers[i];
					buffers[i] = null;
				}
				EntityBufferLength[layerIndex] = 0;
			}

			PhysicsFrame++;
		}


		#endregion




		#region --- LGC ---


		private uint GetSpriteGlobalID (string name) {
			if (name != null && SpriteSheetIDMaps.ContainsKey(name)) {
				return SpriteSheetIDMaps[name];
			} else {
#if UNITY_EDITOR
				Debug.LogWarning($"Fail to get sprite {name}");
#endif
				return uint.MaxValue;
			}
		}


		private Entity CreateEntity (System.Type type, EntityLayer layer) {
			int layerIndex = (int)layer;
			int bufferLen = EntityBufferLength[layerIndex];
			if (bufferLen < ENTITY_BUFFER_CAPACITY[layerIndex]) {
				if (System.Activator.CreateInstance(type) is Entity entity) {
					EntityBuffers[layerIndex][bufferLen] = entity;
					EntityBufferLength[layerIndex]++;
					return entity;
				}
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
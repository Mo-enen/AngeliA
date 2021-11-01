using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	[CreateAssetMenu(fileName = "New Game", menuName = "AngeliA/New Game", order = 99)]
	public class Game : ScriptableObject {




		#region --- VAR ---


		// Const
		private readonly int[] ENTITY_CAPACITY = { 128, 128, 1024, 128, };
		private readonly int[] ENTITY_BUFFER_CAPACITY = { 64, 64, 64, 64 };

		// Ser
		[SerializeField] SpriteSheet[] m_Sheets = null;

		// Data
		private Dictionary<ushort, System.Type> EntityTypePool = new Dictionary<ushort, System.Type>();
		private Dictionary<string, (int sheet, int id)> SpriteSheetIDMaps = new Dictionary<string, (int sheet, int id)>();
		private Entity[][] Entities = null;
		private Entity[][] EntityBuffers = null;
		private RectInt ViewRect = default;
		private RectInt SpawnRect = new RectInt(0, 0, 36 * Const.CELL_SIZE, 28 * Const.CELL_SIZE);
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
			Entity.GetSpriteSheetAndID = GetSpriteSheetAndID;
			Entity.CreateEntity = CreateEntity;

			// Entity Global ID Map
			foreach (var eType in typeof(Entity).GetAllChildClass()) {
				ushort id = (ushort)((uint)Mathf.Abs(eType.GetHashCode()) % ushort.MaxValue);
				if (!EntityTypePool.ContainsKey(id)) {
					EntityTypePool.Add(id, eType);
				} else {
					Debug.LogError($"{eType} has same global id with {EntityTypePool[id]}");
				}
			}

			// Sprite Index Map
			SpriteSheetIDMaps = new Dictionary<string, (int sheet, int id)>();
			for (int i = 0; i < m_Sheets.Length; i++) {
				var sheet = m_Sheets[i];
				int len = sheet.Sprites.Length;
				for (int j = 0; j < len; j++) {
					SpriteSheetIDMaps.TryAdd(sheet.Prefix + sheet.Sprites[j].name, (i, j));
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
				SpawnRect.width / Const.CELL_SIZE,
				SpawnRect.height / Const.CELL_SIZE,
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
			(ViewRect.width, ViewRect.height) = CellRenderer.GetCameraSize();
			//ViewRect.x = ;
			//ViewRect.y = ;
			CellPhysics.PositionX = SpawnRect.x = (int)ViewRect.center.x - SpawnRect.width / 2;
			CellPhysics.PositionY = SpawnRect.y = (int)ViewRect.center.y - SpawnRect.height / 2;



		}


		private void FrameUpdate_Level () {
			// Draw BG/Level, Fill Physics




		}


		private void FrameUpdate_Entity () {

			// Fill Physics
			CellPhysics.BeginFill(PhysicsFrame);
			for (int layerIndex = 0; layerIndex < Const.ENTITY_LAYER_COUNT; layerIndex++) {
				var entities = Entities[layerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity == null) { continue; }
					if (!entity.Active) { entities[i] = null; continue; }
					entity.FillPhysics();
				}
			}

			// Update/Draw
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




			///////////////////////////// Test ///////////////////////////////////////////////
			CellRenderer.Draw(
				TestLayer, TestID, TestX, TestY, TestPivotX, TestPivotY, TestRot,
				TestWidth, TestHeight, TestColor
			);
			CellRenderer.Draw(
				TestLayer, TestID, TestX + 256, TestY, TestPivotX, TestPivotY, TestRot,
				TestWidth, TestHeight, TestColor
			);
			CellRenderer.Draw(
				TestLayer, TestID, TestX + 256 + 256, TestY, TestPivotX, TestPivotY, TestRot,
				TestWidth, TestHeight, TestColor
			);
			//////////////////////////// Test ///////////////////////////////////////////////





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


		public int TestLayer = 0;
		public int TestID = 0;
		public int TestX = 0;
		public int TestY = 0;
		public int TestPivotX = 0;
		public int TestPivotY = 0;
		public int TestRot = 0;
		public int TestWidth = 256;
		public int TestHeight = 256;
		public Color32 TestColor = new Color32(255, 255, 255, 255);


		#endregion




		#region --- LGC ---


		private (int sheet, int id) GetSpriteSheetAndID (string name) => SpriteSheetIDMaps.ContainsKey(name) ? SpriteSheetIDMaps[name] : (-1, -1);


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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	[CreateAssetMenu(fileName = "New Game", menuName = "AngeliA/New Game", order = 99)]
	public class Game : ScriptableObject {




		#region --- VAR ---


		// Const
		private int RENDERER_LAYER_COUNT = 5;
		private int ENTITY_LAYER_COUNT = 3;
		private int PHYSCIS_LAYER_COUNT = 4;
		private readonly int[] CELL_CAPACITY = new int[] { 1024, 1024, 1024, 1024, 1024, };
		private readonly int[] ENTITY_CAPACITY = new int[] { 256, 128, 1024, };

		// Ser
		[SerializeField, LabeledByEnum(typeof(RendererLayer))] SpriteSheet[] m_Sheets = null;

		// Data
		private Dictionary<ushort, System.Type> EntityTypePool = new Dictionary<ushort, System.Type>();
		private Dictionary<string, int>[] SpriteIDMaps = new Dictionary<string, int>[0];
		private Entity[][] Entities = null;
		private int[] PrevEmptyEntityIndex = null;
		private int LayerIndex = 0;
		private RectInt ViewRect = default;
		private RectInt SpawnRect = new RectInt(0, 0, 36 * 512, 28 * 512);


		#endregion




		#region --- MSG ---


		public void Init () {

			RENDERER_LAYER_COUNT = System.Enum.GetNames(typeof(RendererLayer)).Length;
			ENTITY_LAYER_COUNT = System.Enum.GetNames(typeof(EntityLayer)).Length;
			PHYSCIS_LAYER_COUNT = System.Enum.GetNames(typeof(PhysicsLayer)).Length;

			// Entity
			Entity.GetSpriteIndex = GetSpriteIDFromName;
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
			SpriteIDMaps = new Dictionary<string, int>[RENDERER_LAYER_COUNT];
			for (int i = 0; i < RENDERER_LAYER_COUNT; i++) {
				var map = new Dictionary<string, int>();
				var sheet = m_Sheets[i];
				int len = sheet.Sprites.Length;
				for (int j = 0; j < len; j++) {
					map.TryAdd(sheet.Sprites[j].name, j);
				}
				SpriteIDMaps[i] = map;
			}

			// Cell Renderer
			CellRenderer.InitLayers(RENDERER_LAYER_COUNT);
			for (int i = 0; i < RENDERER_LAYER_COUNT; i++) {
				var sheet = m_Sheets[i];
				CellRenderer.SetupLayer(i, CELL_CAPACITY[i], sheet.GetMaterial(), sheet.GetUVs());
			}

			// Entity
			Entities = new Entity[ENTITY_LAYER_COUNT][];
			PrevEmptyEntityIndex = new int[ENTITY_LAYER_COUNT];
			for (int layerIndex = 0; layerIndex < ENTITY_LAYER_COUNT; layerIndex++) {
				Entities[layerIndex] = new Entity[ENTITY_CAPACITY[layerIndex]];
			}

			// Physics
			CellPhysics.Init(SpawnRect.width / 512, SpawnRect.height / 512);

			// FPS
			Application.targetFrameRate = Application.platform == RuntimePlatform.WindowsEditor ? 10000 : 120;

		}


		public void FrameUpdate () {
			CellPhysics.Clear();
			FrameUpdate_View();
			FrameUpdate_Level();
			FrameUpdate_Entity();
		}


		private void FrameUpdate_View () {
			(ViewRect.width, ViewRect.height) = CellRenderer.GetCameraSize();
			//ViewRect.x = ;
			//ViewRect.y = ;
			SpawnRect.x = (int)ViewRect.center.x - SpawnRect.width / 2;
			SpawnRect.y = (int)ViewRect.center.y - SpawnRect.height / 2;



		}


		private void FrameUpdate_Level () {
			// Draw Level, Update Physics




		}


		private void FrameUpdate_Entity () {
			for (LayerIndex = 2; LayerIndex < ENTITY_LAYER_COUNT; LayerIndex++) {
				CellRenderer.BeginDraw(LayerIndex);
				var entities = Entities[LayerIndex];
				int len = entities.Length;
				for (int i = 0; i < len; i++) {
					var entity = entities[i];
					if (entity == null) { continue; }
					if (!entity.Active) { entities[i] = null; continue; }
					entity.FrameUpdate();
				}
				CellRenderer.EndDraw();
			}
		}


		#endregion




		#region --- LGC ---


		private int GetSpriteIDFromName (string name) {
			var map = SpriteIDMaps[LayerIndex];
			return map.ContainsKey(name) ? map[name] : -1;
		}


		private Entity CreateEntity (System.Type type, RendererLayer layer) {
			int layerIndex = (int)layer;
			var entities = Entities[layerIndex];
			int len = entities.Length;
			int offset = PrevEmptyEntityIndex[layerIndex];
			for (int i = 0; i < len; i++) {
				int index = (i + offset) % len;
				if (entities[index] == null) {
					PrevEmptyEntityIndex[layerIndex] = index;
					var entity = System.Activator.CreateInstance(type) as Entity;
					if (entity != null) {
						entity.Active = true;
					}
					entities[index] = entity;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public enum Layer {
		Background = 0,
		Level = 1,
		Item = 2,
		Character = 3,
		Effect = 4,
	}



	[CreateAssetMenu(fileName = "New Game", menuName = "AngeliA/New Game", order = 99)]
	public class Game : ScriptableObject {




		#region --- VAR ---


		// Const
		private readonly int[] CELL_CAPACITY = new int[] { 1024, 1024, 1024, 1024, 1024, };
		private readonly int[] ENTITY_CAPACITY = new int[] { 256, 256, 256, 256, 256, };

		// Ser
		[SerializeField, LabeledByEnum(typeof(Layer))] SpriteSheet[] m_Sheets = null;

		// Data
		private Dictionary<ushort, System.Type> EntityTypePool = new Dictionary<ushort, System.Type>();
		private Dictionary<string, int>[] SpriteIDMaps = new Dictionary<string, int>[0];
		private Entity[][] Entities = null;
		private int[] PrevEmptyEntityIndex = null;
		private int LayerIndex = 0;
		private int LayerCount = 0;


		#endregion




		#region --- MSG ---


		public void Init () {

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
			LayerCount = System.Enum.GetNames(typeof(Layer)).Length;
			SpriteIDMaps = new Dictionary<string, int>[LayerCount];
			for (int i = 0; i < LayerCount; i++) {
				var map = new Dictionary<string, int>();
				var sheet = m_Sheets[i];
				int len = sheet.Sprites.Length;
				for (int j = 0; j < len; j++) {
					map.TryAdd(sheet.Sprites[j].name, j);
				}
				SpriteIDMaps[i] = map;
			}

			// Cell Renderer
			CellRenderer.InitLayers(LayerCount);
			for (int i = 0; i < LayerCount; i++) {
				var sheet = m_Sheets[i];
				CellRenderer.SetupLayer(i, CELL_CAPACITY[i], sheet.GetMaterial(), sheet.GetUVs());
			}

			// Entity
			Entities = new Entity[LayerCount][];
			PrevEmptyEntityIndex = new int[LayerCount];
			for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
				Entities[layerIndex] = new Entity[ENTITY_CAPACITY[layerIndex]];
			}

			// FPS
			Application.targetFrameRate = Application.platform == RuntimePlatform.WindowsEditor ? 10000 : 120;

		}


		public void FrameUpdate () {

			// Camera




			// Level



			// Update All Entities
			for (LayerIndex = 0; LayerIndex < LayerCount; LayerIndex++) {
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


		private Entity CreateEntity (System.Type type, Layer layer) {
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
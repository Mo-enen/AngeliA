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
		private readonly int[] LAYER_CAPACITY = new int[] {
			1024, 1024, 1024, 1024, 1024,
		};

		// Ser
		[SerializeField, LabeledByEnum(typeof(Layer))] SpriteSheet[] m_Sheets = null;

		// Data
		private Dictionary<ushort, System.Type> EntityTypePool = new Dictionary<ushort, System.Type>();
		private Dictionary<string, int>[] SpriteIndexMaps = new Dictionary<string, int>[0];
		private Dictionary<string, int> CurrentSpriteIndexMap = null;
		private int LayerCount = 0;

		[Space(12)]
		public int TestID = 0;
		public Vector2Int TestPos = default;
		[Range(0, 1)] public float TestPivotX = 0.5f;
		[Range(0, 1)] public float TestPivotY = 0.5f;
		public int TestRot = 0;
		public Vector2Int TestSize = new Vector2Int(512, 512);
		public Color TestColor = Color.white;


		#endregion




		#region --- MSG ---


		public void Init () {

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
			SpriteIndexMaps = new Dictionary<string, int>[LayerCount];
			for (int i = 0; i < LayerCount; i++) {
				var map = new Dictionary<string, int>();
				var sheet = m_Sheets[i];
				int len = sheet.Sprites.Length;
				for (int j = 0; j < len; j++) {
					map.TryAdd(sheet.Sprites[j].name, j);
				}
				SpriteIndexMaps[i] = map;
			}
			Entity.GetSpriteIndex = GetSpriteIndexFromName;

			// Cell Renderer
			CellRenderer.InitLayers(LayerCount);
			for (int i = 0; i < LayerCount; i++) {
				var sheet = m_Sheets[i];
				CellRenderer.SetupLayer(i, LAYER_CAPACITY[i], sheet.GetMaterial(), sheet.GetUVs());
			}

			// FPS
			Application.targetFrameRate = Application.platform == RuntimePlatform.WindowsEditor ? 10000 : 120;

		}


		public void FrameUpdate () {


			for (int layerIndex = 0; layerIndex < LayerCount; layerIndex++) {
				CellRenderer.BeginDraw(layerIndex);
				CurrentSpriteIndexMap = SpriteIndexMaps[layerIndex];



				CellRenderer.EndDraw();
			}


			// Test
			CellRenderer.BeginDraw(0);
			CellRenderer.Draw(
				TestID,
				TestPos.x, TestPos.y,
				TestPivotX, TestPivotY,
				TestRot, TestSize.x, TestSize.y,
				TestColor
			);
			CellRenderer.Draw(
				TestID + 1,
				TestPos.x + 512, TestPos.y,
				TestPivotX, TestPivotY,
				TestRot, TestSize.x, TestSize.y,
				TestColor
			);
			CellRenderer.Draw(
				TestID + 2,
				TestPos.x + 1024, TestPos.y,
				TestPivotX, TestPivotY,
				TestRot, TestSize.x, TestSize.y,
				TestColor
			);
			CellRenderer.EndDraw();
			// Test

		}


		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---


		private int GetSpriteIndexFromName (string name) => CurrentSpriteIndexMap.ContainsKey(name) ? CurrentSpriteIndexMap[name] : -1;


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
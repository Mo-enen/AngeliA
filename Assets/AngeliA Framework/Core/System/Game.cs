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
		private Dictionary<ushort, System.Type> EntityPool = new Dictionary<ushort, System.Type>();
		private Dictionary<string, int>[] SpriteIndexMaps = new Dictionary<string, int>[0];

		[Space(12)]
		public int TestID = 0;
		public Vector2Int TestPos = default;
		public Vector2Int TestPivot = new Vector2Int(500, 500);
		public int TestRot = 0;
		public int TestScl = 1000;
		public Color TestColor = Color.white;



		#endregion




		#region --- MSG ---


		public void Init () {

			// Entity Global ID Map
			foreach (var eType in typeof(Entity).GetAllChildClass()) {
				ushort id = (ushort)((uint)Mathf.Abs(eType.GetHashCode()) % ushort.MaxValue);
				if (!EntityPool.ContainsKey(id)) {
					EntityPool.Add(id, eType);
				} else {
					Debug.LogError($"{eType} has same global id with {EntityPool[id]}");
				}
			}

			// Sprite Index Map
			int layerCount = System.Enum.GetNames(typeof(Layer)).Length;
			SpriteIndexMaps = new Dictionary<string, int>[layerCount];
			for (int i = 0; i < layerCount; i++) {
				var map = new Dictionary<string, int>();
				var sheet = m_Sheets[i];
				int len = sheet.Sprites.Length;
				for (int j = 0; j < len; j++) {
					map.TryAdd(sheet.Sprites[j].name, j);
				}
				SpriteIndexMaps[i] = map;
			}

			// Cell Renderer
			CellRenderer.InitLayers(layerCount);
			for (int i = 0; i < layerCount; i++) {
				var sheet = m_Sheets[i];
				CellRenderer.SetupLayer(i, LAYER_CAPACITY[i], sheet.Material, sheet.GetUVs());
			}
			CellRenderer.FocusLayer(0);

			// FPS
			Application.targetFrameRate = Application.platform == RuntimePlatform.WindowsEditor ? 10000 : 120;

		}


		public void FrameUpdate () {

			CellRenderer.FocusLayer(0);

			CellRenderer.SetCell(
				0, TestID,
				TestPos.x, TestPos.y,
				TestPivot.x, TestPivot.y,
				TestRot, TestScl,
				TestColor
			);

			CellRenderer.MarkAsRoadblock(1);

		}


		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---





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
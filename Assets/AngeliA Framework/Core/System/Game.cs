using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Game", menuName = "AngeliA/New Game", order = 99)]
	public class Game : ScriptableObject {




		#region --- SUB ---


		public enum Layer {
			Background = 0,
			BackLevel = 1,
			Ground = 2,
			Item = 3,
			Character = 4,
			Effect = 5,
		}


		#endregion




		#region --- VAR ---


		// Ser
		[Header("Prefab")]
		[SerializeField] Prefab[] m_Prefabs = null;
		[Header("Sheet")]
		[SerializeField] SpriteSheet m_Background = null;
		[SerializeField] SpriteSheet m_Level = null;
		[SerializeField] SpriteSheet m_Item = null;
		[SerializeField] SpriteSheet m_Character = null;
		[SerializeField] SpriteSheet m_Effect = null;

		// Data
		private Dictionary<ushort, Prefab> PrefabPool = new Dictionary<ushort, Prefab>();


		#endregion




		#region --- MSG ---


		public void Init () {

			// Prefab
			foreach (var prefab in m_Prefabs) {
				PrefabPool.TryAdd(prefab.GlobalID, prefab);
			}

			// Cell Renderer
			CellRenderer.InitLayers(System.Enum.GetNames(typeof(Layer)).Length);
			CellRenderer.SetupLayer((int)Layer.Background, 1024, m_Background.Material, m_Background.GetUVs());
			CellRenderer.SetupLayer((int)Layer.BackLevel, 1024, m_Level.Material, m_Level.GetUVs());
			CellRenderer.SetupLayer((int)Layer.Ground, 1024, m_Level.Material, m_Level.GetUVs());
			CellRenderer.SetupLayer((int)Layer.Item, 1024, m_Item.Material, m_Item.GetUVs());
			CellRenderer.SetupLayer((int)Layer.Character, 1024, m_Character.Material, m_Character.GetUVs());
			CellRenderer.SetupLayer((int)Layer.Effect, 1024, m_Effect.Material, m_Effect.GetUVs());

			// FPS
			Application.targetFrameRate = Application.platform == RuntimePlatform.WindowsEditor ? 10000 : 120;

		}


		public void FrameUpdate () {



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
		private void OnEnable () => FixDuplicateGlobalID();
		private void OnDisable () => FixDuplicateGlobalID();
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) {
				FixDuplicateGlobalID();
			}
		}
		private void FixDuplicateGlobalID () {
			serializedObject.Update();
			var p_Prefabs = serializedObject.FindProperty("m_Prefabs");
			int len = p_Prefabs.arraySize;
			if (len > ushort.MaxValue - 10) {
				Debug.LogError("Too many prefabs in game.");
				return;
			}
			bool changed = false;
			var hash = new HashSet<ushort>();
			var random = new System.Random(Random.Range(int.MinValue, int.MaxValue));
			for (int i = 0; i < len; i++) {
				var prefab = p_Prefabs.GetArrayElementAtIndex(i).objectReferenceValue as Prefab;
				while (prefab.GlobalID == 0 || hash.Contains(prefab.GlobalID)) {
					prefab.SetGlobalID((ushort)random.Next(1, ushort.MaxValue));
					EditorUtility.SetDirty(prefab);
					changed = true;
				}
				hash.Add(prefab.GlobalID);
			}
			serializedObject.ApplyModifiedProperties();
			if (changed) {
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
			}
		}
	}
}
#endif

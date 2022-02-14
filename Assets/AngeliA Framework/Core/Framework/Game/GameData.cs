using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Game Data", menuName = "бя AngeliA/Game Data", order = 99)]
	[PreferBinarySerialization]
	public class GameData : ScriptableObject {

		// SUB
		[System.Serializable]
		public class MinimapConfig {

			[System.Serializable]
			public struct EntityItem {
				[TypeEnum(typeof(Entity))] public string TypeFullName;
				public Color32 Color;
			}

			[System.Serializable]
			public struct BlockItem {
				public string Name;
				public Color32 Color;
			}

			public List<EntityItem> Entities => m_Entities;
			public List<BlockItem> Blocks => m_Blocks;

			[SerializeField] List<EntityItem> m_Entities = null;
			[SerializeField] List<BlockItem> m_Blocks = null;

		}

		// Api
		public SpriteSheet[] Sheets => m_Sheets;
		public AudioClip[] Musics => m_Musics;
		public AudioClip[] Sounds => m_Sounds;
		public ScriptableObject[] Assets => m_Assets;
		public MinimapConfig MiniMap => m_MiniMap;

		// Ser
		[SerializeField] SpriteSheet[] m_Sheets = null;
		[SerializeField] AudioClip[] m_Musics = null;
		[SerializeField] AudioClip[] m_Sounds = null;
		[SerializeField] ScriptableObject[] m_Assets = null;
		[SerializeField] MinimapConfig m_MiniMap = null;

	}
}
#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEditor;
	[CustomEditor(typeof(GameData))]
	public class Game_Inspector : Editor {
		[InitializeOnLoadMethod]
		private static void Init () {
			int gameCount = 0;
			foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(GameData)}")) {
				if (AssetDatabase.LoadAssetAtPath<GameData>(AssetDatabase.GUIDToAssetPath(guid)) != null) {
					gameCount++;
					if (gameCount > 1) {
						Debug.LogError("[Game] only 1 game data is allowed in the project.");
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
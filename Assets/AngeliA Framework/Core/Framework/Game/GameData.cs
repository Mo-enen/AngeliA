using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Game Data", menuName = "бя AngeliA/Game Data", order = 99)]
	[PreferBinarySerialization]
	public class GameData : ScriptableObject {

		// Api
		public SpriteSheet[] Sheets => m_Sheets;
		public AudioClip[] Musics => m_Musics;
		public AudioClip[] Sounds => m_Sounds;
		public Language[] Languages => m_Languages;
		public ScriptableObject[] Assets => m_Assets;

		// Ser
		[SerializeField] SpriteSheet[] m_Sheets = null;
		[SerializeField] AudioClip[] m_Musics = null;
		[SerializeField] AudioClip[] m_Sounds = null;
		[SerializeField] Language[] m_Languages = null;
		[SerializeField] ScriptableObject[] m_Assets = null;

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
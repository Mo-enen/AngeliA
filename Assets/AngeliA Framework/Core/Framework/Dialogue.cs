using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[System.Serializable]
	public class Dialogue {




		#region --- SUB ---


		[System.Serializable]
		public class Conversation {
			public string GlobalKey = "";


		}


		#endregion




		#region --- VAR ---


		// Api-Ser
		public SystemLanguage LanguageID = SystemLanguage.English;
		public List<Conversation> Conversations = new();

		// Data
		[System.NonSerialized] private Dictionary<int, Conversation> ConversationPool = new();


		#endregion




		#region --- API ---


		public Dialogue (SystemLanguage language) => LanguageID = language;


		public static Dialogue LoadFromDisk (SystemLanguage language) {
			try {
				var path = Util.CombinePaths(AUtil.GetDialogueRoot(), language.ToString());
				if (Util.FileExists(path) && Util.FileToObject(path) is Dialogue dia) {
					dia.ConversationPool.Clear();
					foreach (var con in dia.Conversations) {
						dia.ConversationPool.TryAdd(con.GlobalKey.AngeHash(), con);
					}
					return dia;
				}
			} catch (System.Exception ex) {
#if UNITY_EDITOR
				Debug.LogException(ex);
#endif
			}
			return new Dialogue(language);
		}


#if UNITY_EDITOR
		public static void EditorOnly_SaveToDisk (Dialogue dialogue) {
			if (dialogue == null) return;
			try {
				Util.ObjectToFile(
					dialogue,
					Util.CombinePaths(AUtil.GetDialogueRoot(), dialogue.LanguageID.ToString())
				);
			} catch (System.Exception ex) {
				Debug.LogException(ex);
			}
		}
#endif



		#endregion




	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(Dialogue))]
	public class Dialogue_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif

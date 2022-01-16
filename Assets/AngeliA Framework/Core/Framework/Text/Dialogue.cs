using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Text {
	[CreateAssetMenu(fileName = "New Dialogue", menuName = "AngeliA/Dialogue", order = 99)]
	public class Dialogue : ScriptableObject {




		#region --- VAR ---


		


		#endregion




		#region --- API ---





		#endregion




		#region --- LGC ---





		#endregion




	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	using AngeliaFramework.Text;
	[CustomEditor(typeof(Dialogue))]
	public class Dialogue_Inspector : Editor {
		[InitializeOnLoadMethod]
		private static void Init () {
			// Check Dialogue Files
			Game game = null;
			foreach (var guid in AssetDatabase.FindAssets("t:Game")) {
				game = AssetDatabase.LoadAssetAtPath<Game>(AssetDatabase.GUIDToAssetPath(guid));
				if (game != null) { break; }
			}
			if (game != null) {
				var languages = Util.GetFieldValue(game, "m_Languages") as Language[];
				foreach (var language in languages) {
					string dPath = $"Assets/Resources/{language.name}.asset";
					if (!Util.FileExists(dPath)) {
						Debug.LogWarning($"[Dialogue] {language.name} don't have dialogue asset.");
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

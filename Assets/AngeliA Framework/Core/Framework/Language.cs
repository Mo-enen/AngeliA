using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Language", menuName = "AngeliA/Language", order = 99)]
	public class Language : ScriptableObject {




		#region --- SUB ---


		[System.Serializable]
		public class LanguageCell {
			public string Key;
			public string Value;
		}


		#endregion




		#region --- VAR ---


		// Api
		public string this[string key] => Map.ContainsKey(key) ? Map[key] : "";
		public SystemLanguage LanguageID => m_Language;
		public string DisplayName => m_DisplayName;

		// Ser
		[SerializeField] SystemLanguage m_Language = SystemLanguage.English;
		[SerializeField] string m_DisplayName = "";
		[SerializeField] LanguageCell[] m_Cells = null;

		// Data
		private Dictionary<string, string> Map = new();


		#endregion




		#region --- API ---


		public void Init () {
			Map.Clear();
			foreach (var cell in m_Cells) {
				Map.TryAdd(cell.Key, cell.Value);
			}
		}


		#endregion




		#region --- EDT ---
#if UNITY_EDITOR
		public LanguageCell[] Editor_GetCells () => m_Cells;
		public void Editor_SetCells (LanguageCell[] cells) => m_Cells = cells;
#endif
		#endregion




	}
}



#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(Language))]
	public class Language_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif

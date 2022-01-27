using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Text {
	[CreateAssetMenu(fileName = "New Language", menuName = "бя AngeliA/Language", order = 99)]
	[PreferBinarySerialization]
	public class Language : ScriptableObject {


		// SUB
		[System.Serializable]
		public class Cell {
			public string Key;
			public string Value;
		}


		// Api
		public string this[int key] => Map.ContainsKey(key) ? Map[key] : "";
		public SystemLanguage LanguageID => m_Language;
		public string DisplayName => m_DisplayName;

		// Ser
		[SerializeField] SystemLanguage m_Language = SystemLanguage.English;
		[SerializeField] string m_DisplayName = "";
		[SerializeField] Cell[] m_Cells = null;

		// Data
		private Dictionary<int, string> Map = new();


		// API
		public void Init () {
			Map.Clear();
			foreach (var cell in m_Cells) {
				Map.TryAdd(cell.Key.ACode(), cell.Value);
			}
		}


		public void ClearCache () => Map.Clear();


	}
}



#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEditor;
	using AngeliaFramework.Text;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



namespace YayaMaker.UI {
	public class LibraryUI : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable]
		public class LibraryItem {
			public string Name = "";
			public ushort EntityID = 0;
			public Sprite Icon = null;
		}


		[System.Serializable]
		public class ItemGroup {
			public string Name = "";
			public List<LibraryItem> Items = new();
		}


		#endregion




		#region --- VAR ---


		// Api
		public List<ItemGroup> Groups => m_Groups;

		// Ser
		[SerializeField] List<ItemGroup> m_Groups = new();
		[SerializeField] RectTransform m_Content = null;


		#endregion




		#region --- MSG ---


		private void Start () {
			LoadGroup(0);
		}


		#endregion




		#region --- API ---


		public void LoadGroup (int groupIndex) {
			if (groupIndex < 0 || groupIndex >= m_Groups.Count) { return; }
			m_Content.DestroyAllChirldrenImmediate();
			var group = m_Groups[groupIndex];





		}


		public void UI_SwitchGroup (bool isOn) {
			if (!isOn) { return; }
			

		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}

#if UNITY_EDITOR
namespace YayaMaker.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using global::YayaMaker.UI;
	using AngeliaFramework;


	[CustomEditor(typeof(LibraryUI))]
	public class LibraryUI_Inspector : Editor {
		public override void OnInspectorGUI () {
			GUILayout.Space(4);
			if (GUI.Button(GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true)), "Copy Entity ID")) {
				var menu = new GenericMenu();
				foreach (var eType in typeof(Entity).GetAllChildClass()) {
					menu.AddItem(new GUIContent(eType.Name), false, () => {
						GUIUtility.systemCopyBuffer = Entity.GetGlobalTypeID(eType).ToString();
					});
				}
				menu.ShowAsContext();
			}
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif
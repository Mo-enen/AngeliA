using System.Collections;
using System.Collections.Generic;
using UIGadget;
using UnityEngine;
using UnityEngine.UI;



namespace YayaMaker.UI {
	public class LibraryUI : MonoBehaviour {




		#region --- SUB ---


		[System.Serializable]
		public struct LibraryItem {
			public int EntityID;
			public Sprite Icon;
		}


		[System.Serializable]
		public struct ItemGroup {
			public List<LibraryItem> Items;
		}


		public delegate void VoidUshortHandler (int? value);


		#endregion




		#region --- VAR ---


		// Api
		public static VoidUshortHandler OnSelectionChanged { get; set; } = null;

		// Ser
		[SerializeField] List<ItemGroup> m_Groups = new();
		[SerializeField] RectTransform m_View = null;
		[SerializeField] Grabber m_ItemTemplate = null;

		// Data
		private readonly List<Toggle> AllToggles = new();


		#endregion




		#region --- MSG ---


		private void Awake () => ReloadUI();


		private void Start () => ShowGroup(0);


		#endregion




		#region --- API ---


		public void ReloadUI () {
			var viewGroup = m_View.GetComponent<ToggleGroup>();
			foreach (RectTransform contentRT in m_View) {
				contentRT.DestroyAllChirldrenImmediate();
				int index = contentRT.GetSiblingIndex();
				var group = m_Groups[index];
				for (int i = 0; i < group.Items.Count; i++) {
					var item = group.Items[i];
					var grab = Util.SpawnItemUI(m_ItemTemplate, contentRT);
					var tg = grab.Grab<Toggle>();
					tg.SetIsOnWithoutNotify(false);
					tg.group = viewGroup;
					tg.onValueChanged.AddListener((isOn) => {
						OnSelectionChanged?.Invoke(isOn ? item.EntityID : null);
						foreach (var _tg in AllToggles) {
							if (_tg.isOn && _tg != tg) {
								_tg.SetIsOnWithoutNotify(false);
							}
						}
					});
					AllToggles.Add(tg);
					grab.Grab<Image>("Icon").sprite = item.Icon;
				}
			}
		}


		public void ShowGroup (int groupIndex) {
			if (groupIndex < 0 || groupIndex >= m_Groups.Count) { return; }
			foreach (RectTransform contentRT in m_View) {
				contentRT.gameObject.SetActive(contentRT.GetSiblingIndex() == groupIndex);
			}
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
					menu.AddItem(new GUIContent($"{eType.Name}: {Entity.GetGlobalTypeID(eType)}"), false, () => {
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
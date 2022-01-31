using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AngeliaFramework.Editor {
	[CreateAssetMenu(fileName = "New Palette", menuName = "бя AngeliA/Map Palette", order = 99)]
	public class MapPalette : ScriptableObject {



		#region --- SUB ---


		[System.Serializable]
		public class Unit {

			public bool IsEntity => string.IsNullOrEmpty(TypeFullName);

			public Sprite Sprite;
			[TypeEnum(typeof(Entities.Entity))] public string TypeFullName;

		}


		#endregion




		#region --- VAR ---


		// Api
		public int Count => m_Units.Count;
		public Unit this[int index] => m_Units[index];
		public bool Opening { get => m_Opening; set => m_Opening = value; }

		// Ser
		[SerializeField] bool m_Opening = false;
		[SerializeField] List<Unit> m_Units = null;


		#endregion




		#region --- API ---


		public void RemoveUnit (int index) => m_Units.RemoveAt(index);


		public void Add (Unit unit) {
			m_Units.Add(unit);
		}


		public void Sort () {
			m_Units.Sort((a, b) => {
				int result = a.TypeFullName.CompareTo(b.TypeFullName);
				if (result != 0) {
					return result;
				} else {
					if (a.Sprite != null && b.Sprite != null) {
						result = a.Sprite.texture.name.CompareTo(b.Sprite.texture.name);
						return result == 0 ? a.Sprite.name.CompareTo(b.Sprite.name) : result;
					} else {
						return a.Sprite != null ? -1 : b.Sprite != null ? 1 : 0;
					}
				}
			});
		}


		#endregion




	}


	public class MapEditorPalettePost : AssetPostprocessor {
		private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (MapEditor.Main == null) return;
			foreach (var path in importedAssets) {
				if (AssetDatabase.LoadAssetAtPath<MapPalette>(path) != null) {
					MapEditor.Main.SetNeedReloadAsset();
					return;
				}
			}
		}
	}


}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(MapPalette))]
	public class MapEditor_PaletteGroup_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif

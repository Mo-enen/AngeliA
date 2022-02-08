using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Editor {
	[CreateAssetMenu(fileName = "New Palette", menuName = "бя AngeliA/Map Palette", order = 99)]
	public class MapPalette : ScriptableObject {



		#region --- SUB ---


		[System.Serializable]
		public class Unit {

			public string DisplayName {
				get {
					if (IsEntity) {
						int dotIndex = TypeFullName.LastIndexOf('.');
						if (dotIndex >= 0 && TypeFullName[dotIndex + 1] == 'e') {
							dotIndex++;
						}
						return Util.GetDisplayName(
							dotIndex >= 0 ? TypeFullName[(dotIndex + 1)..] : TypeFullName
						);
					} else {
						return Sprite != null ? Sprite.name : "";
					}
				}
			}

			public bool IsEntity => !string.IsNullOrEmpty(TypeFullName);

			public Sprite Sprite;
			[TypeEnum(typeof(Entity))]
			public string TypeFullName;
			public int Tag;
			public bool IsTrigger;

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





}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;


	[CustomEditor(typeof(MapPalette))]
	public class MapPalette_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}


	public class MapEditorPalettePost : AssetPostprocessor {
		private static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (MapEditorWindow.Main == null) return;
			foreach (var path in importedAssets) {
				if (AssetDatabase.LoadAssetAtPath<MapPalette>(path) != null) {
					MapEditorWindow.Main.SetNeedReloadAsset();
					return;
				}
			}
		}
	}


}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Editor {
	[CreateAssetMenu(fileName = "New Palette", menuName = "бя AngeliA/Map Palette", order = 99)]
	public class MapPalette : ScriptableObject {



		#region --- SUB ---


		[System.Serializable]
		public class Unit {

			public string EntityDisplayName {
				get {
					int dotIndex = TypeFullName.LastIndexOf('.');
					if (dotIndex >= 0 && TypeFullName[dotIndex + 1] == 'e') {
						dotIndex++;
					}
					return Util.GetDisplayName(
						dotIndex >= 0 ? TypeFullName[(dotIndex + 1)..] : TypeFullName
					);
				}
			}
			public int EntityID => TypeFullName.ACode();

			public bool IsEntity => !string.IsNullOrEmpty(TypeFullName);

			// Block
			public int BlockID;
			public int Tag = 0;
			public bool IsTrigger = false;
			public BlockLayer BlockLayer = BlockLayer.Level;

			// Entity
			[TypeEnum(typeof(Entity))]
			public string TypeFullName;

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
					if (a.BlockID != 0 && b.BlockID != 0) {
						return result == 0 ? a.BlockID.CompareTo(b.BlockID) : result;
					} else {
						return a.BlockID != 0 ? -1 : b.BlockID != 0 ? 1 : 0;
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
			foreach (var path in importedAssets) {
				if (AssetDatabase.LoadAssetAtPath<MapPalette>(path) != null) {
					MapPaletteWindow.SetNeedReloadAsset();
					return;
				}
			}
		}
	}


}
#endif
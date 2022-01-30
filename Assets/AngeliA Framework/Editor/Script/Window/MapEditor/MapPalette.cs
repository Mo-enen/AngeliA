using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AngeliaFramework.Editor {
	[CreateAssetMenu(fileName = "New Palette", menuName = "бя AngeliA/Map Palette", order = 99)]
	public class MapPalette : ScriptableObject {



		#region --- SUB ---


		public abstract class Unit {
			public Sprite Sprite;
		}


		[System.Serializable]
		public class Block : Unit {

		}


		[System.Serializable]
		public class Entity : Unit {
			[TypeEnum(typeof(Entities.Entity))]
			public string TypeFullName;
		}


		#endregion




		#region --- VAR ---


		// Api
		public int AllCount => Blocks.Count + Entities.Count;
		public Unit this[int index] {
			get {
				if (index < Blocks.Count) {
					return Blocks[index];
				} else {
					return Entities[index - Blocks.Count];
				}
			}
		}


		public bool Opening = false;
		public List<Block> Blocks = null;
		public List<Entity> Entities = null;


		#endregion




		#region --- API ---


		public void RemoveUnit (int index) {
			if (index < Blocks.Count) {
				Blocks.RemoveAt(index);
			} else {
				Entities.RemoveAt(index - Blocks.Count);
			}
		}


		public void AddBlock (Block block) => Blocks.Add(block);


		public void AddEntity (Entity entity) => Entities.Add(entity);


		#endregion




		#region --- LGC ---




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

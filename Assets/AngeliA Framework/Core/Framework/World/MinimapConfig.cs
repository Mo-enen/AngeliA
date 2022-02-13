using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Minimap Config", menuName = "бя AngeliA/Minimap Config", order = 99)]
	[PreferBinarySerialization]
	public class MinimapConfig : ScriptableObject {


		[System.Serializable]
		public struct EntityItem {
			[TypeEnum(typeof(Entity))] public string TypeFullName;
			public Color32 Color;
		}


		[System.Serializable]
		public struct BlockItem {
			public string Name;
			public Color32 Color;
		}


		public List<EntityItem> Entities => m_Entities;
		public List<BlockItem> Blocks => m_Blocks;


		[SerializeField] List<EntityItem> m_Entities = null;
		[SerializeField] List<BlockItem> m_Blocks = null;


	}
}

#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(MinimapConfig))]
	public class MinimapConfig_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif

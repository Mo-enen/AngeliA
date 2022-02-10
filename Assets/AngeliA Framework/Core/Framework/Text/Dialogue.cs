using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Dialogue", menuName = "бя AngeliA/Dialogue", order = 99)]
	[PreferBinarySerialization]
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

	[CustomEditor(typeof(Dialogue))]
	public class Dialogue_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif

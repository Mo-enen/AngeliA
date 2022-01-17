using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Entities {
	[CreateAssetMenu(fileName = "New Renderer", menuName = "AngeliA/Character/Renderer", order = 99)]
	public class CharacterRenderer : ScriptableObject {




		#region --- VAR ---




		#endregion




		#region --- MSG ---


		public void FrameUpdate (int frame, eCharacter character) {



		}


		#endregion




		#region --- API ---




		#endregion




		#region --- LGC ---




		#endregion




	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEditor;
	using AngeliaFramework.Entities;
	[CustomEditor(typeof(CharacterRenderer))]
	public class CharacterRenderer_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif

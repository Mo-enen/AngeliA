using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Movement", menuName = "AngeliA/Character/Movement", order = 99)]
	public class CharacterMovement : ScriptableObject {




		#region --- VAR ---




		#endregion




		#region --- MSG ---


		public void FrameUpdate (int frame) {



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
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(CharacterMovement))]
	public class CharacterMovement_Inspector : Editor {
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}
}
#endif

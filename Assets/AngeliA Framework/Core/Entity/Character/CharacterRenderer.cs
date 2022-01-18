using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Rendering;


namespace AngeliaFramework.Entities {
	[CreateAssetMenu(fileName = "New Renderer", menuName = "AngeliA/Character/Renderer", order = 99)]
	public class CharacterRenderer : ScriptableObject {




		#region --- VAR ---


		[SerializeField] string m_Test = "";


		#endregion




		#region --- MSG ---


		public void FrameUpdate (int frame, eCharacter character) {

			// Debug
			CellRenderer.Draw(m_Test.ACode(), new RectInt(
				character.X - character.Width / 2, character.Y, character.Width, character.Height
			));
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

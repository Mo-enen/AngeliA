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


		public void FrameUpdate (int frame, eCharacter ch, Direction2 facing, bool squating) {

			// Debug
			CellRenderer.Draw(m_Test.ACode(), new RectInt(
				ch.X - (int)facing * ch.Width / 2,
				ch.Y,
				(int)facing * ch.Width,
				squating ? ch.Height / 2 : ch.Height
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

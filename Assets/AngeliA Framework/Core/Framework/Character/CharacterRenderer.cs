using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class CharacterRenderer {




		#region --- VAR ---


		public string Test { get; init; } = "Test Pump";
		public int Width { get; init; } = 256;
		public int Height { get; init; } = 256;
		public int SqrtHeight { get; init; } = 158;

		// Data
		private eRigidbody Rig = null;


		#endregion




		#region --- MSG ---


		public CharacterRenderer (eCharacter ch) {
			Rig = ch;
		}


		public void FrameUpdate (int frame, Direction2 facingX, bool squating) {

			// Debug
			CellRenderer.Draw(
				Test.ACode(),
				Rig.X,
				Rig.Y,
				500, 0, 0,
				(int)facingX * Width,
				squating ? SqrtHeight : Height
			);
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

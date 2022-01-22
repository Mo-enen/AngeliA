using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework.Rendering;


namespace AngeliaFramework.Entities {
	public class CharacterRenderer {




		#region --- VAR ---


		public string Test { get; init; } = "Test Pump";
		public int Width { get; init; } = 256;
		public int Height { get; init; } = 256;
		public int SqrtHeight { get; init; } = 158;


		#endregion




		#region --- MSG ---


		public void FrameUpdate (int frame, eCharacter ch) {

			// Debug
			CellRenderer.Draw(Test.ACode(),
				ch.X,
				ch.Y,
				500, 0, 0,
				(int)ch.Movement.CurrentFacing * Width,
				ch.Movement.IsSquating ? SqrtHeight : Height
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

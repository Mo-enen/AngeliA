using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	// === Main ===
	public partial class CharacterRenderer {




		#region --- VAR ---


		// Init
		

		// Api
		protected eCharacter Character { get; init; } = null;
		public virtual RectInt LocalBounds => new(-Const.CELL_SIZE / 2, 0, Const.CELL_SIZE, Const.CELL_SIZE * 2);


		#endregion




		#region --- MSG ---


		public CharacterRenderer (eCharacter ch) => Character = ch;


		public virtual void FrameUpdate (int frame) {

			if (Character.Movement.FacingFront) {
				// Front

				DrawTail();
				DrawHair(false);

				DrawArm(!Character.Movement.FacingRight);
				DrawHand(!Character.Movement.FacingRight);
				DrawLeg(!Character.Movement.FacingRight);
				DrawFoot(!Character.Movement.FacingRight);

				DrawBody();
				DrawBoingBoing();
				DrawHead();
				DrawFace();
				DrawHair(true);

				DrawArm(Character.Movement.FacingRight);
				DrawHand(Character.Movement.FacingRight);
				DrawLeg(Character.Movement.FacingRight);
				DrawFoot(Character.Movement.FacingRight);

			} else {
				// Back

				DrawArm(!Character.Movement.FacingRight);
				DrawHand(!Character.Movement.FacingRight);
				DrawLeg(!Character.Movement.FacingRight);
				DrawFoot(!Character.Movement.FacingRight);

				DrawHair(true);
				DrawFace();
				DrawHead();
				DrawBoingBoing();
				DrawBody();

				DrawArm(Character.Movement.FacingRight);
				DrawHand(Character.Movement.FacingRight);
				DrawLeg(Character.Movement.FacingRight);
				DrawFoot(Character.Movement.FacingRight);

				DrawHair(false);
				DrawTail();
			}

		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}


#if UNITY_EDITOR
namespace Yaya.Editor {
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

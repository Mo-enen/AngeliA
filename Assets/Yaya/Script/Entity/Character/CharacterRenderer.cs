using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class CharacterRenderer {




		#region --- VAR ---


		// Api
		protected eCharacter Character { get; init; } = null;
		public RectInt LocalBounds { get; private set; } = new(-Const.CELL_SIZE / 2, 0, Const.CELL_SIZE, Const.CELL_SIZE);


		#endregion




		#region --- MSG ---


		public CharacterRenderer (eCharacter ch) => Character = ch;


		public virtual void FrameUpdate (CharacterPose pose) {

			if (pose.FacingFront) {
				// Front

				DrawTail(pose);
				DrawHair(pose, false);

				DrawArm(pose, !pose.FacingRight);
				DrawHand(pose, !pose.FacingRight);
				DrawLeg(pose, !pose.FacingRight);
				DrawFoot(pose, !pose.FacingRight);

				DrawBody(pose);
				DrawBoingBoing(pose);
				DrawHead(pose);
				DrawFace(pose);
				DrawHair(pose, true);

				DrawArm(pose, pose.FacingRight);
				DrawHand(pose, pose.FacingRight);
				DrawLeg(pose, pose.FacingRight);
				DrawFoot(pose, pose.FacingRight);

			} else {
				// Back

				DrawArm(pose, !pose.FacingRight);
				DrawHand(pose, !pose.FacingRight);
				DrawLeg(pose, !pose.FacingRight);
				DrawFoot(pose, !pose.FacingRight);

				DrawHair(pose, true);
				DrawFace(pose);
				DrawHead(pose);
				DrawBoingBoing(pose);
				DrawBody(pose);

				DrawArm(pose, pose.FacingRight);
				DrawHand(pose, pose.FacingRight);
				DrawLeg(pose, pose.FacingRight);
				DrawFoot(pose, pose.FacingRight);

				DrawHair(pose, false);
				DrawTail(pose);
			}

		}


		#endregion




		#region --- API ---


		protected virtual void DrawHair (CharacterPose pose, bool front) { }
		protected virtual void DrawHead (CharacterPose pose) { }
		protected virtual void DrawFace (CharacterPose pose) { }
		protected virtual void DrawBody (CharacterPose pose) { }
		protected virtual void DrawBoingBoing (CharacterPose pose) { }
		protected virtual void DrawTail (CharacterPose pose) { }
		protected virtual void DrawArm (CharacterPose pose, bool right) { }
		protected virtual void DrawHand (CharacterPose pose, bool right) { }
		protected virtual void DrawLeg (CharacterPose pose, bool right) { }
		protected virtual void DrawFoot (CharacterPose pose, bool right) { }


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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class CharacterRenderer {




		#region --- VAR ---


		// Init
		public int Width { get; init; } = 256;
		public int Height { get; init; } = 256;
		public int SqrtHeight { get; init; } = 158;

		// Api
		public bool Squating { get; set; } = false;
		public bool FacingFront { get; set; } = true;
		public bool FacingRight { get; set; } = true;
		protected int CurrentFrame { get; private set; } = 0;

		// Data
		protected eRigidbody Rig { get; private set; } = null;


		#endregion




		#region --- MSG ---


		public CharacterRenderer (eCharacter ch) {
			Rig = ch;
		}


		public void FrameUpdate (int frame) {

			CellRenderer.Draw(
				"Test Pump".AngeHash(),
				Rig.X + Rig.OffsetX + Rig.Width / 2,
				Rig.Y + Rig.OffsetY,
				500, 0, 0,
				FacingRight ? Width : -Width,
				!Squating ? Height : SqrtHeight,
				Color.white
			);

			CurrentFrame = frame;

			if (FacingFront) {
				// Front

				DrawTail();
				DrawHair(false);

				DrawArm(!FacingRight);
				DrawHand(!FacingRight);
				DrawLeg(!FacingRight);
				DrawFoot(!FacingRight);

				DrawBody();
				DrawBoingBoing();
				DrawHead();
				DrawFace();
				DrawHair(true);

				DrawArm(FacingRight);
				DrawHand(FacingRight);
				DrawLeg(FacingRight);
				DrawFoot(FacingRight);

			} else {
				// Back

				DrawArm(!FacingRight);
				DrawHand(!FacingRight);
				DrawLeg(!FacingRight);
				DrawFoot(!FacingRight);

				DrawHair(true);
				DrawFace();
				DrawHead();
				DrawBoingBoing();
				DrawBody();

				DrawArm(FacingRight);
				DrawHand(FacingRight);
				DrawLeg(FacingRight);
				DrawFoot(FacingRight);

				DrawHair(false);
				DrawTail();
			}

		}


		#endregion




		#region --- API ---


		protected virtual void DrawHair (bool front) { }
		protected virtual void DrawHead () { }
		protected virtual void DrawFace () { }
		protected virtual void DrawBody () { }
		protected virtual void DrawBoingBoing () { }
		protected virtual void DrawTail () { }
		protected virtual void DrawArm (bool right) { }
		protected virtual void DrawHand (bool right) { }
		protected virtual void DrawLeg (bool right) { }
		protected virtual void DrawFoot (bool right) { }


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

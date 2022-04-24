using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class CharacterRenderer {




		#region --- VAR ---


		// Init
		protected eCharacter Character { get; init; } = null;
		public int Width { get; init; } = Const.CELL_SIZE;
		public int Height { get; init; } = Const.CELL_SIZE * 2;

		// Api
		public virtual RectInt LocalBounds => new(-Width / 2, 0, Width, Height);


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




		#region --- OVR ---


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
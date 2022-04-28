using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class CharacterRendererConfig {
		public int Head { get; init; } = 0;
		public int HairF { get; init; } = 0;
		public int HairB { get; init; } = 0;
		public int BodyF { get; init; } = 0;
		public int BodyB { get; init; } = 0;
		public int ArmL { get; init; } = 0;
		public int ArmR { get; init; } = 0;
		public int LegL { get; init; } = 0;
		public int LegR { get; init; } = 0;
	}


	public abstract class CharacterRenderer {




		#region --- VAR ---


		// Init
		protected eCharacter Character { get; init; } = null;
		public CharacterRendererConfig[] Configs { get; init; } = null;

		// Api
		public virtual RectInt LocalBounds => new(-Width / 2, 0, Width, Height);
		public int ConfigIndex { get; set; } = 0;

		// Data
		private int Width = Const.CELL_SIZE;
		private int Height = Const.CELL_SIZE * 2;


		#endregion




		#region --- MSG ---


		public CharacterRenderer (eCharacter ch) => Character = ch;


		public virtual void FrameUpdate (int frame) {

			if (Configs == null || Configs.Length == 0) return;

			var config = Configs[ConfigIndex % Configs.Length];

			// Calculate Part Size





			// Draw
			if (Character.Movement.FacingFront) {
				// Front

				Draw_Back();
				DrawHair(false);

				DrawArm(!Character.Movement.FacingRight);
				DrawLeg(!Character.Movement.FacingRight);

				DrawBody();
				DrawHead();
				DrawFace();
				DrawHair(true);

				DrawArm(Character.Movement.FacingRight);
				DrawLeg(Character.Movement.FacingRight);
				Draw_Front();

			} else {
				// Back

				Draw_Front();
				DrawArm(!Character.Movement.FacingRight);
				DrawLeg(!Character.Movement.FacingRight);

				DrawHair(true);
				DrawFace();
				DrawHead();
				DrawBody();

				DrawArm(Character.Movement.FacingRight);
				DrawLeg(Character.Movement.FacingRight);

				DrawHair(false);
				Draw_Back();
			}

		}


		#endregion




		#region --- API ---


		// Body-Part
		protected virtual void DrawHair (bool front) {

		}


		protected virtual void DrawHead () {


		}


		protected virtual void DrawFace () { }


		protected virtual void DrawBody () {

		}


		protected virtual void DrawArm (bool right) {

		}


		protected virtual void DrawLeg (bool right) {

		}


		// Extra
		protected virtual void Draw_Front () { }


		protected virtual void Draw_Back () { }


		// Misc
		public void Reset () {
			ConfigIndex = 0;
			Width = Const.CELL_SIZE;
			Height = Const.CELL_SIZE * 2;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Navigation {




		#region --- VAR ---


		// Api
		public eCharacter Source { get; private set; } = null;

		// Ser
		[SerializeField] bool AllowCheat = true;
		[SerializeField] int AnalyzeFrequency = 30;

		// Data
		private int TargetX = 0;
		private int TargetY = 0;
		private int LastAnalyzeFrame = int.MinValue;


		#endregion




		#region --- API ---


		public void OnInitialize (eCharacter source) {
			Source = source;
		}


		public void OnActived () {
			TargetX = Source.X;
			TargetY = Source.Y;
		}


		public void SetTargetPosition (int x, int y) {
			TargetX = x;
			TargetY = y;
			Analyze();
		}


		public void Navigate () {
			if (Game.GlobalFrame >= LastAnalyzeFrame + AnalyzeFrequency) {
				Analyze();
			}
			// Source.X, Source.Y >> TargetX, TargetY
			// AllowCheat

			//if (Game.GlobalFrame % 60 == 0) Source.Movement.Jump();
			//Source.Movement.HoldJump(true);

		}


		public void CheatNavigate () {




		}


		#endregion




		#region --- LGC ---


		private void Analyze () {
			LastAnalyzeFrame = Game.GlobalFrame;



		}


		#endregion




	}
}

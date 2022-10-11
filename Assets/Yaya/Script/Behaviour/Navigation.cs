using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Navigation {




		#region --- SUB ---


		private enum NavigationType { None = 0, Walk, Jump, Cheat, }
		public enum NavigationState { Move, Jump, Fly }


		private class NavigationCell {
			public NavigationType MoveType;
			public int TargetX;
			public int TargetY;
			public int Frame;
		}


		#endregion




		#region --- VAR ---


		// Api
		public eCharacter Source { get; private set; } = null;
		public int TargetX { get; private set; } = 0;
		public int TargetY { get; private set; } = 0;
		public NavigationState State { get; private set; } = NavigationState.Move;

		// Ser
		[SerializeField] bool AllowCheat = true;
		[SerializeField] int AnalyzeFrequency = 30;
		[SerializeField] int JumpHeight = Const.CELL_SIZE * 5;
		[SerializeField] int JumpRange = Const.CELL_SIZE * 6;

		// Data
		private NavigationCell[,] Cells = null;
		private int CellsX = 0;
		private int CellsY = 0;
		private int LastAnalyzeFrame = int.MinValue;
		private int JumpProgress = 0;
		private int JumpDuration = 0;
		private Vector2Int JumppingFrom = default;
		private Vector2Int JumppingTo = default;


		#endregion




		#region --- API ---


		public void OnInitialize (eCharacter source) {
			Source = source;
			Cells = new NavigationCell[CellPhysics.Width, CellPhysics.Height];
			for (int y = 0; y < CellPhysics.Height; y++) {
				for (int x = 0; x < CellPhysics.Width; x++) {
					Cells[x, y] = new NavigationCell() {
						MoveType = NavigationType.None,
						Frame = int.MinValue,
					};
				}
			}
		}


		public void OnActived () {
			TargetX = Source.X;
			TargetY = Source.Y;
			LastAnalyzeFrame = int.MinValue;
			State = NavigationState.Move;
			JumpProgress = -1;
		}


		public void SetTargetPosition (int x, int y) {
			TargetX = x;
			TargetY = y;
			Analyze();
		}


		public void Navigate () {
			// Analyze
			if (Game.GlobalFrame >= LastAnalyzeFrame + AnalyzeFrequency) {
				Analyze();
			}
			// Navigate
			switch (State) {
				case NavigationState.Move:
					// Source.X, Source.Y >> TargetX, TargetY



					break;

				case NavigationState.Jump:
					int x;
					int y;
					JumpProgress = JumpProgress.Clamp(0, JumpDuration);
					if (JumpProgress < JumpDuration) {
						// Jumpping
						int progress = JumpProgress * 1000 / JumpDuration;
						x = JumppingFrom.x.LerpTo(JumppingTo.x, progress);
						y = JumppingFrom.y.LerpTo(JumppingTo.y, progress);
						//y += ;
					} else {
						// End Jump
						JumpProgress = 0;
						x = JumppingTo.x;
						y = JumppingTo.y;
						State = NavigationState.Move;
					}
					Source.X = x;
					Source.Y = y;
					Source.MakeNotGrounded(1);
					JumpProgress++;
					break;

				case NavigationState.Fly:



					break;
			}
		}



		#endregion




		#region --- LGC ---


		private void Analyze () {
			LastAnalyzeFrame = Game.GlobalFrame;
			CellsX = CellPhysics.PositionX;
			CellsY = CellPhysics.PositionY;

			// Physics >> Navigation Cell
			// ToPhysicsCellIndexX




		}


		private void InvokeJump (Vector2Int to, int duration) {
			JumppingFrom.x = Source.X;
			JumppingFrom.y = Source.Y;
			JumppingTo = to;
			JumpProgress = 0;
			JumpDuration = duration.Clamp(0, int.MaxValue);
			Source.Renderer.Bounce();
		}


		// Util
		private int ToPhysicsCellIndexX (int globalX) => (globalX - CellsX + Const.BLOCK_SPAWN_PADDING) / Const.CELL_SIZE;


		private int ToPhysicsCellIndexY (int globalY) => (globalY - CellsY + Const.BLOCK_SPAWN_PADDING) / Const.CELL_SIZE;



		#endregion




	}
}

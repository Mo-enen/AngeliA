using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public class eGamePadUI : UI {

		private static readonly int BodyCode = "GamePad Body".AngeHash();
		private static readonly int DPadDownCode = "GamePad Down".AngeHash();
		private static readonly int DPadUpCode = "GamePad Up".AngeHash();
		private static readonly int DPadLeftCode = "GamePad Left".AngeHash();
		private static readonly int DPadRightCode = "GamePad Right".AngeHash();
		private static readonly int ButtonACode = "GamePad A".AngeHash();
		private static readonly int ButtonBCode = "GamePad B".AngeHash();
		private static readonly int ButtonSelectCode = "GamePad Select".AngeHash();
		private static readonly int ButtonStartCode = "GamePad Start".AngeHash();

		public Color32 PressingTint = new();
		public Color32 DarkButtonTint = new();
		public Color32 ColorfulButtonTint = new();

		public RectInt DPadLeftPosition = default;
		public RectInt DPadRightPosition = default;
		public RectInt DPadDownPosition = default;
		public RectInt DPadUpPosition = default;
		public RectInt SelectPosition = default;
		public RectInt StartPosition = default;
		public RectInt ButtonAPosition = default;
		public RectInt ButtonBPosition = default;



		public override void OnActived () {
			base.OnActived();
			ColorfulButtonTint = new(240, 86, 86, 255);
			DarkButtonTint = new(0, 0, 0, 255);
			PressingTint = new(0, 255, 0, 255);
		}


		protected override void FrameUpdateUI () {

			if (FrameTask.IsTasking<DialoguePerformer>(Const.TASK_ROUTE)) return;
			
			X = 6 * UNIT;
			Y = 6 * UNIT;
			Width = 132 * UNIT;
			Height = 60 * UNIT;
			DPadLeftPosition = new(10 * UNIT, 22 * UNIT, 12 * UNIT, 8 * UNIT);
			DPadRightPosition = new(22 * UNIT, 22 * UNIT, 12 * UNIT, 8 * UNIT);
			DPadDownPosition = new(18 * UNIT, 14 * UNIT, 8 * UNIT, 12 * UNIT);
			DPadUpPosition = new(18 * UNIT, 26 * UNIT, 8 * UNIT, 12 * UNIT);
			SelectPosition = new(44 * UNIT, 20 * UNIT, 12 * UNIT, 4 * UNIT);
			StartPosition = new(60 * UNIT, 20 * UNIT, 12 * UNIT, 4 * UNIT);
			ButtonAPosition = new(106 * UNIT, 18 * UNIT, 12 * UNIT, 12 * UNIT);
			ButtonBPosition = new(86 * UNIT, 18 * UNIT, 12 * UNIT, 12 * UNIT);

			var screenRect = CellRenderer.CameraRect;

			// Body
			CellRenderer.Draw(BodyCode, base.Rect.Shift(screenRect.x, screenRect.y));

			// DPad
			CellRenderer.Draw(DPadLeftCode, DPadLeftPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Left) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadRightCode, DPadRightPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Right) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadDownCode, DPadDownPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Down) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadUpCode, DPadUpPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Up) ? PressingTint : DarkButtonTint);

			// Func
			CellRenderer.Draw(ButtonSelectCode, SelectPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Select) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(ButtonStartCode, StartPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Start) ? PressingTint : DarkButtonTint);

			// Buttons
			CellRenderer.Draw(ButtonACode, ButtonAPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Jump) ? PressingTint : ColorfulButtonTint);
			CellRenderer.Draw(ButtonBCode, ButtonBPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Action) ? PressingTint : ColorfulButtonTint);

		}


	}
}

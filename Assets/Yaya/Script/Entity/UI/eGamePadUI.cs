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



		protected override void UpdateForUI () {

			var screenRect = CellRenderer.CameraRect;

			// Body
			CellRenderer.Draw(BodyCode, base.Rect.Shift(screenRect.x, screenRect.y));

			// DPad
			CellRenderer.Draw(DPadLeftCode, DPadLeftPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GetKey(GameKey.Left) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadRightCode, DPadRightPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GetKey(GameKey.Right) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadDownCode, DPadDownPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GetKey(GameKey.Down) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadUpCode, DPadUpPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GetKey(GameKey.Up) ? PressingTint : DarkButtonTint);

			// Func
			CellRenderer.Draw(ButtonSelectCode, SelectPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GetKey(GameKey.Select) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(ButtonStartCode, StartPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GetKey(GameKey.Start) ? PressingTint : DarkButtonTint);

			// Buttons
			CellRenderer.Draw(ButtonACode, ButtonAPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GetKey(GameKey.Jump) ? PressingTint : ColorfulButtonTint);
			CellRenderer.Draw(ButtonBCode, ButtonBPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GetKey(GameKey.Action) ? PressingTint : ColorfulButtonTint);

		}


	}
}

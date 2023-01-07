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

		public static readonly Color32 DirectionTint = new(255, 255, 0, 255);
		public static readonly Color32 PressingTint = new(0, 255, 0, 255);
		public static readonly Color32 DarkButtonTint = new(0, 0, 0, 255);
		public static readonly Color32 ColorfulButtonTint = new(240, 86, 86, 255);


		// MSG
		protected override void FrameUpdateUI () {

			if (FrameTask.IsTasking(Const.TASK_ROUTE)) return;

			Width = 132 * UNIT;
			Height = 60 * UNIT;
			X = 6 * UNIT;
			Y = 6 * UNIT;
			var DPadLeftPosition = new RectInt(10 * UNIT, 22 * UNIT, 12 * UNIT, 8 * UNIT);
			var DPadRightPosition = new RectInt(22 * UNIT, 22 * UNIT, 12 * UNIT, 8 * UNIT);
			var DPadDownPosition = new RectInt(18 * UNIT, 14 * UNIT, 8 * UNIT, 12 * UNIT);
			var DPadUpPosition = new RectInt(18 * UNIT, 26 * UNIT, 8 * UNIT, 12 * UNIT);
			var DPadCenterPos = new Vector2Int(22 * UNIT, 26 * UNIT);
			var SelectPosition = new RectInt(44 * UNIT, 20 * UNIT, 12 * UNIT, 4 * UNIT);
			var StartPosition = new RectInt(60 * UNIT, 20 * UNIT, 12 * UNIT, 4 * UNIT);
			var ButtonAPosition = new RectInt(106 * UNIT, 18 * UNIT, 12 * UNIT, 12 * UNIT);
			var ButtonBPosition = new RectInt(86 * UNIT, 18 * UNIT, 12 * UNIT, 12 * UNIT);

			var screenRect = CellRenderer.CameraRect;

			// Body
			CellRenderer.Draw(BodyCode, Rect.Shift(screenRect.x, screenRect.y));

			// DPad
			CellRenderer.Draw(DPadLeftCode, DPadLeftPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Left) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadRightCode, DPadRightPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Right) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadDownCode, DPadDownPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Down) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(DPadUpCode, DPadUpPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Up) ? PressingTint : DarkButtonTint);

			// Direction
			if (FrameInput.UsingLeftStick) {
				var nDir = FrameInput.Direction;
				CellRenderer.Draw(
					Const.PIXEL, DPadCenterPos.x + X + screenRect.x, DPadCenterPos.y + Y + screenRect.y,
					500, 0, (int)Vector3.SignedAngle(Vector3.up, (Vector2)nDir, Vector3.back),
					3 * UNIT, (int)nDir.magnitude * UNIT / 50, DirectionTint
				);
			}

			// Func
			CellRenderer.Draw(ButtonSelectCode, SelectPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Select) ? PressingTint : DarkButtonTint);
			CellRenderer.Draw(ButtonStartCode, StartPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Start) ? PressingTint : DarkButtonTint);

			// Buttons
			CellRenderer.Draw(ButtonACode, ButtonAPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Action) ? PressingTint : ColorfulButtonTint);
			CellRenderer.Draw(ButtonBCode, ButtonBPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), FrameInput.GameKeyPress(GameKey.Jump) ? PressingTint : ColorfulButtonTint);

		}


	}
}

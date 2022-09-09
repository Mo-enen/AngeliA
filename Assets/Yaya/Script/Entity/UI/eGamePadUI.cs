using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.EntityCapacity(1)]
	public class eGamePadUI : ScreenUI {

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

			var screenRect = AngeliaFramework.Renderer.CameraRect;

            // Body
            AngeliaFramework.Renderer.Draw(BodyCode, base.Rect.Shift(screenRect.x, screenRect.y));

            // DPad
            AngeliaFramework.Renderer.Draw(DPadLeftCode, DPadLeftPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), AngeliaFramework.Input.KeyPressing(GameKey.Left) ? PressingTint : DarkButtonTint);
            AngeliaFramework.Renderer.Draw(DPadRightCode, DPadRightPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), AngeliaFramework.Input.KeyPressing(GameKey.Right) ? PressingTint : DarkButtonTint);
            AngeliaFramework.Renderer.Draw(DPadDownCode, DPadDownPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), AngeliaFramework.Input.KeyPressing(GameKey.Down) ? PressingTint : DarkButtonTint);
            AngeliaFramework.Renderer.Draw(DPadUpCode, DPadUpPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), AngeliaFramework.Input.KeyPressing(GameKey.Up) ? PressingTint : DarkButtonTint);

            // Func
            AngeliaFramework.Renderer.Draw(ButtonSelectCode, SelectPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), AngeliaFramework.Input.KeyPressing(GameKey.Select) ? PressingTint : DarkButtonTint);
            AngeliaFramework.Renderer.Draw(ButtonStartCode, StartPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), AngeliaFramework.Input.KeyPressing(GameKey.Start) ? PressingTint : DarkButtonTint);

            // Buttons
            AngeliaFramework.Renderer.Draw(ButtonACode, ButtonAPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), AngeliaFramework.Input.KeyPressing(GameKey.Jump) ? PressingTint : ColorfulButtonTint);
            AngeliaFramework.Renderer.Draw(ButtonBCode, ButtonBPosition.Shift(X, Y).Shift(screenRect.x, screenRect.y), AngeliaFramework.Input.KeyPressing(GameKey.Action) ? PressingTint : ColorfulButtonTint);

		}


	}
}

using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace AngeliaGame {
	public sealed class GameForUnity : Game {




		#region --- VAR ---


		// Api
		public Camera UnityCamera { get; init; }


		#endregion




		#region --- MSG ---


		public GameForUnity (GameConfiguration config) : base(config) {
			UnityCamera = Camera.main;
		}


		#endregion




		#region --- API ---


		// System
		public override void SetTargetFramerate (int targetFramerate) => Application.targetFrameRate = targetFramerate;
		public override void SetVSync (bool vsync) => QualitySettings.vSyncCount = vsync ? 1 : 0;
		public override void SetFullscreenMode (FullscreenMode mode) {
			switch (mode) {
				case FullscreenMode.Window:
					Screen.SetResolution(
						Display.main.systemWidth * 2 / 3, Display.main.systemHeight * 2 / 3, false
					);
					break;
				case FullscreenMode.Fullscreen:
					Screen.SetResolution(
						Display.main.systemWidth, Display.main.systemHeight, true
					);
					break;
				case FullscreenMode.FullscreenLow:
					Screen.SetResolution(
						Display.main.systemWidth / 2, Display.main.systemHeight / 2, true
					);
					break;
			}
		}

		// Camera
		public override FRect GetCameraScreenLocacion () => UnityCamera.rect;
		public override void SetCameraScreenLocacion (FRect rect) => UnityCamera.rect = rect;
		public override float GetCameraAspect () => UnityCamera.aspect;
		public override float GetCameraOrthographicSize () => UnityCamera.orthographicSize;


		#endregion




		#region --- LGC ---



		#endregion




	}
}

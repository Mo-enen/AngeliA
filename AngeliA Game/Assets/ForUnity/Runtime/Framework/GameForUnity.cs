using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


[assembly: AngeliA]
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaForUnity {
	public sealed partial class GameForUnity : Game {



		#region --- VAR ---


		private static GameForUnity InstanceUnity = null;


		#endregion




		#region --- MSG ---


		public override void Initialize () {
			BeforeGameInitialize_Rendering();
			base.Initialize();
			if (Application.isEditor) MapEditor.OpenMapEditorSmoothly(false);
		}


		public GameForUnity () => InstanceUnity = this;


		#endregion




		#region --- API ---


		// System
		protected override bool GetIsEdittime () => Application.isEditor;
		protected override void SetGraphicFramerate (int targetFramerate) => Application.targetFrameRate = targetFramerate;
		protected override void SetVSync (bool vsync) => QualitySettings.vSyncCount = vsync ? 1 : 0;
		protected override void SetFullscreenMode (FullscreenMode mode) {
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
		protected override int GetScreenWidth () => Screen.width;
		protected override int GetScreenHeight () => Screen.height;

		// Debug
		protected override void DebugLog (object target) => Debug.Log(target);
		protected override void DebugLogWarning (object target) => Debug.LogWarning(target);
		protected override void DebugLogError (object target) => Debug.LogError(target);
		protected override void DebugLogException (System.Exception ex) => Debug.LogException(ex);
		protected override void SetDebugLoggerEnable (bool enable) => Debug.unityLogger.logEnabled = enable;
		protected override bool GetDebugLoggerEnable () => Debug.unityLogger.logEnabled;

		// Sheet
		protected override object LoadSheetTextureFromDisk () => AngeUtilUnity.LoadTexture(AngePath.SheetTexturePath);


		#endregion




		#region --- LGC ---





		#endregion




	}
}

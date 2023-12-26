using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: AngeliA]
namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaForUnity {
	public sealed partial class GameForUnity : Game {



		#region --- VAR ---


		// Api
		public Camera UnityCamera => _UnityCamera != null ? _UnityCamera : (_UnityCamera = GetOrCreateCamera());
		private Camera _UnityCamera = null;
		public Font[] Fonts { get; init; } = new Font[0];
		public AudioClip[] AudioClips { get; init; } = new AudioClip[0];
		public Texture2D[] Cursors { get; init; } = new Texture2D[0];
		public Vector2[] CursorPivots { get; init; } = new Vector2[0];

		// Data
		private static GameForUnity InstanceUnity = null;


		#endregion




		#region --- MSG ---


		public override void Initialize () {
			InitializeRendering();
			InitializeAudio();
			Initialize_Input();
			base.Initialize();
			if (Application.isEditor) MapEditor.OpenMapEditorSmoothly(false);
		}


		public GameForUnity () => InstanceUnity = this;


		#endregion




		#region --- API ---


		// System
		protected override bool _GetIsEdittime () => Application.isEditor;
		protected override void _SetGraphicFramerate (int targetFramerate) => Application.targetFrameRate = targetFramerate;
		protected override void _SetVSync (bool vsync) => QualitySettings.vSyncCount = vsync ? 1 : 0;
		protected override void _SetFullscreenMode (FullscreenMode mode) {
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
		protected override int _GetScreenWidth () => Screen.width;
		protected override int _GetScreenHeight () => Screen.height;
		protected override void _QuitApplication () => Application.Quit();


		// Listener
		protected override void _AddGameQuittingListener (System.Action callback) {
			Application.quitting -= callback;
			Application.quitting += callback;
		}
		protected override void _AddGameTryingToQuitListener (System.Func<bool> callback) {
			Application.wantsToQuit -= callback;
			Application.wantsToQuit += callback;
		}
		protected override void _AddTextInputListener (System.Action<char> callback) {
			if (Keyboard.current != null) {
				Keyboard.current.onTextInput -= callback;
				Keyboard.current.onTextInput += callback;
			}
		}


		// Debug
		protected override void _Log (object target) => Debug.Log(target);
		protected override void _LogWarning (object target) => Debug.LogWarning(target);
		protected override void _LogError (object target) => Debug.LogError(target);
		protected override void _LogException (System.Exception ex) => Debug.LogException(ex);
		protected override void _SetDebugEnable (bool enable) => Debug.unityLogger.logEnabled = enable;
		protected override bool _GetDebugEnable () => Debug.unityLogger.logEnabled;


		#endregion




	}
}

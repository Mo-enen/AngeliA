using System;
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
		public AudioClip[] AudioClips { get; init; } = new AudioClip[0];
		public Texture2D[] Cursors { get; init; } = new Texture2D[0];
		public Vector2[] CursorPivots { get; init; } = new Vector2[0];
		public Font[] Fonts { get; init; } = null;

		// Data
		private static GameForUnity InstanceUnity = null;


		#endregion




		#region --- MSG ---


		public override void Initialize () {
			InitializeRendering();
			InitializeAudio();
			Initialize_Input();
			base.Initialize();
		}


		public GameForUnity () => InstanceUnity = this;


		#endregion




		#region --- API ---


		// System
		protected override bool _GetIsEdittime () => Application.isEditor;
		protected override void _SetGraphicFramerate (int targetFramerate) => Application.targetFrameRate = targetFramerate;
		protected override void _SetFullscreen (bool fullScreen) {
			if (fullScreen == Screen.fullScreen) return;
			if (fullScreen) {
				Screen.SetResolution(
					Display.main.systemWidth, Display.main.systemHeight, true
				);
			} else {
				Screen.SetResolution(
					Display.main.systemWidth * 2 / 3, Display.main.systemHeight * 2 / 3, false
				);
			}
		}
		protected override int _GetScreenWidth () => Screen.width;
		protected override int _GetScreenHeight () => Screen.height;
		protected override void _QuitApplication () {
			Application.Quit();
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
		protected override void _UnloadTexture (object texture) { }
		protected override void _SetWindowSize (int width, int height) { }


		// Listener
		protected override void _AddGameQuittingCallback (Action callback) {
			Application.quitting -= callback;
			Application.quitting += callback;
		}
		protected override void _AddGameTryingToQuitCallback (Func<bool> callback) {
			Application.wantsToQuit -= callback;
			Application.wantsToQuit += callback;
		}
		protected override void _AddTextInputCallback (Action<char> callback) {
			if (Keyboard.current == null) return;
			Keyboard.current.onTextInput -= callback;
			Keyboard.current.onTextInput += callback;
		}
		protected override void _AddFocusChangedCallback (Action<bool> callback) {
			Application.focusChanged -= callback;
			Application.focusChanged += callback;
		}


		// Debug
		protected override void _Log (object target) => Debug.Log(target);
		protected override void _LogWarning (object target) => Debug.LogWarning(target);
		protected override void _LogError (object target) => Debug.LogError(target);
		protected override void _LogException (System.Exception ex) => Debug.LogException(ex);


		#endregion




	}
}

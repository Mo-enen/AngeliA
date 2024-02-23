using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;


namespace AngeliaPlayer.Framework;

public partial class RayGame {


	// System
	protected override bool _GetIsEdittime () {
#if DEBUG
		return true;
#else
		return false;
#endif
	}

	protected override void _SetFullscreen (bool fullScreen) {
		if (Raylib.IsWindowFullscreen() == fullScreen) return;
		Raylib.ToggleFullscreen();
	}

	protected override int _GetScreenWidth () => Raylib.GetRenderWidth();

	protected override int _GetScreenHeight () => Raylib.GetRenderHeight();

	protected override void _QuitApplication () => RequireQuitGame = true;

	protected override void _SetWindowSize (int width, int height) {
		if (Raylib.IsWindowFullscreen()) return;
		Raylib.SetWindowSize(width, height);
	}


	// Listener
	protected override void _AddGameQuittingCallback (System.Action callback) {
		OnGameQuitting -= callback;
		OnGameQuitting += callback;
	}

	protected override void _AddGameTryingToQuitCallback (System.Func<bool> callback) {
		OnGameTryingToQuit -= callback;
		OnGameTryingToQuit += callback;
	}

	protected override void _AddTextInputCallback (System.Action<char> callback) {
		OnTextInput -= callback;
		OnTextInput += callback;
	}

	protected override void _AddFocusChangedCallback (System.Action<bool> callback) {
		OnWindowFocusChanged -= callback;
		OnWindowFocusChanged += callback;
	}


}


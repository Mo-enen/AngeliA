using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;


namespace AngeliaToRaylib.Framework;

public partial class RaylibGame {


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


	// Debug
	protected override void _Log (object target) {
		if (!IsEdittime || target == null) return;
		System.Console.ResetColor();
		System.Console.WriteLine(target.ToString());
	}

	protected override void _LogWarning (object target) {
		if (!IsEdittime || target == null) return;
		System.Console.ForegroundColor = System.ConsoleColor.Yellow;
		System.Console.WriteLine(target.ToString());
		System.Console.ResetColor();
	}

	protected override void _LogError (object target) {
		if (!IsEdittime || target == null) return;
		System.Console.ForegroundColor = System.ConsoleColor.Red;
		System.Console.WriteLine(target.ToString());
		System.Console.ResetColor();
	}

	protected override void _LogException (System.Exception ex) {
		if (!IsEdittime) return;
		System.Console.ForegroundColor = System.ConsoleColor.Red;
		System.Console.WriteLine(ex.Source);
		System.Console.WriteLine(ex.GetType().Name);
		System.Console.WriteLine(ex.Message);
		System.Console.WriteLine();
		System.Console.ResetColor();
	}


}


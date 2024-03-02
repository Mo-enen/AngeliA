using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;


namespace AngeliaRuntime.Framework;

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


	// Window
	protected override void _SetWindowSize (int width, int height) {
		if (Raylib.IsWindowFullscreen()) return;
		Raylib.SetWindowSize(width, height);
	}

	protected override Int2 _GetWindowPosition () {
		var pos = Raylib.GetWindowPosition();
		return new((int)pos.X, (int)pos.Y);
	}

	protected override void _SetWindowPosition (int x, int y) => Raylib.SetWindowPosition(x, y);

	protected override bool _IsWindowFocused () => Raylib.IsWindowFocused();

	protected override int _GetCurrentMonitor () => Raylib.GetCurrentMonitor();

	protected override int _GetMonitorWidth (int monitor) => Raylib.GetMonitorWidth(monitor);

	protected override int _GetMonitorHeight (int monitor) => Raylib.GetMonitorHeight(monitor);

	protected override void _SetWindowDecorated (bool decorated) {
		if (decorated) {
			Raylib.ClearWindowState(ConfigFlags.UndecoratedWindow);
		} else {
			Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
		}
	}

	protected override void _SetWindowMaximized (bool maximized) {
		if (maximized) {
			Raylib.SetWindowState(ConfigFlags.MaximizedWindow);
		} else {
			Raylib.ClearWindowState(ConfigFlags.MaximizedWindow);
		}
	}

	protected override void _SetWindowResizable (bool resizable) {
		if (resizable) {
			Raylib.SetWindowState(ConfigFlags.ResizableWindow);
		} else {
			Raylib.ClearWindowState(ConfigFlags.ResizableWindow);
		}
	}

	protected override void _SetWindowTopmost (bool topmost) {
		if (topmost) {
			Raylib.SetWindowState(ConfigFlags.TopmostWindow);
		} else {
			Raylib.ClearWindowState(ConfigFlags.TopmostWindow);
		}
	}

	protected override bool _GetWindowDecorated () => !Raylib.IsWindowState(ConfigFlags.UndecoratedWindow);

	protected override bool _GetWindowMaximized () => Raylib.IsWindowState(ConfigFlags.MaximizedWindow);

	protected override bool _GetWindowResizable () => Raylib.IsWindowState(ConfigFlags.ResizableWindow);

	protected override bool _GetWindowTopmost () => Raylib.IsWindowState(ConfigFlags.TopmostWindow);

	protected override void _SetWindowTitle (string title) => Raylib.SetWindowTitle(title);


}


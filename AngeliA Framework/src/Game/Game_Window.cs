using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class Game {

	public static bool IsWindowFocused => Instance._IsWindowFocused();
	protected abstract bool _IsWindowFocused ();

	public static void MakeWindowFocused () => Instance._MakeWindowFocused();
	protected abstract void _MakeWindowFocused ();

	public static void SetWindowPosition (int x, int y) => Instance._SetWindowPosition(x, y);
	protected abstract void _SetWindowPosition (int x, int y);

	public static Int2 GetWindowPosition () => Instance._GetWindowPosition();
	protected abstract Int2 _GetWindowPosition ();

	public static void SetWindowSize (int x, int y) => Instance._SetWindowSize(x, y);
	protected abstract void _SetWindowSize (int x, int y);

	public static int CurrentMonitor => Instance._GetCurrentMonitor();
	protected abstract int _GetCurrentMonitor ();

	public static int GetMonitorWidth (int monitor) => Instance._GetMonitorWidth(monitor);
	protected abstract int _GetMonitorWidth (int monitor);

	public static int GetMonitorHeight (int monitor) => Instance._GetMonitorHeight(monitor);
	protected abstract int _GetMonitorHeight (int monitor);

	public static bool IsWindowDecorated {
		get => Instance._GetWindowDecorated();
		set => Instance._SetWindowDecorated(value);
	}
	protected abstract bool _GetWindowDecorated ();
	protected abstract void _SetWindowDecorated (bool decorated);

	public static bool IsWindowTopmost {
		get => Instance._GetWindowTopmost();
		set => Instance._SetWindowTopmost(value);
	}
	protected abstract bool _GetWindowTopmost ();
	protected abstract void _SetWindowTopmost (bool topmost);

	public static bool IsWindowResizable {
		get => Instance._GetWindowResizable();
		set => Instance._SetWindowResizable(value);
	}
	protected abstract bool _GetWindowResizable ();
	protected abstract void _SetWindowResizable (bool resizable);

	public static bool IsWindowMaximized {
		get => Instance._GetWindowMaximized();
		set => Instance._SetWindowMaximized(value);
	}
	protected abstract bool _GetWindowMaximized ();
	protected abstract void _SetWindowMaximized (bool maximized);

	public static bool IsWindowMinimized {
		get => Instance._GetWindowMinimized();
		set => Instance._SetWindowMinimized(value);
	}
	protected abstract bool _GetWindowMinimized ();
	protected abstract void _SetWindowMinimized (bool minimized);

	public static void SetWindowTitle (string title) => Instance._SetWindowTitle(title);
	protected abstract void _SetWindowTitle (string title);

	public static void SetWindowIcon (int spriteID) => Instance._SetWindowIcon(spriteID);
	protected abstract void _SetWindowIcon (int spriteID);

	public static void SetWindowMinSize (int size) => Instance._SetWindowMinSize(size);
	protected abstract void _SetWindowMinSize (int size);

	public static void SetEventWaiting (bool enable) => Instance._SetEventWaiting(enable);
	protected abstract void _SetEventWaiting (bool enable);

}

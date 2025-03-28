using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class Game {


	/// <inheritdoc cref="_IsWindowFocused"/>
	public static bool IsWindowFocused => Instance._IsWindowFocused();
	/// <summary>
	/// True if the application window is currently focused
	/// </summary>
	protected abstract bool _IsWindowFocused ();


	/// <inheritdoc cref="_MakeWindowFocused"/>
	public static void MakeWindowFocused () => Instance._MakeWindowFocused();
	/// <summary>
	/// Make the application window focused
	/// </summary>
	protected abstract void _MakeWindowFocused ();


	/// <inheritdoc cref="_SetWindowPosition"/>
	public static void SetWindowPosition (int x, int y) => Instance._SetWindowPosition(x, y);
	/// <summary>
	/// Set application window position. (0,0) is top-left corner
	/// </summary>
	protected abstract void _SetWindowPosition (int x, int y);


	/// <inheritdoc cref="_GetWindowPosition"/>
	public static Int2 GetWindowPosition () => Instance._GetWindowPosition();
	/// <summary>
	/// Get application window position. (0,0) is top-left corner
	/// </summary>
	/// <returns></returns>
	protected abstract Int2 _GetWindowPosition ();


	/// <inheritdoc cref="_SetWindowSize"/>
	public static void SetWindowSize (int x, int y) => Instance._SetWindowSize(x, y);
	/// <summary>
	/// Set application window size. Use Game.GetMonitorWidth and Game.GetMonitorHeight to get the size of screen size
	/// </summary>
	protected abstract void _SetWindowSize (int x, int y);


	/// <inheritdoc cref="_GetCurrentMonitor"/>
	public static int CurrentMonitor => Instance._GetCurrentMonitor();
	/// <summary>
	/// Get index of the monitor which this application window currently in
	/// </summary>
	protected abstract int _GetCurrentMonitor ();


	/// <inheritdoc cref="_GetMonitorWidth"/>
	public static int GetMonitorWidth (int monitor) => Instance._GetMonitorWidth(monitor);
	/// <summary>
	/// Get width of the current monitor. Use Game.CurrentMonitor to get the monitor index
	/// </summary>
	protected abstract int _GetMonitorWidth (int monitor);


	/// <inheritdoc cref="_GetMonitorHeight"/>
	public static int GetMonitorHeight (int monitor) => Instance._GetMonitorHeight(monitor);
	/// <summary>
	/// Get height of the current monitor. Use Game.CurrentMonitor to get the monitor index
	/// </summary>
	protected abstract int _GetMonitorHeight (int monitor);


	/// <summary>
	/// If the application window has title bar and border frame or not
	/// </summary>
	public static bool IsWindowDecorated {
		get => Instance._GetWindowDecorated();
		set => Instance._SetWindowDecorated(value);
	}
	/// <summary>
	/// True if the application window is currently having Title bar and border frame
	/// </summary>
	protected abstract bool _GetWindowDecorated ();
	/// <summary>
	/// Set to true to make the application window have title bar and border frame
	/// </summary>
	protected abstract void _SetWindowDecorated (bool decorated);


	/// <summary>
	/// If the application window renders on top of all other windows
	/// </summary>
	public static bool IsWindowTopmost {
		get => Instance._GetWindowTopmost();
		set => Instance._SetWindowTopmost(value);
	}
	/// <summary>
	/// True if the application window renders on top of all other windows
	/// </summary>
	protected abstract bool _GetWindowTopmost ();
	/// <summary>
	/// Set to true to make the application window renders on top of all other windows
	/// </summary>
	protected abstract void _SetWindowTopmost (bool topmost);

	/// <summary>
	/// If the application window can be resize by the user
	/// </summary>
	public static bool IsWindowResizable {
		get => Instance._GetWindowResizable();
		set => Instance._SetWindowResizable(value);
	}
	/// <summary>
	/// True if the application window can be resize by the user
	/// </summary>
	protected abstract bool _GetWindowResizable ();
	/// <summary>
	/// Set to true to make the application window can be resize by the user
	/// </summary>
	protected abstract void _SetWindowResizable (bool resizable);


	/// <summary>
	/// If the application window is currently maximized
	/// </summary>
	public static bool IsWindowMaximized {
		get => Instance._GetWindowMaximized();
		set => Instance._SetWindowMaximized(value);
	}
	/// <summary>
	/// True if the application window is currently maximized
	/// </summary>
	protected abstract bool _GetWindowMaximized ();
	/// <summary>
	/// Set to true to make the application window is currently maximized
	/// </summary>
	protected abstract void _SetWindowMaximized (bool maximized);


	/// <summary>
	/// If the application window is currently minimized
	/// </summary>
	public static bool IsWindowMinimized {
		get => Instance._GetWindowMinimized();
		set => Instance._SetWindowMinimized(value);
	}
	/// <summary>
	/// True if the application window is currently minimized
	/// </summary>
	protected abstract bool _GetWindowMinimized ();
	/// <summary>
	/// Set to true to make the application window is currently minimized
	/// </summary>
	protected abstract void _SetWindowMinimized (bool minimized);


	/// <inheritdoc cref="_SetWindowTitle"/>
	public static void SetWindowTitle (string title) => Instance._SetWindowTitle(title);
	/// <summary>
	/// Set to title text of the application window
	/// </summary>
	protected abstract void _SetWindowTitle (string title);


	/// <inheritdoc cref="_SetWindowIcon"/>
	public static void SetWindowIcon (int spriteID) => Instance._SetWindowIcon(spriteID);
	/// <summary>
	/// Set the icon of the application window using a loaded artwork sprite from the main sheet
	/// </summary>
	protected abstract void _SetWindowIcon (int spriteID);


	/// <inheritdoc cref="_SetWindowMinSize"/>
	public static void SetWindowMinSize (int size) => Instance._SetWindowMinSize(size);
	/// <summary>
	/// Set the minimal size limitation of the application window
	/// </summary>
	protected abstract void _SetWindowMinSize (int size);


	/// <inheritdoc cref="_SetEventWaiting"/>
	public static void SetEventWaiting (bool enable) => Instance._SetEventWaiting(enable);
	/// <summary>
	/// Set to true to make the application only repaint when a user input happens
	/// </summary>
	protected abstract void _SetEventWaiting (bool enable);

}

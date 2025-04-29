using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class Game {


	private static int ForcedTargetFramerate = 60;
	private static int TargetFramerateFrame = -1;
	public static float FrameDurationMilliSecond { get; protected set; } = 0f;


	// API
	/// <inheritdoc cref="_SetFullscreen"/>
	public static bool IsFullscreen {
		get => _IsFullscreen.Value;
		set {
			_IsFullscreen.Value = value;
			Instance._SetFullscreen(value);
		}
	}
	/// <summary>
	/// Set to true to make the application fullscreen
	/// </summary>
	protected abstract void _SetFullscreen (bool fullScreen);


	/// <inheritdoc cref="_GetScreenWidth"/>
	public static int ScreenWidth { get; private set; }
	/// <summary>
	/// Width of the monitor which the application currently inside
	/// </summary>
	public static int MonitorWidth { get; private set; }
	/// <summary>
	/// Width of the application window
	/// </summary>
	protected abstract int _GetScreenWidth ();


	/// <inheritdoc cref="_GetScreenHeight"/>
	public static int ScreenHeight { get; private set; }
	/// <summary>
	/// Height of the monitor which the application currently inside
	/// </summary>
	public static int MonitorHeight { get; private set; }
	/// <summary>
	/// Height of the application window
	/// </summary>
	protected abstract int _GetScreenHeight ();


	/// <inheritdoc cref="_QuitApplication"/>
	public static void QuitApplication () => Instance._QuitApplication();
	/// <summary>
	/// Make the application quit without any confirmation
	/// </summary>
	protected abstract void _QuitApplication ();


	/// <inheritdoc cref="_OpenUrl"/>
	public static void OpenUrl (string url) => Instance._OpenUrl(url);
	/// <summary>
	/// Open the given url with OS default application
	/// </summary>
	protected abstract void _OpenUrl (string url);


	public static int GetTargetFramerate () => GlobalFrame <= TargetFramerateFrame ? ForcedTargetFramerate : 60;
	/// <summary>
	/// Override the target framerate for specified frames long
	/// </summary>
	public static void ForceTargetFramerate (int framerate, int duration = 1) {
		TargetFramerateFrame = GlobalFrame + duration;
		ForcedTargetFramerate = framerate.Clamp(4, 1200);
	}
	/// <summary>
	/// Do not override the target framerate anymore
	/// </summary>
	public static void CancelForceTargetFramerate () {
		ForcedTargetFramerate = 60;
		TargetFramerateFrame = -1;
	}

}

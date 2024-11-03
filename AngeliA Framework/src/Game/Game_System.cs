using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class Game {

	private static int ForcedTargetFramerate = 60;
	private static int TargetFramerateFrame = -1;

	// API
	internal static bool IsFullscreen {
		get => _IsFullscreen.Value;
		set {
			_IsFullscreen.Value = value;
			Instance._SetFullscreen(value);
		}
	}
	protected abstract void _SetFullscreen (bool fullScreen);

	public static int ScreenWidth { get; private set; }
	public static int MonitorWidth { get; private set; }
	protected abstract int _GetScreenWidth ();

	public static int ScreenHeight { get; private set; }
	public static int MonitorHeight { get; private set; }
	protected abstract int _GetScreenHeight ();

	public static void QuitApplication () => Instance._QuitApplication();
	protected abstract void _QuitApplication ();

	public static void OpenUrl (string url) => Instance._OpenUrl(url);
	protected abstract void _OpenUrl (string url);

	public static int GetTargetFramerate () => GlobalFrame <= TargetFramerateFrame ? ForcedTargetFramerate : 60;
	public static void ForceTargetFramerate (int framerate, int duration = 1) {
		TargetFramerateFrame = GlobalFrame + duration;
		ForcedTargetFramerate = framerate.Clamp(4, 1200);
	}
	public static void CancelForceTargetFramerate () {
		ForcedTargetFramerate = 60;
		TargetFramerateFrame = -1;
	}

}

using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class Game {

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

	public static void SetTargetFramerate (int framerate) => Instance._SetTargetFramerate(framerate);
	protected abstract void _SetTargetFramerate (int framerate);

}

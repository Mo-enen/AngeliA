#if DEBUG
using System.Linq;

namespace AngeliaToRaylib.Editor;

public class EditorRefit {
	[OnQuit]
	internal static void OnGameQuitting () {
		// Close CMD
		System.Diagnostics.Process.GetProcessesByName(
			"WindowsTerminal"
		).ToList().ForEach(item => item.CloseMainWindow());
	}
}
#endif
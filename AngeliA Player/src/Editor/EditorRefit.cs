using System.Linq;

namespace AngeliaPlayer;

#if DEBUG
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
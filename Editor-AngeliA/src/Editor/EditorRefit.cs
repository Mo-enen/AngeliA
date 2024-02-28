#if DEBUG
using System.Linq;

namespace AngeliaEditor;

public class EditorRefit {
	[OnQuit(int.MaxValue)]
	internal static void OnGameQuitting () {
		// Close CMD
		System.Diagnostics.Process.GetProcessesByName(
			"WindowsTerminal"
		).ToList().ForEach(item => item.CloseMainWindow());
	}
}
#endif
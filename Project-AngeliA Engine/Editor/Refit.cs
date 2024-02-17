#if DEBUG
using System.Linq;

namespace AngeliaEngine;

public class Refit {
	[OnGameQuit]
	internal static void OnGameQuitting () {
		// Close CMD
		System.Diagnostics.Process.GetProcessesByName(
			"WindowsTerminal"
		).ToList().ForEach(item => item.CloseMainWindow());
	}
}

#endif
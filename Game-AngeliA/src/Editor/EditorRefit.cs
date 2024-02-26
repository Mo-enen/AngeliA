#if DEBUG
using System.Linq;
using AngeliA.Framework;

namespace AngeliaGame.Editor;

public class EditorRefit {
	[OnGameQuitting]
	internal static void OnGameQuitting () {
		// Close CMD
		System.Diagnostics.Process.GetProcessesByName(
			"WindowsTerminal"
		).ToList().ForEach(item => item.CloseMainWindow());
	}
}
#endif
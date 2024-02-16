using System.Linq;
using AngeliA.Framework;

namespace AngeliaGame;

public class Refit {
	[OnGameQuitting(int.MaxValue)]
	internal static void OnGameQuitting () {
		// Close CMD
		System.Diagnostics.Process.GetProcessesByName(
			"WindowsTerminal"
		).ToList().ForEach(item => item.CloseMainWindow());
	}
}
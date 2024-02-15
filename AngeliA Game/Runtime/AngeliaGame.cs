using System.Linq;
using AngeliA.Framework;

namespace AngeliaGame;

public class AngeliaGame {
	[OnGameQuitting(int.MaxValue)]
	internal static void OnGameQuitting () {
		// Close CMD
		if (Game.IsEdittime) {
			System.Diagnostics.Process.GetProcessesByName(
				"WindowsTerminal"
			).ToList().ForEach(item => item.CloseMainWindow());
		}
	}
}
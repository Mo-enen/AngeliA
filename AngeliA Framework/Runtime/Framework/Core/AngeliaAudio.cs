using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public static class AngeliaAudio {

		private static bool PrevPlaying = false;

		[OnGameUpdatePauseless]
		public static void OnGameUpdatePauseless () {

			// Stop Sound on Pause
			if (PrevPlaying && Game.IsPausing) Game.StopAllSounds();
			PrevPlaying = Game.IsPlaying;

			// Load or Stop Music
			bool requireMusic = Game.IsPlaying && Game.MusicVolume > 0 && !MapEditor.IsEditing;
			if (requireMusic != Game.IsMusicPlaying) {
				if (requireMusic) {
					Game.UnPauseMusic();
				} else {
					Game.PauseMusic();
				}
			}
		}

	}
}
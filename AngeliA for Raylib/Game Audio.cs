using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using Raylib_cs;

namespace AngeliaForRaylib;

public partial class GameForRaylib {


	// Data
	private Music CurrentBGM;


	// Music
	protected override IEnumerable<KeyValuePair<int, object>> _ForAllAudioClips () {
		string root = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Audio");
		foreach (var path in Util.EnumerateFiles(root, false, "*.wav", "*.mp3", "*.ogg")) {
			string name = Util.GetNameWithoutExtension(path);
			if (name.EndsWith("#music", System.StringComparison.OrdinalIgnoreCase)) {
				yield return new(name[..^6].TrimEnd(' ').AngeHash(), Raylib.LoadMusicStream(path));
			} else {
				yield return new(name.AngeHash(), Raylib.LoadSound(path));
			}
		}
	}

	protected override void _PlayMusic (int id) {
		Raylib.StopMusicStream(CurrentBGM);
		if (TryGetResource<Music>(id, out var music)) {
			Raylib.PlayMusicStream(music);
			CurrentBGM = music;
		}
	}

	protected override void _StopMusic () => Raylib.StopMusicStream(CurrentBGM);

	protected override void _PauseMusic () => Raylib.PauseMusicStream(CurrentBGM);

	protected override void _UnPauseMusic () => Raylib.ResumeMusicStream(CurrentBGM);

	protected override void _SetMusicVolume (int volume) => Raylib.SetMusicVolume(CurrentBGM, ScaledMusicVolume);

	protected override bool _IsMusicPlaying () => Raylib.IsMusicStreamPlaying(CurrentBGM);


	// Sound
	protected override void _PlaySound (int id, float volume) {
		if (TryGetResource<Sound>(id, out var sound)) {
			Raylib.PlaySound(sound);
			Raylib.SetSoundVolume(sound, ScaledSoundVolume * volume);
		}
	}

	protected override void _StopAllSounds () { }

	protected override void _SetSoundVolume (int volume) { }


}

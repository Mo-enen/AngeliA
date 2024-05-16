using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;

namespace AngeliaRaylib;

public partial class RayGame {


	// Music
	protected override void _UnloadMusic (object music) {
		if (CurrentBGM == null) return;
		Raylib.UnloadMusicStream((Music)music);
	}

	protected override void _PlayMusic (int id) {

		if (!MusicPool.TryGetValue(id, out var path)) return;

		// Stop Current
		if (CurrentBGM is Music bgm && Raylib.IsMusicStreamPlaying(bgm)) {
			Raylib.StopMusicStream(bgm);
			Raylib.UnloadMusicStream(bgm);
		}

		// Play New
		var music = Raylib.LoadMusicStream(path);
		Raylib.PlayMusicStream(music);
		music.Looping = true;
		CurrentBGM = music;

	}

	protected override void _StopMusic () {
		if (CurrentBGM == null) return;
		Raylib.StopMusicStream((Music)CurrentBGM);
	}

	protected override void _PauseMusic () {
		if (CurrentBGM == null) return;
		Raylib.PauseMusicStream((Music)CurrentBGM);
	}

	protected override void _UnPauseMusic () {
		if (CurrentBGM == null) return;
		Raylib.ResumeMusicStream((Music)CurrentBGM);
	}

	protected override void _SetMusicVolume (int volume) {
		if (CurrentBGM == null) return;
		Raylib.SetMusicVolume((Music)CurrentBGM, ScaledMusicVolume);
	}

	protected override bool _IsMusicPlaying () {
		if (CurrentBGM == null) return false;
		return Raylib.IsMusicStreamPlaying((Music)CurrentBGM);
	}


	// Sound
	protected override object _LoadSound (string filePath) => Raylib.LoadSound(filePath);

	protected override void _UnloadSound (object sound) {
		if (sound == null) return;
		Raylib.UnloadSound((Sound)sound);
	}

	protected override void _PlaySound (int id, float volume) {
		if (!SoundPool.TryGetValue(id, out var soundObj) || soundObj == null) return;
		var sound = (Sound)soundObj;
		Raylib.PlaySound(sound);
		Raylib.SetSoundVolume(sound, ScaledSoundVolume * volume);
	}

	protected override void _StopAllSounds () {
		foreach (var (_, soundObj) in SoundPool) {
			if (soundObj == null) continue;
			Raylib.StopSound((Sound)soundObj);
		}
	}

	protected override void _SetSoundVolume (int volume) { }


}

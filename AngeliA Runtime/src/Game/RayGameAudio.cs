using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;

namespace AngeliaRuntime.Framework;

public partial class RayGame {


	// Data
	private static readonly Dictionary<int, Sound> SoundPool = new();
	private static readonly Dictionary<int, string> MusicPool = new();
	private Music CurrentBGM;


	// Music
	private void InitializeAudio () {
		Raylib.InitAudioDevice();
		// Music
		MusicPool.Clear();
		string musicRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Audio", "Music");
		foreach (var path in Util.EnumerateFiles(musicRoot, false, "*.wav", "*.mp3", "*.ogg")) {
			MusicPool.TryAdd(Util.GetNameWithoutExtension(path).TrimEnd(' ').AngeHash(), path);
		}
		// Sound
		SoundPool.Clear();
		string soundRoot = Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Audio", "Sound");
		foreach (var path in Util.EnumerateFiles(soundRoot, false, "*.wav", "*.mp3", "*.ogg")) {
			SoundPool.TryAdd(Util.GetNameWithoutExtension(path).AngeHash(), Raylib.LoadSound(path));
		}
	}

	protected override void _PlayMusic (int id) {

		if (!MusicPool.TryGetValue(id, out var path)) return;

		// Stop Current
		if (Raylib.IsMusicStreamPlaying(CurrentBGM)) {
			Raylib.StopMusicStream(CurrentBGM);
			Raylib.UnloadMusicStream(CurrentBGM);
		}

		// Play New
		var music = Raylib.LoadMusicStream(path);
		Raylib.PlayMusicStream(music);
		music.Looping = true;
		CurrentBGM = music;

	}

	protected override void _StopMusic () => Raylib.StopMusicStream(CurrentBGM);

	protected override void _PauseMusic () => Raylib.PauseMusicStream(CurrentBGM);

	protected override void _UnPauseMusic () => Raylib.ResumeMusicStream(CurrentBGM);

	protected override void _SetMusicVolume (int volume) => Raylib.SetMusicVolume(CurrentBGM, ScaledMusicVolume);

	protected override bool _IsMusicPlaying () => Raylib.IsMusicStreamPlaying(CurrentBGM);


	// Sound
	protected override void _PlaySound (int id, float volume) {
		if (SoundPool.TryGetValue(id, out var sound)) {
			Raylib.PlaySound(sound);
			Raylib.SetSoundVolume(sound, ScaledSoundVolume * volume);
		}
	}

	protected override void _StopAllSounds () { }

	protected override void _SetSoundVolume (int volume) { }


}

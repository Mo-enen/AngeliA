using System.Collections;
using System.Collections.Generic;
using AngeliA;
using Raylib_cs;

namespace AngeliaRaylib;

public partial class RayGame {


	// Music
	protected override void _UnloadMusic (object music) {
		if (music is not Music rMusic) return;
		if (Raylib.IsAudioStreamReady(rMusic.Stream)) {
			Raylib.UnloadMusicStream(rMusic);
		}
	}

	protected override void _PlayMusic (int id) {

		if (!MusicPool.TryGetValue(id, out var data)) return;

		// Stop Current
		if (CurrentBGM is Music bgm && Raylib.IsMusicStreamPlaying(bgm)) {
			Raylib.StopMusicStream(bgm);
			Raylib.UnloadMusicStream(bgm);
		}
		CurrentBGM = null;
		CurrentBgmID = 0;

		// Play New
		if (Util.FileExists(data.Path)) {
			var music = Raylib.LoadMusicStream(data.Path);
			Raylib.PlayMusicStream(music);
			music.Looping = true;
			CurrentBGM = music;
			CurrentBgmID = id;
		}

	}

	protected override void _StopMusic () {
		if (CurrentBGM == null) return;
		if (CurrentBGM is Music bgm && Raylib.IsMusicStreamPlaying(bgm)) {
			Raylib.StopMusicStream(bgm);
			Raylib.UnloadMusicStream(bgm);
		}
		CurrentBGM = null;
		CurrentBgmID = 0;
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

	protected override int _GetCurrentMusicID () => CurrentBgmID;


	// Sound
	protected override object _LoadSound (string filePath) => Raylib.LoadSound(filePath);

	protected override object _LoadSoundAlias (object source) {
		if (source is not Sound sound) return null;
		return Raylib.LoadSoundAlias(sound);
	}

	protected override void _UnloadSound (SoundData sound) {
		if (sound == null) return;
		for (int i = 0; i < Const.SOUND_CHANNEL_COUNT; i++) {
			if (sound.SoundObjects[i] is Sound s && Raylib.IsSoundReady(s)) {
				if (i != 0) {
					Raylib.UnloadSoundAlias(s);
				} else {
					Raylib.UnloadSound(s);
				}
			}
		}
	}

	protected override void _PlaySound (int id, float volume, float pitch, float pan) {
		if (!SoundPool.TryGetValue(id, out var soundData) || soundData == null) return;
		bool played = false;
		int earlistIndex = -1;
		int earlistFrame = int.MaxValue;
		for (int i = 0; i < Const.SOUND_CHANNEL_COUNT; i++) {
			var sound = (Sound)soundData.SoundObjects[i];
			if (!Raylib.IsSoundReady(sound)) continue;
			if (Raylib.IsSoundPlaying(sound)) {
				int frame = soundData.StartFrames[i];
				if (frame < earlistFrame) {
					earlistFrame = frame;
					earlistIndex = i;
				}
				continue;
			}
			Raylib.PlaySound(sound);
			Raylib.SetSoundVolume(sound, ScaledSoundVolume * volume);
			Raylib.SetSoundPitch(sound, pitch);
			Raylib.SetSoundPan(sound, 1f - pan);
			soundData.StartFrames[i] = GlobalFrame;
			played = true;
			break;
		}
		// Force Play
		if (!played && earlistIndex >= 0) {
			var sound = (Sound)soundData.SoundObjects[earlistIndex];
			Raylib.PlaySound(sound);
			Raylib.SetSoundVolume(sound, ScaledSoundVolume * volume);
			Raylib.SetSoundPitch(sound, pitch);
			Raylib.SetSoundPan(sound, 1f - pan);
			soundData.StartFrames[earlistIndex] = GlobalFrame;
		}
	}

	protected override void _StopAllSounds () {
		foreach (var (_, soundObj) in SoundPool) {
			if (soundObj == null) continue;
			for (int i = 0; i < Const.SOUND_CHANNEL_COUNT; i++) {
				var sound = (Sound)soundObj.SoundObjects[i];
				if (Raylib.IsSoundReady(sound)) {
					Raylib.StopSound(sound);
				}
			}
		}
	}

	protected override void _SetSoundVolume (int volume) { }


}

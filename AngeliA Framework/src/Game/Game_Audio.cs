using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class Game {


	// Music
	public static void UnloadMusic (object music) => Instance._UnloadMusic(music);
	protected abstract void _UnloadMusic (object music);

	public static void PlayMusic (int id) => Instance._PlayMusic(id);
	protected abstract void _PlayMusic (int id);

	public static void StopMusic () => Instance._StopMusic();
	protected abstract void _StopMusic ();

	public static void PauseMusic () => Instance._PauseMusic();
	protected abstract void _PauseMusic ();

	public static void UnpauseMusic () => Instance._UnPauseMusic();
	protected abstract void _UnPauseMusic ();

	public static void SetMusicVolume (int volume) {
		_MusicVolume.Value = volume;
		Instance._SetMusicVolume(volume);
	}
	protected abstract void _SetMusicVolume (int volume);

	public static bool IsMusicPlaying => Instance._IsMusicPlaying();
	protected abstract bool _IsMusicPlaying ();

	public static int CurrentMusicID => Instance._GetCurrentMusicID();
	protected abstract int _GetCurrentMusicID ();


	// Sound
	public static object LoadSound (string filePath) => Instance._LoadSound(filePath);
	protected abstract object _LoadSound (string filePath);

	protected static object LoadSoundAlias (object source) => Instance._LoadSoundAlias(source);
	protected abstract object _LoadSoundAlias (object source);

	public static void UnloadSound (SoundData sound) => Instance._UnloadSound(sound);
	protected abstract void _UnloadSound (SoundData sound);

	public static void PlaySound (int id, float volume = 1f, float pitch = 1f, float pan = 0.5f) => Instance._PlaySound(id, volume, pitch, pan);
	protected abstract void _PlaySound (int id, float volume, float pitch, float pan);

	public static void StopAllSounds () => Instance._StopAllSounds();
	protected abstract void _StopAllSounds ();

	public static void SetSoundVolume (int volume) {
		_SoundVolume.Value = volume;
		Instance._SetSoundVolume(volume);
	}
	protected abstract void _SetSoundVolume (int volume);


}

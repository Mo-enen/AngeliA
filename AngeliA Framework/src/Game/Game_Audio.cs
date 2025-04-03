using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AngeliA;

public partial class Game {


	// Music
	internal static void UnloadMusic (object music) => Instance._UnloadMusic(music);
	/// <summary>
	/// Unload the bgm object from memory
	/// </summary>
	protected abstract void _UnloadMusic (object music);

	/// <inheritdoc cref="_PlayMusic"/>
	public static void PlayMusic (int id, bool fromStart = false) => Instance._PlayMusic(id, fromStart);
	/// <summary>
	/// Load the music file, create the stream and play it as the background music 
	/// </summary>
	/// <param name="id">ID of the audio</param>
	/// <param name="fromStart">True if seek the music to start</param>
	protected abstract void _PlayMusic (int id, bool fromStart);

	/// <inheritdoc cref="_StopMusic"/>
	public static void StopMusic () => Instance._StopMusic();
	/// <summary>
	/// Stop the music stream and unload from memory
	/// </summary>
	protected abstract void _StopMusic ();

	/// <inheritdoc cref="_PauseMusic"/>
	public static void PauseMusic () => Instance._PauseMusic();
	/// <summary>
	/// Pause the background music
	/// </summary>
	protected abstract void _PauseMusic ();

	/// <inheritdoc cref="_UnPauseMusic"/>
	public static void UnpauseMusic () => Instance._UnPauseMusic();
	/// <summary>
	/// Resume the background music
	/// </summary>
	protected abstract void _UnPauseMusic ();

	/// <inheritdoc cref="_SetMusicVolume"/>
	public static void SetMusicVolume (int volume) {
		_MusicVolume.Value = volume;
		Instance._SetMusicVolume(volume);
	}
	/// <summary>
	/// Set volume for the background music (0 means mute, 1000 means loudest)
	/// </summary>
	protected abstract void _SetMusicVolume (int volume);

	/// <inheritdoc cref="_IsMusicPlaying"/>
	public static bool IsMusicPlaying => Instance._IsMusicPlaying();
	/// <summary>
	/// True if the background music is currently playing
	/// </summary>
	protected abstract bool _IsMusicPlaying ();

	/// <inheritdoc cref="_GetCurrentMusicID"/>
	public static int CurrentMusicID => Instance._GetCurrentMusicID();
	/// <summary>
	/// Audio ID of the current loaded background music
	/// </summary>
	protected abstract int _GetCurrentMusicID ();


	// Sound
	internal static object LoadSound (string filePath) => Instance._LoadSound(filePath);
	/// <summary>
	/// Load a audio file as sound effect
	/// </summary>
	protected abstract object _LoadSound (string filePath);

	internal static object LoadSoundAlias (object source) => Instance._LoadSoundAlias(source);
	/// <summary>
	/// Copy a sound instance that shares the same audio data with given sound object
	/// </summary>
	protected abstract object _LoadSoundAlias (object source);

	internal static void UnloadSound (SoundData sound) => Instance._UnloadSound(sound);
	/// <summary>
	/// Unload the sound object from memory
	/// </summary>
	protected abstract void _UnloadSound (SoundData sound);

	/// <inheritdoc cref="_PlaySound"/>
	public static void PlaySound (int id, float volume = 1f, float pitch = 1f, float pan = 0.5f) => Instance._PlaySound(id, volume, pitch, pan);
	
	/// <summary>
	/// Play a sound
	/// </summary>
	/// <param name="id">Audio ID</param>
	/// <param name="volume"></param>
	/// <param name="pitch"></param>
	/// <param name="pan"></param>
	protected abstract void _PlaySound (int id, float volume, float pitch, float pan);

	/// <inheritdoc cref="_StopAllSounds"/>
	public static void StopAllSounds () => Instance._StopAllSounds();
	/// <summary>
	/// Stop all sound effects that currently playing
	/// </summary>
	protected abstract void _StopAllSounds ();

	/// <inheritdoc cref="_SetSoundVolume"/>
	public static void SetSoundVolume (int volume) {
		_SoundVolume.Value = volume;
		Instance._SetSoundVolume(volume);
	}
	/// <summary>
	/// Set volume for all sound effects
	/// </summary>
	protected abstract void _SetSoundVolume (int volume);

}

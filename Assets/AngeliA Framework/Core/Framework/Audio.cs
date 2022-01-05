using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public static class Audio {




		#region --- VAR ---


		// Data
		public static AudioSource MusicSource = null;
		private static AudioSource SoundSource = null;
		private static Dictionary<int, AudioClip> MusicMap = new();
		private static Dictionary<int, AudioClip> SoundMap = new();
		private static float SoundVolume = 1f;


		#endregion




		#region --- API ---


		public static void Initialize () {
			// Setup Components
			var root = new GameObject("Audio", typeof(AudioListener)).transform;
			root.SetParent(null);
			root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			root.localScale = Vector3.one;
			MusicSource = root.gameObject.AddComponent<AudioSource>();
			MusicSource.loop = true;
			MusicSource.playOnAwake = false;
			MusicSource.mute = false;
			MusicSource.volume = 1f;
			MusicSource.pitch = 1f;
			SoundSource = root.gameObject.AddComponent<AudioSource>();
			SoundSource.loop = false;
			SoundSource.playOnAwake = false;
			SoundSource.mute = false;
			SoundSource.volume = 1f;
			SoundSource.pitch = 1f;
		}


		// Music
		public static void AddMusic (AudioClip clip) {
			int id = clip.name.ACode();
			if (!MusicMap.ContainsKey(id)) {
				MusicMap.Add(id, clip);
			}
#if UNITY_EDITOR
			else {
				Debug.LogError($"[Audio] Audio clip {clip.name} id already exists.");
			}
#endif
		}


		public static void PlayMusic (int musicID) {
			MusicSource.Stop();
			if (!MusicMap.ContainsKey(musicID)) {
#if UNITY_EDITOR
				Debug.LogError("[Audio] Do not contains music id " + musicID);
#endif
				return;
			}
			var clip = MusicMap[musicID];
			MusicSource.clip = clip;
			if (clip) {
				while (MusicSource.clip.loadState == AudioDataLoadState.Loading) { }
			}
			if (clip.loadState != AudioDataLoadState.Failed) {
				MusicSource.Play();
			}
		}


		public static void StopMusic () => MusicSource.Stop();


		public static void SetMusicVolume (float volume) => MusicSource.volume = volume;


		public static void SetMusicPitch (float pitch) => MusicSource.pitch = pitch;


		// Sound
		public static void AddSound (AudioClip clip) {
			int id = clip.name.ACode();
			if (!SoundMap.ContainsKey(id)) {
				SoundMap.Add(id, clip);
			}
#if UNITY_EDITOR
			else {
				Debug.LogError($"[Audio] Audio clip {clip.name} id already exists.");
			}
#endif
		}


		public static void PlaySound (int soundID) {
			if (SoundMap.ContainsKey(soundID)) {
				SoundSource.PlayOneShot(SoundMap[soundID], SoundVolume);
			}
#if UNITY_EDITOR
			else {
				Debug.LogError("[Audio] Do not contains sound id " + soundID);
			}
#endif
		}


		public static void SetSoundVolume (float volume) => SoundVolume = volume;


		#endregion




	}
}

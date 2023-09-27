using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


namespace AngeliaFramework {
	public static class AudioPlayer {




		#region --- VAR ---


		// Const
		private const int SOUND_TRACK_COUNT = 6;

		// Api
		public static int MusicVolume => _MusicVolume.Value;
		public static int SoundVolume => _SoundVolume.Value;

		// Data
		private static readonly Dictionary<int, AudioClip> AudioMap = new();
		private static AudioSource MusicSource = null;
		private static AudioSource[] SoundSources = null;

		// Saving
		private static readonly SavingInt _MusicVolume = new("Audio.MusicVolume", 500);
		private static readonly SavingInt _SoundVolume = new("Audio.SoundVolume", 1000);


		#endregion




		#region --- API ---


		public static void Initialize (AudioClip[] audioClips) {

			AudioMap.Clear();

			// Music
			var root = new GameObject("Audio", typeof(AudioListener)).transform;
			root.SetParent(null);
			root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			root.localScale = Vector3.one;
			MusicSource = root.gameObject.AddComponent<AudioSource>();
			MusicSource.loop = true;
			MusicSource.playOnAwake = false;
			MusicSource.volume = GetVolume01(true);
			MusicSource.pitch = 1f;
			MusicSource.mute = false;
			Object.DontDestroyOnLoad(root);

			// Sound
			SoundSources = new AudioSource[SOUND_TRACK_COUNT];
			for (int i = 0; i < SOUND_TRACK_COUNT; i++) {
				var source = SoundSources[i] = root.gameObject.AddComponent<AudioSource>();
				source.loop = false;
				source.playOnAwake = false;
				source.volume = GetVolume01(false);
				source.pitch = 1f;
				source.mute = false;
			}

			// Fill Map
			foreach (var clip in audioClips) {
				int id = clip.name.AngeHash();
				if (!AudioMap.ContainsKey(id)) {
					AudioMap.Add(id, clip);
				} else Debug.LogError($"[Audio] Audio clip {clip.name} id already exists.");
			}

		}


		internal static void FrameUpdate (bool isPausing) {
			// Update for BGM
			if (
				MusicSource != null &&
				MusicSource.clip != null &&
				MusicSource.clip.loadState == AudioDataLoadState.Loaded
			) {
				if (!isPausing) {
					if (!MusicSource.isPlaying) MusicSource.UnPause();
				} else {
					if (MusicSource.isPlaying) MusicSource.Pause();
				}
			}
		}


		public static void PauseAll () {
			MusicSource.Pause();
			foreach (var source in SoundSources) source.Stop();
		}


		public static void UnPauseAll () {
			MusicSource.UnPause();
			foreach (var source in SoundSources) source.Stop();
		}


		// Music
		public static void PlayMusic (int musicID) {
			MusicSource.Stop();
			if (!AudioMap.ContainsKey(musicID)) {
#if UNITY_EDITOR
				Debug.LogError("[Audio] Do not contains music id " + musicID);
#endif
				return;
			}
			var clip = AudioMap[musicID];
			MusicSource.clip = clip;
			if (clip) {
				while (MusicSource.clip.loadState == AudioDataLoadState.Loading) { }
			}
			if (clip.loadState != AudioDataLoadState.Failed) {
				MusicSource.Play();
			}
		}


		public static void StopMusic () => MusicSource.Stop();


		public static void SetMusicVolume (int volume) {
			volume = volume.Clamp(0, 1000);
			_MusicVolume.Value = volume;
			MusicSource.volume = GetVolume01(true);
		}


		public static void SetSoundVolume (int volume) => _SoundVolume.Value = volume.Clamp(0, 1000);


		// Sound
		public static void PlaySound (int soundID, float volume = 1f) {
			volume *= GetVolume01(false);
			if (soundID == 0 || volume.LessOrAlmost(0f)) return;
			if (AudioMap.TryGetValue(soundID, out var clip)) {
				float maxTime = -1f;
				int maxIndex = -1;
				for (int i = 0; i < SoundSources.Length; i++) {
					var source = SoundSources[i];
					if (!source.isPlaying) {
						Play(source, clip, volume);
						maxIndex = -1;
						break;
					} else if (source.time > maxTime) {
						maxTime = source.time;
						maxIndex = i;
					}
				}
				if (maxIndex >= 0) {
					Play(SoundSources[maxIndex], clip, volume);
				}
			}
#if UNITY_EDITOR
			else Debug.LogWarning("[Audio] Do not contains sound id " + soundID);
#endif
			// Func
			static void Play (AudioSource source, AudioClip clip, float volume) {
				if (source.clip != clip) source.clip = clip;
				if (source.volume.NotAlmost(volume)) source.volume = volume;
				source.PlayScheduled(0);
			}
		}


		#endregion




		#region --- LGC ---


		private static float GetVolume01 (bool music) {
			float volume = (music ? _MusicVolume.Value : _SoundVolume.Value) / 1000f;
			return volume * volume;
		}


		#endregion




	}
}

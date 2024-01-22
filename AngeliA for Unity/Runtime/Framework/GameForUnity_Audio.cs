using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngeliaFramework;
using UnityEngine;
using UnityEngine.Networking;

namespace AngeliaForUnity {
	public sealed partial class GameForUnity {


		// VAR
		private const int SOUND_TRACK_COUNT = 6;
		private static AudioSource MusicSource = null;
		private static AudioSource[] SoundSources = null;


		// MSG
		private static void InitializeAudio () {

			// Music
			var root = new GameObject("Audio", typeof(AudioListener)).transform;
			root.SetParent(null);
			root.SetPositionAndRotation(Vector3.zero, default);
			root.localScale = Vector3.one;
			MusicSource = root.gameObject.AddComponent<AudioSource>();
			MusicSource.loop = true;
			MusicSource.playOnAwake = false;
			MusicSource.volume = 0f;
			MusicSource.pitch = 1f;
			MusicSource.mute = false;

			// Sound
			SoundSources = new AudioSource[SOUND_TRACK_COUNT];
			for (int i = 0; i < SOUND_TRACK_COUNT; i++) {
				var source = SoundSources[i] = root.gameObject.AddComponent<AudioSource>();
				source.loop = false;
				source.playOnAwake = false;
				source.volume = 0f;
				source.pitch = 1f;
				source.mute = false;
			}

			Object.DontDestroyOnLoad(root.gameObject);
		}


		// Resource
		protected override IEnumerable<KeyValuePair<int, object>> _ForAllAudioClips () {
			foreach (var clip in AudioClips) {
				if (clip == null || string.IsNullOrEmpty(clip.name)) continue;
				yield return new(clip.name.AngeHash(), clip);
			}
		}


		// Music
		protected override void _PlayMusic (int id) {
			MusicSource.Stop();
			if (MusicVolume == 0) return;
			if (TryGetResource<AudioClip>(id, out var unityClip)) {
				MusicSource.clip = unityClip;
				MusicSource.Play();
			}
		}


		protected override void _StopMusic () => MusicSource.Stop();


		protected override void _PauseMusic () => MusicSource.Pause();


		protected override void _UnPauseMusic () => MusicSource.UnPause();


		protected override bool _IsMusicPlaying () => MusicSource.clip != null && MusicSource.clip.loadState == AudioDataLoadState.Loaded && MusicSource.isPlaying;


		protected override void _SetMusicVolume (int volume) => MusicSource.volume = ScaledMusicVolume;


		// Sound
		protected override void _PlaySound (int soundID, float volume) {
			if (SoundVolume == 0 || volume.LessOrAlmost(0f)) return;
			volume *= ScaledSoundVolume;
			if (TryGetResource<AudioClip>(soundID, out var clip)) {
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
			} else {
				Debug.LogWarning("[Audio] Do not contains sound id " + soundID);
			}
			// Func
			static void Play (AudioSource source, AudioClip clip, float volume) {
				if (source.clip != clip) source.clip = clip;
				if (source.volume.NotAlmost(volume)) source.volume = volume;
				source.PlayScheduled(0);
			}

		}


		protected override void _StopAllSounds () {
			foreach (var source in SoundSources) {
				source.Stop();
			}
		}


		protected override void _SetSoundVolume (int volume) {
			float scaledVolume = ScaledSoundVolume;
			foreach (var source in SoundSources) {
				source.volume = scaledVolume;
			}
		}


	}
}
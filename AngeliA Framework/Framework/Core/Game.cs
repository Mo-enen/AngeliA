using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AngeliaFramework;


[assembly: AngeliA]

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }


namespace AngeliaFramework {


	public enum FullscreenMode { Window = 0, Fullscreen = 1, FullscreenLow = 2, }


	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitializeAttribute : System.Attribute { public int Order; public OnGameInitializeAttribute (int order = 0) => Order = order; }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitializeLaterAttribute : System.Attribute { public int Order; public OnGameInitializeLaterAttribute (int order = 0) => Order = order; }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateAttribute : OrderedAttribute { public OnGameUpdateAttribute (int order = 0) : base(order) { } }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateLaterAttribute : OrderedAttribute { public OnGameUpdateLaterAttribute (int order = 0) : base(order) { } }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdatePauselessAttribute : OrderedAttribute { public OnGameUpdatePauselessAttribute (int order = 0) : base(order) { } }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameRestartAttribute : OrderedAttribute { public OnGameRestartAttribute (int order = 0) : base(order) { } }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameTryingToQuitAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameQuittingAttribute : System.Attribute { }


	public abstract partial class Game {




		#region --- VAR ---


		// Api
		public static Game Instance { get; private set; } = null;
		public static int GlobalFrame { get; private set; } = 0;
		public static int SettleFrame => GlobalFrame - Stage.LastSettleFrame;
		public static int PauselessFrame { get; private set; } = 0;
		public static bool IsPausing => !IsPlaying;
		public static bool IsPlaying { get; private set; } = true;
		public static bool ShowFPS {
			get => _ShowFPS.Value;
			set {
				_ShowFPS.Value = value;
				if (value) {
					LastGraphicUpdateTime = GameWatch.ElapsedMilliseconds;
				}
			}
		}
		public static int MusicVolume {
			get => _MusicVolume.Value;
			set => _MusicVolume.Value = value;
		}
		public static int SoundVolume {
			get => _SoundVolume.Value;
			set => _SoundVolume.Value = value;
		}
		public static float ScaledMusicVolume => AngeUtil.GetScaledAudioVolume(_MusicVolume.Value, ProcedureAudioVolume);
		public static float ScaledSoundVolume => AngeUtil.GetScaledAudioVolume(_SoundVolume.Value, ProcedureAudioVolume);
		public static int ProcedureAudioVolume { get; set; } = 1000;
		public static float CurrentFPS { get; private set; } = 1f;
		public static int GameMajorVersion { get; private set; } = -1;
		public static int GameMinorVersion { get; private set; } = -1;
		public static int GamePatchVersion { get; private set; } = -1;
		public static ReleaseLifeCycle GameLifeCycle { get; private set; } = ReleaseLifeCycle.Release;
		public static string GameTitle { get; private set; } = "";
		public static string GameDeveloper { get; private set; } = "";
		public static bool AllowMakerFeaures { get; private set; } = false;

		// Event
		private static event System.Action OnGameRestart;
		private static event System.Action OnGameTryingToQuit;
		private static event System.Action OnGameQuitting;
		private static event System.Action OnGameUpdate;
		private static event System.Action OnGameUpdateLater;
		private static event System.Action OnGameUpdatePauseless;

		// Data
		private static readonly Dictionary<int, object> ResourcePool = new();
		private static Stopwatch GameWatch;
		private static long LastGraphicUpdateTime = 0;

		// Saving
		private static readonly SavingInt _GraphicFramerate = new("Game.GraphicFramerate", 60);
		private static readonly SavingInt _FullscreenMode = new("Game.FullscreenMode", 0);
		private static readonly SavingInt _MusicVolume = new("Audio.MusicVolume", 500);
		private static readonly SavingInt _SoundVolume = new("Audio.SoundVolume", 1000);
		private static readonly SavingBool _VSync = new("Game.VSync", false);
		private static readonly SavingBool _ShowFPS = new("Game.ShowFPS", false);


		#endregion




		#region --- MSG ---


		public Game () {
			Instance = this;
			// Info from Attribute
			GameTitle = AngeliaGameTitleAttribute.GetTitle();
			GameDeveloper = AngeliaGameDeveloperAttribute.GetDeveloper();
			AllowMakerFeaures = IsEdittime || AngeliaAllowMakerAttribute.AllowMakerFeatures;
			AngeliaVersionAttribute.GetVersion(out int major, out int minor, out int patch, out var cycle);
			GameMajorVersion = major;
			GameMinorVersion = minor;
			GamePatchVersion = patch;
			GameLifeCycle = cycle;
		}


		public virtual void Initialize () {
			try {

				GlobalFrame = 0;
				GameWatch = Stopwatch.StartNew();

				Util.LinkEventWithAttribute<OnGameUpdateAttribute>(typeof(Game), nameof(OnGameUpdate));
				Util.LinkEventWithAttribute<OnGameUpdateLaterAttribute>(typeof(Game), nameof(OnGameUpdateLater));
				Util.LinkEventWithAttribute<OnGameUpdatePauselessAttribute>(typeof(Game), nameof(OnGameUpdatePauseless));
				Util.LinkEventWithAttribute<OnGameTryingToQuitAttribute>(typeof(Game), nameof(OnGameTryingToQuit));
				Util.LinkEventWithAttribute<OnGameQuittingAttribute>(typeof(Game), nameof(OnGameQuitting));
				Util.LinkEventWithAttribute<OnGameRestartAttribute>(typeof(Game), nameof(OnGameRestart));

				_AddGameTryingToQuitCallback(OnTryingToQuit);
				_AddGameQuittingCallback(OnGameQuitting);
				_AddTextInputCallback(CellRendererGUI.OnTextInput);

				Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));

				SetDebugEnable(_GetIsEdittime());
				_SetGraphicFramerate(GraphicFramerate);
				_SetVSync(_VSync.Value);
				_SetFullscreenMode((FullscreenMode)_FullscreenMode.Value);
				_SetMusicVolume(MusicVolume);
				_SetSoundVolume(SoundVolume);

				Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeLaterAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));

				ResourcePool.Clear();
				ResourcePool.AddRange(_ForAllAudioClips());

				System.GC.Collect();

				if (IsEdittime) {
					WindowUI.OpenWindow(MapEditor.TYPE_ID);
				} else if (AllowMakerFeaures) {
					WindowUI.OpenWindow(HomeScreen.TYPE_ID);
				} else {
					RestartGame();
				}

			} catch (System.Exception ex) { LogException(ex); }
			// Func
			static bool OnTryingToQuit () {
				if (IsPausing || IsEdittime) return true;
				PauseGame();
				OnGameTryingToQuit?.Invoke();
				return false;
			}
		}


		public virtual void GameUpdate () {
			try {

				// Update Callbacks
				if (IsPlaying) {
					OnGameUpdate?.Invoke();
					OnGameUpdateLater?.Invoke();
				}
				OnGameUpdatePauseless?.Invoke();

				// Switch Between Play and Pause
				if (FrameInput.GameKeyUp(Gamekey.Start)) {
					if (IsPlaying) {
						PauseGame();
					} else {
						UnpauseGame();
					}
				}

				// Grow Frame
				if (!IsPausing) GlobalFrame++;
				PauselessFrame++;

			} catch (System.Exception ex) { LogException(ex); }
		}


		public virtual void GraphicUpdate () {
			if (_ShowFPS.Value) {
				long currentTime = GameWatch.ElapsedMilliseconds;
				float deltaTime = (currentTime - LastGraphicUpdateTime) / 1000f;
				LastGraphicUpdateTime = currentTime;
				CurrentFPS = Util.LerpUnclamped(CurrentFPS, 1f / Util.Max(deltaTime, 0.000001f), 0.1f);
			}
		}


		[OnGameUpdatePauseless]
		internal static void RefreshGameAudio () {
			// Load or Stop Music
			bool requireMusic = IsPlaying && ScaledMusicVolume > 0 && !MapEditor.IsEditing;
			if (requireMusic != IsMusicPlaying) {
				if (requireMusic) {
					UnpauseMusic();
				} else {
					PauseMusic();
				}
			}
		}


		#endregion




		#region --- API ---


		public static void RestartGame () => OnGameRestart?.Invoke();


		public static void StopGame () {
			WorldSquad.Enable = false;
			Stage.DespawnAllEntitiesFromWorld();
			if (Player.Selecting != null) Player.Selecting.Active = false;
		}


		public static void UnpauseGame () {
			if (IsPlaying) return;
			IsPlaying = true;
		}


		public static void PauseGame () {
			if (!IsPlaying) return;
			StopAllSounds();
			IsPlaying = false;
		}


		public static bool TryGetResource<T> (int id, out T resource) {
			if (ResourcePool.TryGetValue(id, out object obj) && obj is T tObj) {
				resource = tObj;
				return true;
			}
			resource = default;
			return false;
		}


		#endregion




	}
}
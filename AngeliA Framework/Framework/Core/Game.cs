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
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameRestartAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameTryingToQuitAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameQuittingAttribute : System.Attribute { }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnSlotChangedAttribute : OrderedAttribute { public OnSlotChangedAttribute (int order = 0) : base(order) { } }
	[System.AttributeUsage(System.AttributeTargets.Method)] public class OnSlotCreatedAttribute : System.Attribute { }


	public abstract partial class Game {




		#region --- VAR ---


		// Api
		public static Game Instance { get; private set; } = null;
		public static int GlobalFrame { get; private set; } = 0;
		public static int SettleFrame => GlobalFrame - Stage.LastSettleFrame;
		public static int PauselessFrame { get; private set; } = 0;
		public static bool IsPausing => !IsPlaying;
		public static bool IsPlaying {
			get => _IsPlaying;
			set {
				if (_IsPlaying != value) {
					_IsPlaying = value;
					if (!value) Game.StopAllSounds();
				}
			}
		}
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
		public static float ScaledMusicVolume => GetScaledAudioVolume(_MusicVolume.Value, ProcedureAudioVolume);
		public static float ScaledSoundVolume => GetScaledAudioVolume(_SoundVolume.Value, ProcedureAudioVolume);
		public static int ProcedureAudioVolume { get; set; } = 1000;
		public static float CurrentFPS { get; private set; } = 1f;

		// Event
		private static event System.Action OnGameRestart;
		private static event System.Action OnGameTryingToQuit;
		private static event System.Action OnGameQuitting;
		private static event System.Action OnGameUpdate;
		private static event System.Action OnGameUpdateLater;
		private static event System.Action OnGameUpdatePauseless;
		private static event System.Action OnSlotChanged;
		private static event System.Action OnSlotCreated;

		// Data
		private static readonly Dictionary<int, object> ResourcePool = new();
		private static int? RequireRestartWithPlayerID = null;
		private static long LastGraphicUpdateTime = 0;
		private static Stopwatch GameWatch;
		private static bool _IsPlaying = true;

		// Saving
		private static readonly SavingInt _GraphicFramerate = new("Game.GraphicFramerate", 60);
		private static readonly SavingInt _FullscreenMode = new("Game.FullscreenMode", 0);
		private static readonly SavingInt _CurrentSaveSlot = new("Game.CurrentSaveSlot", 0);
		private static readonly SavingBool _VSync = new("Game.VSync", false);
		private static readonly SavingBool _ShowFPS = new("Game.ShowFPS", false);
		private static readonly SavingInt _MusicVolume = new("Audio.MusicVolume", 500);
		private static readonly SavingInt _SoundVolume = new("Audio.SoundVolume", 1000);


		#endregion




		#region --- MSG ---


		public Game () => Instance = this;


		public virtual void Initialize () {
			try {

				GlobalFrame = 0;
				GameWatch = Stopwatch.StartNew();

				AngePath.CurrentSaveSlot = _CurrentSaveSlot.Value;

				Util.LinkEventWithAttribute<OnGameUpdateAttribute>(typeof(Game), nameof(OnGameUpdate));
				Util.LinkEventWithAttribute<OnGameUpdateLaterAttribute>(typeof(Game), nameof(OnGameUpdateLater));
				Util.LinkEventWithAttribute<OnGameUpdatePauselessAttribute>(typeof(Game), nameof(OnGameUpdatePauseless));
				Util.LinkEventWithAttribute<OnGameTryingToQuitAttribute>(typeof(Game), nameof(OnGameTryingToQuit));
				Util.LinkEventWithAttribute<OnGameQuittingAttribute>(typeof(Game), nameof(OnGameQuitting));
				Util.LinkEventWithAttribute<OnGameRestartAttribute>(typeof(Game), nameof(OnGameRestart));
				Util.LinkEventWithAttribute<OnSlotChangedAttribute>(typeof(Game), nameof(OnSlotChanged));
				Util.LinkEventWithAttribute<OnSlotCreatedAttribute>(typeof(Game), nameof(OnSlotCreated));

				_AddGameTryingToQuitListener(OnTryingToQuit);
				_AddGameQuittingListener(OnGameQuitting);
				_AddTextInputListener(CellRendererGUI.OnTextInput);

				Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));
				OnSlotChanged?.Invoke();

				SetDebugEnable(_GetIsEdittime());
				_SetGraphicFramerate(GraphicFramerate);
				_SetVSync(_VSync.Value);
				_SetFullscreenMode((FullscreenMode)_FullscreenMode.Value);
				_SetMusicVolume(MusicVolume);
				_SetSoundVolume(SoundVolume);

				AngeUtil.CreateAngeFolders();

				Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeLaterAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));

				ResourcePool.Clear();
				ResourcePool.AddRange(_ForAllAudioClips());

				RestartGameLogic();
				System.GC.Collect();

				if (IsEdittime) GlobalEditorUI.OpenEditorSmoothly(MapEditor.TYPE_ID, false);

			} catch (System.Exception ex) { LogException(ex); }
			// Func
			static bool OnTryingToQuit () {
				if (IsPausing || IsEdittime) return true;
				IsPlaying = false;
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
				if (FrameInput.GameKeyUp(Gamekey.Start)) IsPlaying = !IsPlaying;

				// Answer Game Restart Require
				if (RequireRestartWithPlayerID.HasValue) RestartGameLogic();

				// Slot Change Detect
				if (_CurrentSaveSlot.Value != AngePath.CurrentSaveSlot) {
					_CurrentSaveSlot.Value = AngePath.CurrentSaveSlot;
					if (!Util.FolderExists(AngePath.SaveSlotRoot)) OnSlotCreated?.Invoke();
					OnSlotChanged?.Invoke();
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


		#endregion




		#region --- API ---


		public static void RestartGame (int playerID = 0, bool immediately = false) {
			RequireRestartWithPlayerID = playerID;
			if (immediately) RestartGameLogic();
		}


		public static void StopGame () {
			WorldSquad.Enable = false;
			Stage.DespawnAllEntitiesFromWorld();
			if (Player.Selecting != null) Player.Selecting.Active = false;
			Player.Selecting = null;

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




		#region --- LGC ---


		private static void RestartGameLogic () {

			WorldSquad.Enable = true;

			// Select New Player
			int playerID = RequireRestartWithPlayerID ?? 0;
			RequireRestartWithPlayerID = null;
			if (Player.Selecting == null || (playerID != Player.Selecting.TypeID)) {
				if (playerID == 0) {
					playerID =
						Player.Selecting != null ? Player.Selecting.TypeID :
						Player.TryGetDefaultSelectPlayer(out var defaultPlayer) ? defaultPlayer.AngeHash() : 0;
				}
				if (playerID != 0) {
					if (Stage.PeekOrGetEntity(playerID) is Player player) {
						Player.Selecting = player;
					}
				}
			}

			// Enable
			if (!IsPlaying) IsPlaying = true;

			// Event
			OnGameRestart?.Invoke();
		}


		private static float GetScaledAudioVolume (int volume, int scale = 1000) {
			float fVolume = volume / 1000f;
			if (scale != 1000) fVolume *= scale / 1000f;
			return fVolume * fVolume;
		}


		[OnGameUpdatePauseless]
		internal static void RefreshGame () {
			// Load or Stop Music
			bool requireMusic = IsPlaying && MusicVolume > 0 && !MapEditor.IsEditing;
			if (requireMusic != IsMusicPlaying) {
				if (requireMusic) {
					UnPauseMusic();
				} else {
					PauseMusic();
				}
			}
		}


		#endregion




	}
}
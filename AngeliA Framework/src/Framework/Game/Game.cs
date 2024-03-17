using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AngeliA.Framework;

[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitializeAttribute : System.Attribute { public int Order; public OnGameInitializeAttribute (int order = 0) => Order = order; }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameInitializeLaterAttribute : System.Attribute { public int Order; public OnGameInitializeLaterAttribute (int order = 0) => Order = order; }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateAttribute : OrderedAttribute { public OnGameUpdateAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdateLaterAttribute : OrderedAttribute { public OnGameUpdateLaterAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameUpdatePauselessAttribute : OrderedAttribute { public OnGameUpdatePauselessAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameRestartAttribute : OrderedAttribute { public OnGameRestartAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameTryingToQuitAttribute : OrderedAttribute { public OnGameTryingToQuitAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameQuittingAttribute : OrderedAttribute { public OnGameQuittingAttribute (int order = 0) : base(order) { } }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameFocusedAttribute : System.Attribute { }
[System.AttributeUsage(System.AttributeTargets.Method)] public class OnGameLostFocusAttribute : System.Attribute { }


public abstract partial class Game {




	#region --- VAR ---


	// Api
	public static Game Instance { get; private set; } = null;
	public static int GlobalFrame { get; private set; } = 0;
	public static int SettleFrame => GlobalFrame - Stage.LastSettleFrame;
	public static int PauselessFrame { get; private set; } = 0;
	public static bool IsPausing => !IsPlaying;
	public static bool IsPlaying { get; private set; } = true;
	public static int MusicVolume {
		get => _MusicVolume.Value;
		set => _MusicVolume.Value = value;
	}
	public static int SoundVolume {
		get => _SoundVolume.Value;
		set => _SoundVolume.Value = value;
	}
	public static float ScaledMusicVolume => Util.GetScaledAudioVolume(_MusicVolume.Value, ProcedureAudioVolume);
	public static float ScaledSoundVolume => Util.GetScaledAudioVolume(_SoundVolume.Value, ProcedureAudioVolume);
	public static int ProcedureAudioVolume { get; set; } = 1000;

	// Attribute Info
	public static ProjectType ProjectType { get; private set; } = ProjectType.Game;
	public static string Title { get; private set; } = "";
	public static string DisplayTitle { get; private set; } = "";
	public static string Developer { get; private set; } = "";
	public static string DeveloperDisplayName { get; private set; } = "";
	public static int MajorVersion { get; private set; } = 0;
	public static int MinorVersion { get; private set; } = 0;
	public static int PatchVersion { get; private set; } = 0;
	public static bool AllowMakerFeatures { get; private set; } = false;

	// Event
	private static event System.Action OnGameRestart;
	private static event System.Action OnGameQuitting;
	private static event System.Action OnGameUpdate;
	private static event System.Action OnGameUpdateLater;
	private static event System.Action OnGameUpdatePauseless;
	private static event System.Action OnGameFocused;
	private static event System.Action OnGameLostFocus;
	private static MethodInfo[] OnGameTryingToQuitMethods;

	// Saving
	private static readonly SavingBool _IsFullscreen = new("Game.IsFullscreen", false);
	private static readonly SavingInt _MusicVolume = new("Game.MusicVolume", 500);
	private static readonly SavingInt _SoundVolume = new("Game.SoundVolume", 1000);
	private static readonly SavingInt _LastUsedWindowWidth = new("Game.LastUsedWindowWidth", 1024 * 16 / 9);
	private static readonly SavingInt _LastUsedWindowHeight = new("Game.LastUsedWindowHeight", 1024);


	#endregion




	#region --- MSG ---


	public Game () {
		Instance = this;
		// Assembly
		Util.AllAssemblies.AddDistinct(typeof(Game).Assembly);
		Util.AllAssemblies.AddDistinct(Assembly.GetCallingAssembly());
		foreach (var dllpath in Util.EnumerateFiles("Library", false, "*.dll")) {
			if (Assembly.LoadFrom(dllpath) is Assembly assembly) {
				Util.AllAssemblies.AddDistinct(assembly);
			}
		}
		// Attribute >> Game
		if (Util.TryGetAttributeFromAllAssemblies<AngeliaProjectTypeAttribute>(out var _project)) {
			ProjectType = _project.Type;
		}
		if (Util.TryGetAttributeFromAllAssemblies<AngeliaGameTitleAttribute>(out var _title)) {
			Title = _title.Title;
			DisplayTitle = _title.DisTitle;
		}
		if (Util.TryGetAttributeFromAllAssemblies<AngeliaGameDeveloperAttribute>(out var _dev)) {
			Developer = _dev.Developer;
			DeveloperDisplayName = _dev.DisName;
		}
		if (Util.TryGetAttributeFromAllAssemblies<AngeliaVersionAttribute>(out var _ver)) {
			MajorVersion = _ver.Version.x;
			MinorVersion = _ver.Version.y;
			PatchVersion = _ver.Version.z;
		}
		if (Util.TryGetAttributeFromAllAssemblies<AngeliaAllowMakerFeaturesAttribute>()) {
			AllowMakerFeatures = true;
		}

	}


	public void Initialize () {
		try {

			GlobalFrame = 0;
			if (IsEdittime) _IsFullscreen.Value = false;

			if (!string.IsNullOrWhiteSpace(DeveloperDisplayName)) {
				_SetWindowTitle($"{DisplayTitle} - {DeveloperDisplayName}");
			} else {
				_SetWindowTitle(DisplayTitle);
			}

			Util.LinkEventWithAttribute<OnGameUpdateAttribute>(typeof(Game), nameof(OnGameUpdate));
			Util.LinkEventWithAttribute<OnGameUpdateLaterAttribute>(typeof(Game), nameof(OnGameUpdateLater));
			Util.LinkEventWithAttribute<OnGameUpdatePauselessAttribute>(typeof(Game), nameof(OnGameUpdatePauseless));
			Util.LinkEventWithAttribute<OnGameQuittingAttribute>(typeof(Game), nameof(OnGameQuitting));
			Util.LinkEventWithAttribute<OnGameRestartAttribute>(typeof(Game), nameof(OnGameRestart));
			Util.LinkEventWithAttribute<OnGameFocusedAttribute>(typeof(Game), nameof(OnGameFocused));
			Util.LinkEventWithAttribute<OnGameLostFocusAttribute>(typeof(Game), nameof(OnGameLostFocus));
			OnGameTryingToQuitMethods = Util.AllStaticMethodWithAttribute<OnGameTryingToQuitAttribute>().Select(selector => selector.Key).ToArray();

			Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));

			_SetFullscreen(_IsFullscreen.Value);
			_SetWindowSize(_LastUsedWindowWidth.Value, _LastUsedWindowHeight.Value);
			_SetMusicVolume(MusicVolume);
			_SetSoundVolume(SoundVolume);

			Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeLaterAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));

			System.GC.Collect();

			// Start Game !!
			switch (ProjectType) {
				case ProjectType.Game:
					if (IsEdittime) {
						WindowUI.OpenWindow(MapEditor.TYPE_ID);
					} else if (AllowMakerFeatures) {
						WindowUI.OpenWindow(HomeScreen.TYPE_ID);
					} else {
						RestartGame();
					}
					break;
				case ProjectType.Application:
					StopGame();
					break;
			}

		} catch (System.Exception ex) { Util.LogException(ex); }
	}


	public void Update () {
		try {

			// Update Callbacks
			if (IsPlaying) {
				OnGameUpdate?.Invoke();
				OnGameUpdateLater?.Invoke();
			}
			OnGameUpdatePauseless?.Invoke();

			// Switch Between Play and Pause
			if (ProjectType == ProjectType.Game && Input.GameKeyUp(Gamekey.Start)) {
				if (IsPlaying) {
					PauseGame();
				} else {
					UnpauseGame();
				}
			}

			// Grow Frame
			if (!IsPausing) GlobalFrame++;
			PauselessFrame++;

		} catch (System.Exception ex) { Util.LogException(ex); }
	}


	[OnGameUpdatePauseless(int.MinValue)]
	internal static void LoadOrStopMusic () {
		bool requireMusic = IsPlaying && ScaledMusicVolume > 0 && !MapEditor.IsEditing;
		if (requireMusic != IsMusicPlaying) {
			if (requireMusic) {
				UnpauseMusic();
			} else {
				PauseMusic();
			}
		}
	}


	[OnGameInitialize(int.MinValue)]
	[OnGameUpdate(int.MinValue)]
	internal static void ScreenSizeCache () {
		int monitor = Instance._GetCurrentMonitor();
		ScreenWidth = Instance._GetScreenWidth();
		ScreenHeight = Instance._GetScreenHeight();
		MonitorWidth = Instance._GetMonitorWidth(monitor);
		MonitorHeight = Instance._GetMonitorHeight(monitor);
	}


	#endregion




	#region --- API ---


	public static void RestartGame () => OnGameRestart?.Invoke();


	public static void StopGame () {
		WorldSquad.Enable = false;
		Stage.DespawnAllNonUiEntities();
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


	#endregion




}
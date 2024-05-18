using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AngeliA;

public abstract partial class Game {




	#region --- VAR ---


	// Api
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
	public static bool IsToolApplication { get; private set; } = false;
	public static bool AllowMakerFeatures { get; private set; } = false;
	public static bool AllowPause { get; private set; } = true;
	public static bool IgnoreArtworkPixels { get; private set; } = false;

	// Event
	private static event System.Action OnGameRestart;
	private static event System.Action OnGameQuitting;
	private static event System.Action OnGameUpdate;
	private static event System.Action OnGameUpdateLater;
	private static event System.Action OnGameUpdatePauseless;
	private static event System.Action OnGameFocused;
	private static event System.Action OnGameLostFocus;
	private static MethodInfo[] OnGameTryingToQuitMethods;

	// Data
	private static Game Instance = null;

	// Saving
	private static readonly SavingBool _IsFullscreen = new("Game.IsFullscreen", false);
	private static readonly SavingInt _MusicVolume = new("Game.MusicVolume", 500);
	private static readonly SavingInt _SoundVolume = new("Game.SoundVolume", 1000);
	private static readonly SavingInt _LastUsedWindowWidth = new("Game.LastUsedWindowWidth", 1024 * 16 / 9);
	private static readonly SavingInt _LastUsedWindowHeight = new("Game.LastUsedWindowHeight", 1024);


	#endregion




	#region --- MSG ---


	static Game () {
		// Framework
		Util.AddAssembly(typeof(Game).Assembly);
		// Game Libs
		foreach (var dllpath in Util.EnumerateFiles("Library", false, "*.dll")) {
			if (Assembly.LoadFrom(dllpath) is Assembly assembly) {
				Util.AddAssembly(assembly);
			}
		}
	}


	public Game () {

		Instance = this;

		// Attribute >> Game
		if (Util.TryGetAttributeFromAllAssemblies<ToolApplicationAttribute>()) {
			IsToolApplication = true;
		}
		if (Util.TryGetAttributeFromAllAssemblies<AllowMakerFeaturesAttribute>()) {
			AllowMakerFeatures = true;
		}
		if (Util.TryGetAttributeFromAllAssemblies<DisablePauseAttribute>()) {
			AllowPause = false;
		}
		if (Util.TryGetAttributeFromAllAssemblies<IgnoreArtworkPixelsAttribute>()) {
			IgnoreArtworkPixels = true;
		}

	}


	public void Initialize () {
		try {

			GlobalFrame = 0;
			ScreenSizeCache();

#if DEBUG
			_IsFullscreen.Value = false;
#endif
			Util.LinkEventWithAttribute<OnGameUpdateAttribute>(typeof(Game), nameof(OnGameUpdate));
			Util.LinkEventWithAttribute<OnGameUpdateLaterAttribute>(typeof(Game), nameof(OnGameUpdateLater));
			Util.LinkEventWithAttribute<OnGameUpdatePauselessAttribute>(typeof(Game), nameof(OnGameUpdatePauseless));
			Util.LinkEventWithAttribute<OnGameQuittingAttribute>(typeof(Game), nameof(OnGameQuitting));
			Util.LinkEventWithAttribute<OnGameRestartAttribute>(typeof(Game), nameof(OnGameRestart));
			Util.LinkEventWithAttribute<OnGameFocusedAttribute>(typeof(Game), nameof(OnGameFocused));
			Util.LinkEventWithAttribute<OnGameLostFocusAttribute>(typeof(Game), nameof(OnGameLostFocus));
			OnGameTryingToQuitMethods = Util.AllStaticMethodWithAttribute<OnGameTryingToQuitAttribute>().Select(selector => selector.Key).ToArray();

			Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));

			SyncAudioPool(UniverseSystem.BuiltInUniverse.UniverseRoot);
			_SetFullscreen(_IsFullscreen.Value);
			_SetWindowSize(_LastUsedWindowWidth.Value, _LastUsedWindowHeight.Value);
			_SetMusicVolume(MusicVolume);
			_SetSoundVolume(SoundVolume);

			Util.InvokeAllStaticMethodWithAttribute<OnGameInitializeLaterAttribute>((a, b) => a.Value.Order.CompareTo(b.Value.Order));

			System.GC.Collect();

			// Start Game !!
			if (IsToolApplication) {
				StopGame();
			} else {
#if DEBUG
				WindowUI.OpenWindow(MapEditor.TYPE_ID);
#else
				if (AllowMakerFeatures) {
					WindowUI.OpenWindow(HomeScreen.TYPE_ID);
				} else {
					RestartGame();
				}
#endif
			}

		} catch (System.Exception ex) { Debug.LogException(ex); }
	}


	public void Update () {
		try {

			// Update Callbacks
			UpdateGuiInput();
			if (IsPlaying) {
				ScreenSizeCache();
				OnGameUpdate?.Invoke();
				OnGameUpdateLater?.Invoke();
			}
			LoadOrStopMusic();
			OnGameUpdatePauseless?.Invoke();

			// Switch Between Play and Pause
			if (AllowPause) {
				if (Input.GameKeyUp(Gamekey.Start)) {
					if (IsPlaying) {
						PauseGame();
					} else {
						UnpauseGame();
					}
				}
			} else if (IsPausing) {
				UnpauseGame();
			}

			// Grow Frame
			if (!IsPausing) GlobalFrame++;
			PauselessFrame++;

		} catch (System.Exception ex) { Debug.LogException(ex); }
	}


	private void UpdateGuiInput () {
		try {
			// Update Pressing Chars
			char c;
			PressingCharCount = 0;
			for (int safe = 0; (c = GetCharPressed()) > 0 && safe < 256; safe++) {
				// GUI
				GUI.OnTextInput(c);
				// List
				PressingCharsForCurrentFrame[PressingCharCount] = c;
				PressingCharCount++;
			}
			// Update Pressing Keys
			KeyboardKey? key;
			PressingKeyCount = 0;
			bool ctrl = IsKeyboardKeyHolding(KeyboardKey.LeftCtrl);
			for (int safe = 0; (key = GetKeyPressed()).HasValue && safe < 256; safe++) {
				// GUI
				switch (key) {
					case KeyboardKey.Enter:
						GUI.OnTextInput(Const.RETURN_SIGN);
						break;
					case KeyboardKey.C:
						if (ctrl) {
							GUI.OnTextInput(Const.CONTROL_COPY);
						}
						break;
					case KeyboardKey.X:
						if (ctrl) {
							GUI.OnTextInput(Const.CONTROL_CUT);
						}
						break;
					case KeyboardKey.V:
						if (ctrl) {
							GUI.OnTextInput(Const.CONTROL_PASTE);
						}
						break;
					case KeyboardKey.A:
						if (ctrl) {
							GUI.OnTextInput(Const.CONTROL_SELECT_ALL);
						}
						break;
					case KeyboardKey.Backspace:
						GUI.OnTextInput(Const.BACKSPACE_SIGN);
						break;
				}
				// List
				PressingKeysForCurrentFrame[PressingKeyCount] = key.Value;
				PressingKeyCount++;
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
	}


	private void LoadOrStopMusic () {
		bool requireMusic = IsPlaying && ScaledMusicVolume > 0 && !MapEditor.IsEditing;
		if (requireMusic != IsMusicPlaying) {
			if (requireMusic) {
				UnpauseMusic();
			} else {
				PauseMusic();
			}
		}
	}


	private void ScreenSizeCache () {
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


	public static void UnpauseGame () => IsPlaying = true;


	public static void PauseGame () {
		if (!IsPlaying || !AllowPause) return;
		StopAllSounds();
		IsPlaying = false;
	}


	// Invoke
	protected void InvokeGameQuitting () {
		int width = _GetScreenWidth();
		int height = _GetScreenHeight();
		if (width > 0 && height > 0) {
			_LastUsedWindowWidth.Value = width;
			_LastUsedWindowHeight.Value = height;
		}
		ClearAndUnloadAudioPool();
		OnGameQuitting?.Invoke();
	}

	protected bool InvokeGameTryingToQuit () {
		if (!IsToolApplication && !IsPausing) PauseGame();
		foreach (var method in OnGameTryingToQuitMethods) {
			if (method.Invoke(null, null) is bool result && !result) {
				return false;
			}
		}
		return true;
	}

	protected void InvokeWindowFocusChanged (bool focus) => (focus ? OnGameFocused : OnGameLostFocus)?.Invoke();


	#endregion




}
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
	public static bool UseProceduralMap { get; private set; }
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
	protected object CurrentBGM { get; set; }

	// Attribute Info
	public static bool IsToolApplication { get; private set; } = false;
	public static bool AllowPause { get; private set; } = true;
	public static bool IgnoreArtworkPixels { get; private set; } = false;
	public static bool AllowPlayerRestart { get; private set; } = true;
	public static bool ForceUnifyBasedOnMonitor { get; private set; } = true;
	public static bool NoQuitFromMenu { get; private set; } = false;

	// Event
	private static event System.Action OnGameRestart;
	private static event System.Action OnGameQuitting;
	private static event System.Action OnGameUpdate;
	private static event System.Action OnGameUpdateLater;
	private static event System.Action OnGameUpdatePauseless;
	private static event System.Action OnGameFocused;
	private static event System.Action OnGameLostFocus;
	private static event System.Action<string> OnFileDropped;
	private static MethodInfo[] OnGameTryingToQuitMethods;

	// Data
	private static Game Instance = null;
	public static readonly Dictionary<int, SoundData> SoundPool = new();
	public static readonly Dictionary<int, MusicData> MusicPool = new();
	public static readonly List<FontData> Fonts = new();
	private static readonly HashSet<int> CacheForAudioSync = new();
	private static readonly List<int> CacheForAudioSyncRemove = new();
	private static readonly int[] ScreenEffectEnableFrames = new int[Const.SCREEN_EFFECT_COUNT].FillWithValue(-1);
	private readonly char[] PressingCharsForCurrentFrame = new char[256];
	private readonly KeyboardKey[] PressingKeysForCurrentFrame = new KeyboardKey[256];
	private int PressingCharCount = 0;
	private int PressingKeyCount = 0;
	private int ForceMinViewHeightValue;
	private int ForceMinViewHeightFrame = -1;
	private int ForceMaxViewHeightValue;
	private int ForceMaxViewHeightFrame = -1;

	// Saving
	private static readonly SavingBool _IsFullscreen = new("Game.IsFullscreen", false, SavingLocation.Global);
	private static readonly SavingInt _MusicVolume = new("Game.MusicVolume", 500, SavingLocation.Global);
	private static readonly SavingInt _SoundVolume = new("Game.SoundVolume", 1000, SavingLocation.Global);
	private static readonly SavingInt _LastUsedWindowWidth = new("Game.LastUsedWindowWidth", 1024 * 16 / 9, SavingLocation.Global);
	private static readonly SavingInt _LastUsedWindowHeight = new("Game.LastUsedWindowHeight", 1024, SavingLocation.Global);


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


	public Game (params string[] args) {

		Instance = this;

		// Args
		string universeRoot = null;
		for (int i = 0; i < args.Length; i++) {
			string arg = args[i];
			try {
				// Load Assemblies from Args
				if (arg.StartsWith("-lib:")) {
					string path = Util.ArgPath_to_Path(arg[5..]);
					if (Util.PathIsFolder(path)) {
						if (!Util.FolderExists(path)) continue;
						foreach (var dllpath in Util.EnumerateFiles(path, false, "*.dll")) {
							if (Assembly.LoadFrom(dllpath) is Assembly assembly) {
								Util.AddAssembly(assembly);
							}
						}
					} else {
						if (!Util.FileExists(path)) continue;
						if (Assembly.LoadFrom(path) is Assembly assembly) {
							Util.AddAssembly(assembly);
						}
					}
				} else if (arg.StartsWith("-uni:")) {
					// Set Universe Path from Args
					universeRoot = Util.ArgPath_to_Path(arg[5..]);
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
		universeRoot ??= AngePath.GetUniverseRoot(System.Environment.CurrentDirectory);
		if (!Util.FolderExists(universeRoot)) {
			universeRoot = AngePath.GetUniverseRoot(Util.GetParentPath(System.Environment.CurrentDirectory));
		}
		AngePath.BuiltInUniverseRoot = universeRoot;

		// Attribute >> Game
		if (Util.TryGetAttributeFromAllAssemblies<ToolApplicationAttribute>()) {
			IsToolApplication = true;
		}
		if (Util.TryGetAttributeFromAllAssemblies<DisablePauseAttribute>()) {
			AllowPause = false;
		}
		if (Util.TryGetAttributeFromAllAssemblies<IgnoreArtworkPixelsAttribute>()) {
			IgnoreArtworkPixels = true;
		}
		if (Util.TryGetAttributeFromAllAssemblies<PlayerCanNotRestartGameAttribute>()) {
			AllowPlayerRestart = false;
		}
		if (Util.TryGetAttributeFromAllAssemblies<ScaleUiBasedOnScreenHeightAttribute>()) {
			ForceUnifyBasedOnMonitor = false;
		}
		if (Util.TryGetAttributeFromAllAssemblies<NoQuitFromMenuAttribute>()) {
			NoQuitFromMenu = true;
		}

	}


	public void Initialize () {
		try {

			GlobalFrame = 0;
			ScreenSizeCache();
			LoadFontsIntoPool(Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Fonts"), builtIn: true);

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
			Util.LinkEventWithAttribute<OnFileDroppedAttribute>(typeof(Game), nameof(OnFileDropped));

			OnGameTryingToQuitMethods = Util.AllStaticMethodWithAttribute<OnGameTryingToQuitAttribute>().Select(selector => selector.Key).ToArray();

			Util.InvokeAsAutoOrderingTask<OnGameInitializeAttribute>();

			_SetFullscreen(_IsFullscreen.Value);
			_SetWindowSize(_LastUsedWindowWidth.Value, _LastUsedWindowHeight.Value);
			_SetMusicVolume(MusicVolume);
			_SetSoundVolume(SoundVolume);

			Util.InvokeAsAutoOrderingTask<OnGameInitializeLaterAttribute>();

			System.GC.Collect();

			// Start Game !!
			if (IsToolApplication) {
				StopGame();
			} else {
				RestartGame();
			}

		} catch (System.Exception ex) { Debug.LogException(ex); }
	}


	public void Update () {
		try {

			// Update Callbacks
			UpdateGuiInput();
			ScreenSizeCache();
			if (IsPlaying) {
				OnGameUpdate?.Invoke();
				OnGameUpdateLater?.Invoke();
			}
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
				if (GUI.IsTyping) GUI.OnTextInput(c);
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
				if (GUI.IsTyping) {
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
					}
				}
				// List
				PressingKeysForCurrentFrame[PressingKeyCount] = key.Value;
				PressingKeyCount++;
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
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


	// Fonts
	public static void LoadFontsIntoPool (string rootPath, bool builtIn) {
		if (builtIn) {
			for (int i = 0; i < Fonts.Count; i++) {
				var font = Fonts[i];
				if (font.BuiltIn) {
					font.Unload();
					Fonts.RemoveAt(i);
					i--;
				}
			}
		}
		foreach (var fontPath in Util.EnumerateFiles(rootPath, true, "*.ttf")) {
			if (Fonts.Any(font => font.Path == fontPath)) continue;
			var data = Instance.CreateNewFontData();
			if (data == null || !data.LoadFromFile(fontPath, builtIn)) continue;
			Fonts.Add(data);
		}
		Fonts.Sort((a, b) => {
			int result = b.BuiltIn.CompareTo(a.BuiltIn);
			return result != 0 ? result : a.LocalLayerIndex.CompareTo(b.LocalLayerIndex);
		});
		if (builtIn) {
			BuiltInFontCount = Fonts.Count(font => font.BuiltIn);
		}
	}

	public static bool SyncFontsWithPool (string rootPath) {
		bool fontChanged = false;
		for (int i = 0; i < Fonts.Count; i++) {
			var font = Fonts[i];
			if (font.BuiltIn) continue;
			if (!Util.FileExists(font.Path)) {
				// File Deleted
				font.Unload();
				Fonts.RemoveAt(i);
				i--;
				fontChanged = true;
				continue;
			}
			if (Util.GetFileModifyDate(font.Path) != font.FileModifyDate) {
				// File Modified
				font.Unload();
				bool loaded = font.LoadFromFile(font.Path, font.BuiltIn);
				if (!loaded) {
					Fonts.RemoveAt(i);
					i--;
				}
				fontChanged = true;
			}
		}
		foreach (var fontPath in Util.EnumerateFiles(rootPath, true, "*.ttf")) {
			if (Fonts.Any(font => font.Path == fontPath)) continue;
			// Load New Font
			var data = Instance.CreateNewFontData();
			if (data == null || !data.LoadFromFile(fontPath, builtIn: false)) continue;
			Fonts.Add(data);
			fontChanged = true;
		}
		if (fontChanged) {
			Fonts.Sort((a, b) => {
				int result = b.BuiltIn.CompareTo(a.BuiltIn);
				return result != 0 ? result : a.LocalLayerIndex.CompareTo(b.LocalLayerIndex);
			});
			Renderer.ClearCharSpritePool();
		}
		return fontChanged;
	}

	public static void UnloadFontsFromPool (bool ignoreBuiltIn = true) {
		for (int i = 0; i < Fonts.Count; i++) {
			var font = Fonts[i];
			if (ignoreBuiltIn && font.BuiltIn) continue;
			font.Unload();
			Fonts.RemoveAt(i);
			i--;
		}
		Renderer.ClearCharSpritePool();
		if (!ignoreBuiltIn) BuiltInFontCount = 0;
	}


	// Audio
	public static void SyncAudioPool (params string[] universeRoots) {

		// Music
		CacheForAudioSync.Clear();
		CacheForAudioSyncRemove.Clear();
		foreach (string root in universeRoots) {
			foreach (var path in Util.EnumerateFiles(AngePath.GetUniverseMusicRoot(root), false, "*.wav", "*.mp3", "*.ogg")) {
				int id = Util.GetNameWithoutExtension(path).TrimEnd(' ').AngeHash();
				CacheForAudioSync.Add(id);
				if (!MusicPool.ContainsKey(id)) {
					MusicPool.Add(id, new MusicData() {
						ID = id,
						Name = Util.GetNameWithoutExtension(path),
						Path = path,
					});
				}
			}
		}
		foreach (var (id, _) in MusicPool) {
			if (!CacheForAudioSync.Contains(id)) {
				CacheForAudioSyncRemove.Add(id);
			}
		}
		foreach (int id in CacheForAudioSyncRemove) {
			MusicPool.Remove(id);
		}

		// Sound
		CacheForAudioSync.Clear();
		CacheForAudioSyncRemove.Clear();
		foreach (string root in universeRoots) {
			foreach (var path in Util.EnumerateFiles(AngePath.GetUniverseSoundRoot(root), false, "*.wav", "*.mp3", "*.ogg")) {
				int id = Util.GetNameWithoutExtension(path).AngeHash();
				CacheForAudioSync.Add(id);
				if (SoundPool.ContainsKey(id)) continue;
				var soundObj = LoadSound(path);
				if (soundObj == null) continue;
				SoundPool.Add(id, new SoundData() {
					ID = id,
					Name = Util.GetNameWithoutExtension(path),
					Path = path,
					Data = soundObj,
				});
			}
		}
		foreach (var (id, sound) in SoundPool) {
			if (!CacheForAudioSync.Contains(id)) {
				UnloadSound(sound);
				CacheForAudioSyncRemove.Add(id);
			}
		}
		foreach (int id in CacheForAudioSyncRemove) {
			SoundPool.Remove(id);
		}

	}

	public static void ClearAndUnloadAudioPool () {
		UnloadMusic(Instance.CurrentBGM);
		foreach (var (_, sound) in SoundPool) {
			UnloadSound(sound);
		}
		MusicPool.Clear();
		SoundPool.Clear();
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
		UnloadFontsFromPool(ignoreBuiltIn: false);
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

	protected void InvokeFileDropped (string path) => OnFileDropped?.Invoke(path);


	// Event
	[OnGameInitialize(int.MinValue + 1)]
	internal static void InitCache () {
		UseProceduralMap = Universe.BuiltIn.Info.UseProceduralMap;
	}


	#endregion




}
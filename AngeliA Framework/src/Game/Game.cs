using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AngeliA;

/// <summary>
/// Represent an AngeliA game
/// </summary>
public abstract partial class Game {




	#region --- VAR ---


	// Api
	/// <summary>
	/// Frame number that grows every 1/60 second
	/// </summary>
	public static int GlobalFrame { get; private set; } = 0;
	/// <summary>
	/// Frame number that could be reset by the stage
	/// </summary>
	public static int SettleFrame => GlobalFrame - Stage.LastSettleFrame;
	/// <summary>
	/// Frame number that still grows when the game is pausing
	/// </summary>
	public static int PauselessFrame { get; private set; } = 0;
	/// <summary>
	/// True if the game is currently pausing
	/// </summary>
	public static bool IsPausing => !IsPlaying;
	/// <summary>
	/// True if the game is currently not pausing
	/// </summary>
	public static bool IsPlaying { get; private set; } = true;
	/// <summary>
	/// Volume amount for the background music (0 for mute, 1000 for loudest)
	/// </summary>
	public static int MusicVolume {
		get => _MusicVolume.Value;
		set => _MusicVolume.Value = value;
	}
	/// <summary>
	/// Volume amount for the sound effect (0 for mute, 1000 for loudest)
	/// </summary>
	public static int SoundVolume {
		get => _SoundVolume.Value;
		set => _SoundVolume.Value = value;
	}
	/// <summary>
	/// Music volume that effect by the internal procedure audio volume
	/// </summary>
	public static float ScaledMusicVolume => FrameworkUtil.GetScaledAudioVolume(_MusicVolume.Value, ProcedureAudioVolume);
	/// <summary>
	/// Sound volume that effect by the internal procedure audio volume
	/// </summary>
	public static float ScaledSoundVolume => FrameworkUtil.GetScaledAudioVolume(_SoundVolume.Value, ProcedureAudioVolume);
	/// <summary>
	/// Audio volume used by internal script
	/// </summary>
	public static int ProcedureAudioVolume {
		get => _ProcedureAudioVolume;
		set {
			if (_ProcedureAudioVolume == value) return;
			_ProcedureAudioVolume = value;
			SetMusicVolume(_MusicVolume.Value);
		}
	}
	/// <summary>
	/// Instance that represent the background music
	/// </summary>
	public static object CurrentBGM { get; protected set; }

	// Attribute Info
	/// <summary>
	/// True if the application should be treated as tool instead of game
	/// </summary>
	public static bool IsToolApplication { get; private set; } = false;
	/// <summary>
	/// True if the application don't need pixel data from artwork sheet
	/// </summary>
	public static bool IgnoreArtworkPixels { get; private set; } = false;

	// Event
	[OnGameRestart] internal static System.Action OnGameRestart;
	[OnGameQuitting] internal static System.Action OnGameQuitting;
	[OnGameUpdate] internal static System.Action OnGameUpdate;
	[OnGameUpdateLater] internal static System.Action OnGameUpdateLater;
	[OnGameUpdatePauseless] internal static System.Action OnGameUpdatePauseless;
	[OnGameFocused] internal static System.Action OnGameFocused;
	[OnGameLostFocus] internal static System.Action OnGameLostFocus;
	[OnWindowSizeChanged] internal static System.Action OnWindowSizeChanged;
	[OnFileDropped_StringPath] internal static System.Action<string> OnFileDropped;
	private static MethodInfo[] OnGameTryingToQuitMethods;

	// Data
	private static Game Instance = null;
	/// <summary>
	/// Instance pool for all available sound effect
	/// </summary>
	public static readonly Dictionary<int, SoundData> SoundPool = [];
	/// <summary>
	/// Instance pool for all available background music
	/// </summary>
	public static readonly Dictionary<int, MusicData> MusicPool = [];
	/// <summary>
	/// Instance pool for all available font
	/// </summary>
	public static readonly List<FontData> Fonts = [];
	private static readonly HashSet<int> CacheForAudioSync = [];
	private static readonly List<int> CacheForAudioSyncRemove = [];
	private static readonly int[] ScreenEffectEnableFrames = new int[Const.SCREEN_EFFECT_COUNT].FillWithValue(-1);
	private static int _ProcedureAudioVolume = 1000;
	private readonly char[] PressingCharsForCurrentFrame = new char[256];
	private readonly KeyboardKey[] PressingKeysForCurrentFrame = new KeyboardKey[256];
	private int PressingCharCount = 0;
	private int PressingKeyCount = 0;
	// Saving
	private static readonly SavingBool _IsFullscreen = new("Game.IsFullscreen", false, SavingLocation.Global);
	private static readonly SavingInt _MusicVolume = new("Game.MusicVolume", 500, SavingLocation.Global);
	private static readonly SavingInt _SoundVolume = new("Game.SoundVolume", 700, SavingLocation.Global);
	private static readonly SavingInt _LastUsedWindowWidth = new("Game.LastUsedWindowWidth", 1024 * 16 / 9, SavingLocation.Global);
	private static readonly SavingInt _LastUsedWindowHeight = new("Game.LastUsedWindowHeight", 1024, SavingLocation.Global);


	#endregion




	#region --- MSG ---


	static Game () {
		// Framework
		Util.AddAssembly(typeof(Game).Assembly);
		// Game Libs
		foreach (var dllpath in Util.EnumerateFiles("Library", true, "*.dll")) {
			if (Assembly.LoadFrom(dllpath) is Assembly assembly) {
				Util.AddAssembly(assembly);
			}
		}
	}


	/// <summary>
	/// Create a game instance with command-line arguments 
	/// </summary>
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
						foreach (var dllpath in Util.EnumerateFiles(path, true, "*.dll")) {
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
		if (Util.TryGetAttributeFromAllAssemblies<IgnoreArtworkPixelsAttribute>()) {
			IgnoreArtworkPixels = true;
		}

	}


	/// <summary>
	/// Call this function once to initialize the game. Must be called before any Update function called.
	/// </summary>
	public void Initialize () {
		try {

			GlobalFrame = 0;
			UpdateCache();
			LoadFontsIntoPool(Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Fonts"), builtIn: true);

#if DEBUG
			_IsFullscreen.Value = false;
#endif
			OnGameTryingToQuitMethods = Util.AllStaticMethodWithAttribute<OnGameTryingToQuitAttribute>().Select(selector => selector.Key).ToArray();

			OrderedAttribute.InvokeAsAutoOrderingTask<OnGameInitializeAttribute>();

			_SetFullscreen(_IsFullscreen.Value);
			_SetWindowSize(_LastUsedWindowWidth.Value, _LastUsedWindowHeight.Value);
			_SetMusicVolume(MusicVolume);
			_SetSoundVolume(SoundVolume);

			OrderedAttribute.InvokeAsAutoOrderingTask<OnGameInitializeLaterAttribute>();

			if (!IsToolApplication) {
				SetWindowTitle(Universe.BuiltInInfo.ProductName);
				SetWindowIcon("WindowIcon".AngeHash());
			}
			System.GC.Collect();

			// Start Game !!
			if (IsToolApplication) {
				WorldSquad.Enable = false;
				Stage.DespawnAllNonUiEntities();
			} else {
				RestartGame();
			}

		} catch (System.Exception ex) { Debug.LogException(ex); }
	}


	/// <summary>
	/// Call this function 60 times per second. Only call this after Initialize has been called.
	/// </summary>
	public void Update () {
		try {

			UpdateWindow();
			UpdateGuiInput();
			UpdateCache();

			// Update Callbacks
			if (IsPlaying) {
				OnGameUpdate?.Invoke();
				OnGameUpdateLater?.Invoke();
			}
			OnGameUpdatePauseless?.Invoke();

			// Switch Between Play and Pause
			if (Universe.BuiltInInfo.AllowPause) {
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


	private void UpdateWindow () {
		// Fix Window Pos in Screen
		if (!IsMouseLeftHolding && PauselessFrame % 42 == 0 && !IsWindowMaximized && !IsWindowMinimized) {
			var pos = GetWindowPosition();
			int monitor = Instance._GetCurrentMonitor();
			int screenW = Instance._GetScreenWidth();
			int monitorW = Instance._GetMonitorWidth(monitor);
			int monitorH = Instance._GetMonitorHeight(monitor);
			int PADDING = (monitorH / 22).GreaterOrEquel(100);
			var newPos = pos.Clamped(
				-screenW + PADDING,
				PADDING,
				monitorW - PADDING,
				monitorH - PADDING
			);
			if (newPos != pos) {
				SetWindowPosition(newPos.x, newPos.y);
			}
		}
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


	private void UpdateCache () {
		int monitor = Instance._GetCurrentMonitor();
		int newScreenW = Instance._GetScreenWidth();
		int newScreenH = Instance._GetScreenHeight();
		if (ScreenWidth != newScreenW || ScreenHeight != newScreenH) {
			ScreenWidth = newScreenW;
			ScreenHeight = newScreenH;
			OnWindowSizeChanged?.Invoke();
		}
		MonitorWidth = Instance._GetMonitorWidth(monitor);
		MonitorHeight = Instance._GetMonitorHeight(monitor);
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Invoke the OnGameRestart event. The game-play logic will be reset after this is called
	/// </summary>
	public static void RestartGame () => OnGameRestart?.Invoke();

	/// <summary>
	/// Continue game from pausing
	/// </summary>
	public static void UnpauseGame () => IsPlaying = true;

	/// <summary>
	/// Pause the game from playing
	/// </summary>
	public static void PauseGame () {
		if (!IsPlaying || !Universe.BuiltInInfo.AllowPause) return;
		StopAllSounds();
		IsPlaying = false;
	}


	// Fonts
	/// <summary>
	/// Load font file into system pool from given folder
	/// </summary>
	/// <param name="rootPath"></param>
	/// <param name="builtIn">True if the fonts are used for built-in font</param>
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
		foreach (var fontPath in Util.EnumerateFiles(rootPath, false, "*.ttf")) {
			if (Fonts.Any(font => font.Path == fontPath)) continue;
			var data = Instance.CreateNewFontData();
			if (data == null || !data.LoadFromFile(fontPath, builtIn)) continue;
			Fonts.Add(data);
		}
		Fonts.Sort((a, b) => {
			int result = b.BuiltIn.CompareTo(a.BuiltIn);
			return result != 0 ? result : a.Name.CompareTo(b.Name);
		});
		if (builtIn) {
			BuiltInFontCount = Fonts.Count(font => font.BuiltIn);
		}

		Renderer.ClearFontIndexIdMap();
	}

	/// <summary>
	/// Reload font file if any font is modified
	/// </summary>
	/// <param name="rootPath"></param>
	/// <returns></returns>
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
		foreach (var fontPath in Util.EnumerateFiles(rootPath, false, "*.ttf")) {
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
				return result != 0 ? result : a.Name.CompareTo(b.Name);
			});
			Renderer.ClearCharSpritePool();
			Renderer.ClearFontIndexIdMap();
		}
		return fontChanged;
	}

	/// <summary>
	/// Unload fonts from system pool
	/// </summary>
	public static void UnloadFontsFromPool (bool ignoreBuiltIn = true) {
		for (int i = 0; i < Fonts.Count; i++) {
			var font = Fonts[i];
			if (ignoreBuiltIn && font.BuiltIn) continue;
			font.Unload();
			Fonts.RemoveAt(i);
			i--;
		}
		Renderer.ClearCharSpritePool();
		Renderer.ClearFontIndexIdMap();
		if (!ignoreBuiltIn) BuiltInFontCount = 0;
	}


	// Audio
	/// <summary>
	/// Update audio files between system pool and file
	/// </summary>
	/// <param name="universeRoots">Folder path of the universe</param>
	public static void SyncAudioPool (params string[] universeRoots) {

		// Music
		CacheForAudioSync.Clear();
		CacheForAudioSyncRemove.Clear();
		foreach (string root in universeRoots) {
			foreach (var path in Util.EnumerateFiles(AngePath.GetUniverseMusicRoot(root), false, "*.wav", "*.mp3", "*.ogg", "*.xm", "*.mod")) {
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
			foreach (var path in Util.EnumerateFiles(AngePath.GetUniverseSoundRoot(root), false, "*.wav", "*.mp3", "*.ogg", "*.xm", "*.mod")) {
				int id = Util.GetNameWithoutExtension(path).AngeHash();
				CacheForAudioSync.Add(id);
				if (SoundPool.ContainsKey(id)) continue;
				var soundObj = LoadSound(path);
				if (soundObj == null) continue;
				var sObjs = new object[Const.SOUND_CHANNEL_COUNT];
				sObjs[0] = soundObj;
				for (int i = 1; i < Const.SOUND_CHANNEL_COUNT; i++) {
					sObjs[i] = LoadSoundAlias(soundObj);
				}
				SoundPool.Add(id, new SoundData() {
					ID = id,
					Name = Util.GetNameWithoutExtension(path),
					Path = path,
					SoundObjects = sObjs,
					StartFrames = new int[Const.SOUND_CHANNEL_COUNT].FillWithValue(int.MinValue),
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

	/// <summary>
	/// Reset audio pool and unload the data in the memory
	/// </summary>
	public static void ClearAndUnloadAudioPool () {
		UnloadMusic(CurrentBGM);
		foreach (var (_, sound) in SoundPool) {
			UnloadSound(sound);
		}
		MusicPool.Clear();
		SoundPool.Clear();
	}


	// Invoke
	/// <summary>
	/// Invoke the OnGameQuitting event
	/// </summary>
	protected void InvokeGameQuitting () {
		int width = _GetScreenWidth();
		int height = _GetScreenHeight();
		if (width > 0 && height > 0) {
			_LastUsedWindowWidth.Value = width;
			_LastUsedWindowHeight.Value = height;
		}
		ClearAndUnloadAudioPool();
		UnloadFontsFromPool(ignoreBuiltIn: false);
		OnGameQuitting?.InvokeSafe();
	}

	/// <summary>
	/// Invoke the OnGameTryingToQuit event
	/// </summary>
	/// <returns>True if the game should quit</returns>
	protected bool InvokeGameTryingToQuit () {
#if DEBUG
		if (!IsKeyboardKeyHolding(KeyboardKey.LeftCtrl) && !IsToolApplication) return true;
#endif
		if (!IsToolApplication && !IsPausing) PauseGame();
		foreach (var method in OnGameTryingToQuitMethods) {
			if (method.Invoke(null, null) is bool result && !result) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Invoke OnGameFocus/OnGameLostFocus event
	/// </summary>
	protected void InvokeWindowFocusChanged (bool focus) => (focus ? OnGameFocused : OnGameLostFocus)?.Invoke();

	/// <summary>
	/// Invoke OnFileDropped event
	/// </summary>
	/// <param name="path">Path of the dropped file</param>
	protected void InvokeFileDropped (string path) => OnFileDropped?.Invoke(path);


	#endregion




}
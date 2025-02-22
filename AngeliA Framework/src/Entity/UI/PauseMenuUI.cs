using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.Capacity(1, 1)]
[EntityAttribute.StageOrder(4096)]
public class PauseMenuUI : MenuUI {




	#region --- SUB ---


	private enum MenuMode { Root, Setting, KeySetter, Restart, Quit, Setter_Keyboard, Setter_Gamepad }


	#endregion




	#region --- VAR ---


	// Const 
	private static readonly LanguageCode MENU_QUIT_MESSAGE = ("Menu.Pause.QuitMessage", "Quit Game?");
	private static readonly LanguageCode MENU_RESTART_MESSAGE = ("Menu.Pause.RestartMessage", "Restart Game?");
	private static readonly LanguageCode MENU_KEYSETTER_GAMEPAD_MESSAGE = ("Menu.KeySetter.GamepadMessage", "Press F1 key to reset");
	private static readonly LanguageCode MENU_KEYSETTER_KEYBOARD_MESSAGE = ("Menu.KeySetter.KeyboardMessage", "Press F1 key to reset");
	private static readonly LanguageCode MENU_KEYSETTER_CONFIRM_MESSAGE = ("Menu.KeySetter.ConfirmMessage", "Save the changes?");
	private static readonly LanguageCode MENU_KEY_SETTER = ("Menu.Pause.KeySetter", "Key Assignment");
	private static readonly LanguageCode MENU_SETTER_KEYBOARD = ("Menu.KeySetter.Keyboard", "Keyboard");
	private static readonly LanguageCode MENU_SETTER_GAMEPAD = ("Menu.KeySetter.Gamepad", "Gamepad");
	private static readonly LanguageCode MENU_SETTER_RECORD = ("Menu.KeySetter.Record", "Press key u want");
	private static readonly LanguageCode MENU_MUSIC_VOLUME = ("Menu.Setting.MusicVolume", "Music Volume");
	private static readonly LanguageCode MENU_SOUND_VOLUME = ("Menu.Setting.SoundVolume", "Sound Volume");
	private static readonly LanguageCode MENU_LANGUAGE = ("Menu.Setting.Language", "Language");
	private static readonly LanguageCode MENU_KEYSETTER_SAVE_BACK = ("Menu.KeySetter.SaveAndBack", "Save and Back");
	private static readonly LanguageCode MENU_FULLSCREEN_LABEL = ("Menu.Setting.Fullscreen.Label", "Fullscreen");
	private static readonly LanguageCode MENU_CONTROL_HINT = ("Menu.Setting.UseControlHint", "Show Control Hint");
	private static readonly LanguageCode MENU_GAMEPAD_HINT = ("Menu.Setting.UseGamepadHint", "Show Gamepad Hint");
	private static readonly LanguageCode MENU_ALLOW_GAMEPAD = ("Menu.Setting.AllowGamepad", "Allow Gamepad");
	private static readonly LanguageCode[] GAMEKEY_UI_CODES = [
		($"UI.GameKey.{Gamekey.Left}", "Left"),
		($"UI.GameKey.{Gamekey.Right}", "Right"),
		($"UI.GameKey.{Gamekey.Down}", "Down"),
		($"UI.GameKey.{Gamekey.Up}", "Up"),
		($"UI.GameKey.{Gamekey.Action}", "Action"),
		($"UI.GameKey.{Gamekey.Jump}", "Jump"),
		($"UI.GameKey.{Gamekey.Start}", "Start"),
		($"UI.GameKey.{Gamekey.Select}", "Select"),
	];

	// Data
	private static PauseMenuUI Instance = null;
	private readonly KeyboardKey[] KeyboardKeys = new KeyboardKey[8];
	private readonly GamepadKey[] GamepadKeys = new GamepadKey[8];
	private readonly IntToChars MusicVolumeCache = new();
	private readonly IntToChars SoundVolumeCache = new();
	private MenuMode Mode = MenuMode.Root;
	private MenuMode RequireMode = MenuMode.Root;
	private int RecordingKey = -1;
	private bool RecordLock = true;
	private bool RecordDirty = false;
	private bool KeySetterConfirming = false;
	private bool BgmPlayingBefore = false;


	#endregion




	#region --- MSG ---


	public PauseMenuUI () {
		Instance = this;
		DefaultLabelStyle = GUI.Skin.Label;
		DefaultContentStyle = GUI.Skin.CenterLabel;
	}


	[OnGameTryingToQuit]
	public static bool OnGameTryingToQuit () {
		if (Game.IsToolApplication) return true;
		Stage.TrySpawnEntity(Instance.TypeID, 0, 0, out _);
		Instance.Mode = Instance.RequireMode = MenuMode.Quit;
		return false;
	}


	[OnGameUpdatePauseless]
	public static void OnGameUpdatePauseless () {
		if (Instance == null || !Game.IsPausing || Instance.Active) return;
		Stage.TrySpawnEntity(Instance.TypeID, 0, 0, out _);
	}


	public override void OnActivated () {
		base.OnActivated();
		Mode = RequireMode = MenuMode.Root;
		ScreenTint = new(0, 0, 0, 128);
		BackgroundTint = new(0, 0, 0, 255);
		MaxItemCount = 11;
		ItemHeight = 28;
		ContentPadding = Int4.Direction(32, 32, 32, 32);
		BgmPlayingBefore = Game.IsMusicPlaying;
		Game.PauseMusic();
	}


	public override void OnInactivated () {
		base.OnInactivated();
		Game.UnpauseGame();
		if (BgmPlayingBefore) {
			Game.UnpauseMusic();
		}
	}


	public override void LateUpdate () {

		if (Mode != RequireMode) {
			Mode = RequireMode;
			RefreshAnimation();
		}
		Interactable = (Mode != MenuMode.Setter_Gamepad && Mode != MenuMode.Setter_Keyboard) || RecordingKey < 0;

		ControlHintUI.ForceShowHint();

		base.LateUpdate();

		if (Game.IsPlaying) {
			Active = false;
		}

	}


	protected override void DrawMenu () {
		Message = string.Empty;
		if (Mode == MenuMode.Quit && !Universe.BuiltInInfo.AllowQuitFromMenu) {
			Mode = MenuMode.Root;
		}
		switch (Mode) {
			case MenuMode.Root:
				MenuRoot();
				break;
			case MenuMode.KeySetter:
				MenuKeySetterHub();
				break;
			case MenuMode.Setting:
				MenuSetting();
				break;
			case MenuMode.Restart:
				MenuRestart();
				break;
			case MenuMode.Quit:
				MenuQuit();
				break;

			case MenuMode.Setter_Keyboard:
				MenuKeySetter(false);
				break;
			case MenuMode.Setter_Gamepad:
				MenuKeySetter(true);
				break;
		}
	}


	// Menus
	private void MenuRoot () {

		// 0-Continue
		if (DrawItem(BuiltInText.UI_CONTINUE) || Input.GameKeyDown(Gamekey.Jump)) {
			Game.UnpauseGame();
			Active = false;
			Input.UseAllHoldingKeys();
		}

		// 1-Key Setter
		if (DrawItem(MENU_KEY_SETTER)) {
			RequireMode = MenuMode.KeySetter;
			SetSelection(0);
		}

		// 2-Setting
		if (DrawItem(BuiltInText.UI_SETTING)) {
			RequireMode = MenuMode.Setting;
			SetSelection(0);
		}

		// 3-Restart Game
		if (Universe.BuiltInInfo.AllowRestartFromMenu) {
			if (DrawItem(BuiltInText.UI_RESTART)) {
				RequireMode = MenuMode.Restart;
				SetSelection(0);
			}
		}

		// 3/4-Quit
		if (Universe.BuiltInInfo.AllowQuitFromMenu) {
			using (new GUIContentColorScope(Color32.RED_BETTER)) {
				if (DrawItem(BuiltInText.UI_QUIT)) {
					bool quitImmediately = false;
#if DEBUG
					quitImmediately = true;
#endif
					if (quitImmediately) {
						Game.QuitApplication();
					} else {
						RequireMode = MenuMode.Quit;
						SetSelection(0);
					}
				}
			}
		}

	}


	private void MenuKeySetterHub () {

		if (DrawItem(MENU_SETTER_KEYBOARD)) {
			RequireMode = MenuMode.Setter_Keyboard;
			SetSelection(0);
			RecordingKey = -1;
			RecordLock = true;
			RecordDirty = false;
			KeySetterConfirming = false;
			for (int i = 0; i < KeyboardKeys.Length; i++) {
				KeyboardKeys[i] = Input.GetKeyboardMap((Gamekey)i);
			}
		}

		if (DrawItem(MENU_SETTER_GAMEPAD)) {
			RequireMode = MenuMode.Setter_Gamepad;
			SetSelection(0);
			RecordingKey = -1;
			RecordLock = true;
			RecordDirty = false;
			KeySetterConfirming = false;
			for (int i = 0; i < GamepadKeys.Length; i++) {
				GamepadKeys[i] = Input.GetGamepadMap((Gamekey)i);
			}
		}

		if (DrawItem(BuiltInText.UI_BACK) || Input.GameKeyDown(Gamekey.Jump)) {
			RequireMode = MenuMode.Root;
			SetSelection(1);
		}
	}


	private void MenuSetting () {

		// Music Volume
		if (DrawArrowItem(
			MENU_MUSIC_VOLUME,
			MusicVolumeCache.GetChars(Game.MusicVolume / 100),
			Game.MusicVolume > 0, Game.MusicVolume < 1000, out int delta
		)) {
			Game.SetMusicVolume(Game.MusicVolume + delta * 100);
		}

		// Sound Volume
		if (DrawArrowItem(
			MENU_SOUND_VOLUME,
			SoundVolumeCache.GetChars(Game.SoundVolume / 100),
			Game.SoundVolume > 0, Game.SoundVolume < 1000, out delta
		)) {
			Game.SetSoundVolume(Game.SoundVolume + delta * 100);
		}

		// Fullscreen
		if (DrawItem(
			MENU_FULLSCREEN_LABEL,
			Game.IsFullscreen ? BuiltInText.UI_ON : BuiltInText.UI_OFF
		)) {
			Game.IsFullscreen = !Game.IsFullscreen;
		}

		// Language
		if (Language.LanguageCount > 0) {
			int currentLanguageIndex = 0;
			for (int i = 0; i < Language.LanguageCount; i++) {
				var lan = Language.GetLanguageAt(i);
				if (lan == Language.CurrentLanguage) {
					currentLanguageIndex = i;
					break;
				}
			}
			if (DrawArrowItem(
				MENU_LANGUAGE,
				Util.GetLanguageDisplayName(Language.CurrentLanguage),
				currentLanguageIndex > 0, currentLanguageIndex < Language.LanguageCount - 1, out delta)
			) {
				int newIndex = currentLanguageIndex + delta;
				newIndex = newIndex.Clamp(0, Language.LanguageCount - 1);
				if (newIndex != currentLanguageIndex) {
					Language.SetLanguage(Language.GetLanguageAt(newIndex));
				}
			}
		}

		// Allow Gamepad
		if (DrawItem(
			MENU_ALLOW_GAMEPAD,
			Input.AllowGamepad ? BuiltInText.UI_YES : BuiltInText.UI_NO
		)) {
			Input.AllowGamepad = !Input.AllowGamepad;
		}

		// Control Hint
		if (DrawItem(
			MENU_CONTROL_HINT,
			ControlHintUI.UseControlHint ? BuiltInText.UI_ON : BuiltInText.UI_OFF
		)) {
			ControlHintUI.UseControlHint = !ControlHintUI.UseControlHint;
		}

		// Gamepad Hint
		if (DrawItem(
			MENU_GAMEPAD_HINT,
			ControlHintUI.UseGamePadHint ? BuiltInText.UI_ON : BuiltInText.UI_OFF
		)) {
			ControlHintUI.UseGamePadHint = !ControlHintUI.UseGamePadHint;
		}

		// Back
		if (DrawItem(BuiltInText.UI_BACK) || Input.GameKeyDown(Gamekey.Jump)) {
			RequireMode = MenuMode.Root;
			SetSelection(2);
		}
	}


	private void MenuRestart () {

		Message = MENU_RESTART_MESSAGE;

		// Continue
		if (DrawItem(BuiltInText.UI_BACK) || Input.GameKeyDown(Gamekey.Jump)) {
			RequireMode = MenuMode.Root;
			SetSelection(3);
		}

		// Restart
		if (DrawItem(BuiltInText.UI_RESTART)) {
			Game.UnpauseGame();
			Active = false;
			Input.UseAllHoldingKeys();
			TaskSystem.AddToLast(RestartGameTask.TYPE_ID);
		}

#if DEBUG
		// Restart & Regenerate Map
		if (MapGenerationSystem.Enable) {
			if (DrawItem(BuiltInText.UI_RESTART_REGENERATE)) {
				Game.UnpauseGame();
				Active = false;
				Input.UseAllHoldingKeys();
				MapGenerationSystem.ResetAll(restartGame: false);
				TaskSystem.AddToLast(RestartGameTask.TYPE_ID);
			}
		}
#endif

	}


	private void MenuQuit () {

		Message = MENU_QUIT_MESSAGE;

		// Continue
		if (DrawItem(BuiltInText.UI_CONTINUE) || Input.GameKeyDown(Gamekey.Jump)) {
			RequireMode = MenuMode.Root;
			SetSelection(1024);
		}

		// Quit Game
		using (new GUIContentColorScope(Color32.RED_BETTER)) {
			if (DrawItem(BuiltInText.UI_QUIT_GAME)) {
				Game.QuitApplication();
			}
		}

	}


	private void MenuKeySetter (bool forGamepad) {

		// Confirming
		if (KeySetterConfirming) {
			Message = MENU_KEYSETTER_CONFIRM_MESSAGE;
			if (DrawItem(BuiltInText.UI_SAVE)) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(forGamepad ? 1 : 0);
				SaveKeySetting(forGamepad);
			}
			if (DrawItem(BuiltInText.UI_DONT_SAVE)) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(forGamepad ? 1 : 0);
			}
			if (DrawItem(BuiltInText.UI_CANCEL)) {
				KeySetterConfirming = false;
			}
			return;
		}

		// Key Setter
		Message = forGamepad ? MENU_KEYSETTER_GAMEPAD_MESSAGE : MENU_KEYSETTER_KEYBOARD_MESSAGE;

		// All Game Keys
		for (int i = 0; i < GAMEKEY_UI_CODES.Length; i++) {
			int code = GAMEKEY_UI_CODES[i].ID;
			int iconID = 0;
			Color32 tint = Color32.WHITE;
			string text;
			if (RecordingKey != i) {
				// Normal
				text = forGamepad ? string.Empty : Util.GetKeyDisplayName(KeyboardKeys[i]);
				iconID = forGamepad && FrameworkUtil.GAMEPAD_CODE.TryGetValue(GamepadKeys[i], out var _value0) ? _value0 : 0;
			} else {
				// Recording
				tint = Game.PauselessFrame % 30 > 15 ? Color32.GREEN : Color32.WHITE;
				text = MENU_SETTER_RECORD;
			}
			using (new GUIContentColorScope(tint)) {
				if (DrawItem(Language.Get(code), text, iconID)) {
					RecordLock = true;
					RecordingKey = i;
				}
			}
		}

		// Save & Back
		using (new GUIContentColorScope(Color32.GREEN)) {
			if (RecordDirty && DrawItem(MENU_KEYSETTER_SAVE_BACK)) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(forGamepad ? 1 : 0);
				SaveKeySetting(forGamepad);
			}
		}

		// Back
		if (
			DrawItem(BuiltInText.UI_BACK) ||
			(RecordingKey < 0 && Input.GameKeyUp(Gamekey.Jump))
		) {
			if (RecordDirty) {
				// Confirm
				KeySetterConfirming = true;
			} else {
				// Just Back
				RequireMode = MenuMode.KeySetter;
				SetSelection(forGamepad ? 1 : 0);
			}
		}

		// Record
		if (RecordingKey >= 0 && !RecordLock) {
			if (forGamepad) {
				// Gamepad
				if (Input.TryGetHoldingGamepadButton(out var button)) {
					if (
						(button != GamepadKey.Start || RecordingKey == (int)Gamekey.Start) &&
						(button != GamepadKey.Select || RecordingKey == (int)Gamekey.Select)
					) {
						if (GamepadKeys[RecordingKey] != button) {
							for (int i = 0; i < GamepadKeys.Length; i++) {
								if (GamepadKeys[i] == button && GamepadKeys[RecordingKey] != button) {
									GamepadKeys[i] = GamepadKeys[RecordingKey];
								}
							}
							GamepadKeys[RecordingKey] = button;
							RecordDirty = true;
						}
					}
					RecordingKey = -1;
					Input.UseAllHoldingKeys();
				} else if (Input.AnyKeyboardKeyHolding || Input.MouseLeftButtonDown) {
					RecordingKey = -1;
					Input.UseAllHoldingKeys();
				}
			} else {
				// Keyboard
				if (Input.TryGetHoldingKeyboardKey(out var button)) {
					if (button != KeyboardKey.Escape || RecordingKey == (int)Gamekey.Start) {
						if (KeyboardKeys[RecordingKey] != button) {
							for (int i = 0; i < KeyboardKeys.Length; i++) {
								if (KeyboardKeys[i] == button && KeyboardKeys[RecordingKey] != button) {
									KeyboardKeys[i] = KeyboardKeys[RecordingKey];
								}
							}
							KeyboardKeys[RecordingKey] = button;
							RecordDirty = true;
						}
					}
					RecordingKey = -1;
					Input.UseAllHoldingKeys();
				} else if (Input.AnyGamepadButtonHolding || Input.MouseLeftButtonDown) {
					RecordingKey = -1;
					Input.UseAllHoldingKeys();
				}
			}
		}

		// Unlock Record
		if (
			RecordLock &&
			!Input.AnyGamepadButtonHolding &&
			!Input.AnyKeyboardKeyHolding &&
			!Input.MouseLeftButtonHolding
		) {
			RecordLock = false;
		}

		// Reset
		if (Input.KeyboardDown(KeyboardKey.F1)) {
			bool changed = false;
			if (forGamepad) {
				for (int i = 0; i < GamepadKeys.Length; i++) {
					var defaultKey = Input.GetDefaultGamepadMap((Gamekey)i);
					changed = changed || GamepadKeys[i] != defaultKey;
					GamepadKeys[i] = defaultKey;
				}
			} else {
				for (int i = 0; i < KeyboardKeys.Length; i++) {
					var defaultKey = Input.GetDefaultKeyboardMap((Gamekey)i);
					changed = changed || KeyboardKeys[i] != defaultKey;
					KeyboardKeys[i] = defaultKey;
				}
			}
			RecordDirty = changed;
		}

		// Use ESC
		if (Input.KeyboardUp(KeyboardKey.Escape) || Input.GameKeyUp(Gamekey.Start)) {
			Input.UseGameKey(Gamekey.Start);
			Input.UseKeyboardKey(KeyboardKey.Escape);
		}

		// Func
		static void SaveKeySetting (bool forGamepad) {
			if (Instance == null) return;
			if (forGamepad) {
				for (int i = 0; i < Instance.GamepadKeys.Length; i++) {
					Input.SetGamepadMap((Gamekey)i, Instance.GamepadKeys[i]);
				}
			} else {
				for (int i = 0; i < Instance.KeyboardKeys.Length; i++) {
					Input.SetKeyboardMap((Gamekey)i, Instance.KeyboardKeys[i]);
				}
			}
		}
	}


	#endregion




}
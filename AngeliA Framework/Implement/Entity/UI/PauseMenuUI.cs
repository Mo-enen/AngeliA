using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Capacity(1, 1)]
	[RequireLanguageFromField]
	[EntityAttribute.StageOrder(4096)]
	public class PauseMenuUI : MenuUI {




		#region --- SUB ---


		private enum MenuMode { Pause, Setting, EditorSetting, KeySetter, Restart, Quit, Setter_Keyboard, Setter_Gamepad }


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
		private static readonly LanguageCode MENU_FRAMERATE = ("Menu.Setting.Framerate", "Framerate");
		private static readonly LanguageCode MENU_LANGUAGE = ("Menu.Setting.Language", "Language");
		private static readonly LanguageCode MENU_SHOW_FPS = ("Menu.Setting.ShowFPS", "Show FPS");
		private static readonly LanguageCode MENU_KEYSETTER_SAVE_BACK = ("Menu.KeySetter.SaveAndBack", "Save and Back");
		private static readonly LanguageCode MENU_FULLSCREEN_LABEL = ("Menu.Setting.Fullscreen.Label", "Fullscreen");
		private static readonly LanguageCode MENU_CONTROL_HINT = ("Menu.Setting.UseControlHint", "Show Control Hint");
		private static readonly LanguageCode MENU_GAMEPAD_HINT = ("Menu.Setting.UseGamepadHint", "Show Gamepad Hint");
		private static readonly LanguageCode MENU_ALLOW_GAMEPAD = ("Menu.Setting.AllowGamepad", "Allow Gamepad");
		private static readonly LanguageCode MENU_MEDT_SETTING = ("Menu.Pause.MEDTSetting", "Editor Setting");
		private static readonly LanguageCode MENU_MEDT_AUTO_ZOOM = ("Menu.MEDTSetting.AutoZoom", "Auto Zoom");
		private static readonly LanguageCode MENU_MEDT_PLAYER_DROP = ("Menu.MEDTSetting.PlayerDrop", "Quick Player Drop");
		private static readonly LanguageCode MENU_MEDT_STATE = ("Menu.MEDTSetting.ShowState", "Show State Info");
		private static readonly LanguageCode[] GAMEKEY_UI_CODES = new LanguageCode[8] {
			($"UI.GameKey.{Gamekey.Left}", "Left"),
			($"UI.GameKey.{Gamekey.Right}", "Right"),
			($"UI.GameKey.{Gamekey.Down}", "Down"),
			($"UI.GameKey.{Gamekey.Up}", "Up"),
			($"UI.GameKey.{Gamekey.Action}", "Action"),
			($"UI.GameKey.{Gamekey.Jump}", "Jump"),
			($"UI.GameKey.{Gamekey.Start}", "Start"),
			($"UI.GameKey.{Gamekey.Select}", "Select"),
		};

		// Data
		private static PauseMenuUI Instance = null;
		private readonly KeyboardKey[] KeyboardKeys = new KeyboardKey[8];
		private readonly GamepadKey[] GamepadKeys = new GamepadKey[8];
		private readonly IntToChars MusicVolumeCache = new();
		private readonly IntToChars SoundVolumeCache = new();
		private readonly IntToChars FramerateCache = new();
		private readonly CellContent KeySetterLabel = new();
		private MenuMode Mode = MenuMode.Pause;
		private MenuMode RequireMode = MenuMode.Pause;
		private int RecordingKey = -1;
		private bool RecordLock = true;
		private bool RecordDirty = false;
		private bool KeySetterConfirming = false;


		#endregion




		#region --- MSG ---


		public PauseMenuUI () => Instance = this;


		[OnGameTryingToQuit]
		public static void OnGameTryingToQuit () {
			Stage.TrySpawnEntity(Instance.TypeID, 0, 0, out _);
			Instance.Mode = Instance.RequireMode = MenuMode.Quit;
		}


		[OnGameUpdatePauseless]
		public static void OnGameUpdatePauseless () {
			if (Instance == null) return;
			if (Game.IsPausing) {
				if (!Instance.Active) {
					Stage.TrySpawnEntity(Instance.TypeID, 0, 0, out _);
					Instance.Mode = Instance.RequireMode = MenuMode.Pause;
				}
			} else {
				if (Instance.Active) Instance.Active = false;
			}
		}


		public override void OnActivated () {
			base.OnActivated();
			ScreenTint = new(0, 0, 0, 128);
			BackgroundTint = new(0, 0, 0, 255);
			MaxItemCount = 11;
		}


		public override void OnInactivated () {
			base.OnInactivated();
			Game.UnpauseGame();
		}


		public override void FrameUpdate () {

			if (Mode != RequireMode) {
				Mode = RequireMode;
				RefreshAnimation();
			}
			Interactable = (Mode != MenuMode.Setter_Gamepad && Mode != MenuMode.Setter_Keyboard) || RecordingKey < 0;
			ContentPadding = new(32, 32, 46, string.IsNullOrEmpty(Message) ? 46 : 23);

			ControlHintUI.ForceShowHint();

			base.FrameUpdate();

		}


		protected override void DrawMenu () {
			Message = string.Empty;
			switch (Mode) {
				case MenuMode.Pause:
					MenuPause();
					break;
				case MenuMode.KeySetter:
					MenuKeySetterHub();
					break;
				case MenuMode.Setting:
					MenuSetting();
					break;
				case MenuMode.EditorSetting:
					MenuMapEditorSetting();
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
		private void MenuPause () {

			// 0-Continue
			if (DrawItem(BuiltInText.UI_CONTINUE) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				Game.UnpauseGame();
				Active = false;
				FrameInput.UseAllHoldingKeys();
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

			if (Game.AllowMakerFeaures) {
				// Maker Game
				if (MapEditor.IsEditing) {
					// 3-Map Editor Setting
					if (DrawItem(MENU_MEDT_SETTING)) {
						RequireMode = MenuMode.EditorSetting;
						SetSelection(0);
					}
				}
			} else {
				// Player Game
				// 3-Restart Game
				if (DrawItem(BuiltInText.UI_RESTART)) {
					RequireMode = MenuMode.Restart;
					SetSelection(0);
				}
			}

			// 3/4-Quit
			if (DrawItem(BuiltInText.UI_QUIT, Const.RED_BETTER)) {
				RequireMode = MenuMode.Quit;
				SetSelection(0);
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
					KeyboardKeys[i] = FrameInput.GetKeyboardMap((Gamekey)i);
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
					GamepadKeys[i] = FrameInput.GetGamepadMap((Gamekey)i);
				}
			}

			if (DrawItem(BuiltInText.UI_BACK) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(1);
			}
		}


		private void MenuSetting () {

			// Music Volume
			if (DrawArrowItem(
				MENU_MUSIC_VOLUME,
				CellContent.Get(MusicVolumeCache.GetChars(Game.MusicVolume / 100)),
				Game.MusicVolume > 0, Game.MusicVolume < 1000, out int delta
			)) {
				Game.SetMusicVolume(Game.MusicVolume + delta * 100);
			}

			// Sound Volume
			if (DrawArrowItem(
				MENU_SOUND_VOLUME,
				CellContent.Get(SoundVolumeCache.GetChars(Game.SoundVolume / 100)),
				Game.SoundVolume > 0, Game.SoundVolume < 1000, out delta
			)) {
				Game.SetSoundVolume(Game.SoundVolume + delta * 100);
			}

			// Framerate
			int currentFramerate = Game.GraphicFramerate;
			if (DrawArrowItem(
				MENU_FRAMERATE,
				CellContent.Get(FramerateCache.GetChars(currentFramerate)),
				currentFramerate > 30, currentFramerate < 120, out delta
			)) {
				Game.GraphicFramerate += delta * 30;
			}

			// Show FPS 
			if (DrawItem(
				MENU_SHOW_FPS,
				CellContent.Get(Game.ShowFPS ? BuiltInText.UI_ON : BuiltInText.UI_OFF)
			)) {
				Game.ShowFPS = !Game.ShowFPS;
			}

			// Fullscreen
			if (DrawItem(
				MENU_FULLSCREEN_LABEL,
				CellContent.Get(Game.IsFullscreen ? BuiltInText.UI_ON : BuiltInText.UI_OFF)
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
					CellContent.Get(Util.GetLanguageDisplayName(Language.CurrentLanguage)),
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
				CellContent.Get(FrameInput.AllowGamepad ? BuiltInText.UI_YES : BuiltInText.UI_NO)
			)) {
				FrameInput.AllowGamepad = !FrameInput.AllowGamepad;
			}

			// Control Hint
			if (DrawItem(
				MENU_CONTROL_HINT,
				CellContent.Get(ControlHintUI.UseControlHint ? BuiltInText.UI_ON : BuiltInText.UI_OFF)
			)) {
				ControlHintUI.UseControlHint = !ControlHintUI.UseControlHint;
			}

			// Gamepad Hint
			if (DrawItem(
				MENU_GAMEPAD_HINT,
				CellContent.Get(ControlHintUI.UseGamePadHint ? BuiltInText.UI_ON : BuiltInText.UI_OFF)
			)) {
				ControlHintUI.UseGamePadHint = !ControlHintUI.UseGamePadHint;
			}

			// Back
			if (DrawItem(BuiltInText.UI_BACK) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(2);
			}
		}


		private void MenuMapEditorSetting () {

			var mapEditor = MapEditor.Instance;
			if (mapEditor != null) {

				// Auto Zoom
				if (DrawItem(
					MENU_MEDT_AUTO_ZOOM,
					CellContent.Get(mapEditor.AutoZoom ? BuiltInText.UI_ON : BuiltInText.UI_OFF)
				)) {
					mapEditor.AutoZoom = !mapEditor.AutoZoom;
				}

				// Drop Player
				if (DrawItem(
					MENU_MEDT_PLAYER_DROP,
					CellContent.Get(mapEditor.QuickPlayerDrop ? BuiltInText.UI_ON : BuiltInText.UI_OFF)
				)) {
					mapEditor.QuickPlayerDrop = !mapEditor.QuickPlayerDrop;
				}

				// Show State
				if (DrawItem(
					MENU_MEDT_STATE,
					CellContent.Get(mapEditor.ShowState ? BuiltInText.UI_ON : BuiltInText.UI_OFF)
				)) {
					mapEditor.ShowState = !mapEditor.ShowState;
				}
			}

			// Back
			if (DrawItem(BuiltInText.UI_BACK) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(3);
			}
		}


		private void MenuRestart () {

			Message = MENU_RESTART_MESSAGE;

			// Continue
			if (DrawItem(BuiltInText.UI_BACK) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(3);
			}

			// Restart
			if (DrawItem(BuiltInText.UI_RESTART)) {
				Game.UnpauseGame();
				Active = false;
				FrameInput.UseAllHoldingKeys();
				FrameTask.AddToLast(RestartGameTask.TYPE_ID);
			}

		}


		private void MenuQuit () {

			Message = MENU_QUIT_MESSAGE;

			// Continue
			if (DrawItem(BuiltInText.UI_CONTINUE) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(1024);
			}

			// Quit Game
			if (DrawItem(BuiltInText.UI_QUIT_GAME, Const.RED_BETTER)) {
				Game.QuitApplication();
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
				CellContent valueLabel;
				int iconID = 0;
				if (RecordingKey != i) {
					// Normal
					KeySetterLabel.Tint = Const.WHITE;
					KeySetterLabel.BackgroundTint = Const.CLEAR;
					KeySetterLabel.Text = forGamepad ? string.Empty : Util.GetKeyDisplayName(KeyboardKeys[i]);
					valueLabel = KeySetterLabel;
					iconID = forGamepad && Const.GAMEPAD_CODE.TryGetValue(GamepadKeys[i], out var _value0) ? _value0 : 0;
				} else {
					// Recording
					KeySetterLabel.Tint = Game.PauselessFrame % 30 > 15 ? Const.BLACK : Const.WHITE;
					KeySetterLabel.Text = MENU_SETTER_RECORD;
					KeySetterLabel.BackgroundTint = Game.PauselessFrame % 30 > 15 ? Const.GREEN : Const.CLEAR;
					valueLabel = KeySetterLabel;
				}
				valueLabel.Tint = Const.WHITE;
				if (DrawItem(
					Language.Get(code), valueLabel, Const.WHITE, iconID
				)) {
					RecordLock = true;
					RecordingKey = i;
				}
			}

			// Save & Back
			if (RecordDirty && DrawItem(MENU_KEYSETTER_SAVE_BACK, Const.GREEN)) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(forGamepad ? 1 : 0);
				SaveKeySetting(forGamepad);
			}

			// Back
			if (
				DrawItem(BuiltInText.UI_BACK) ||
				(RecordingKey < 0 && FrameInput.GameKeyUp(Gamekey.Jump))
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
					if (FrameInput.TryGetHoldingGamepadButton(out var button)) {
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
						FrameInput.UseAllHoldingKeys();
					} else if (FrameInput.AnyKeyboardKeyHolding || FrameInput.MouseLeftButtonDown) {
						RecordingKey = -1;
						FrameInput.UseAllHoldingKeys();
					}
				} else {
					// Keyboard
					if (FrameInput.TryGetHoldingKeyboardKey(out var button)) {
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
						FrameInput.UseAllHoldingKeys();
					} else if (FrameInput.AnyGamepadButtonHolding || FrameInput.MouseLeftButtonDown) {
						RecordingKey = -1;
						FrameInput.UseAllHoldingKeys();
					}
				}
			}

			// Unlock Record
			if (
				RecordLock &&
				!FrameInput.AnyGamepadButtonHolding &&
				!FrameInput.AnyKeyboardKeyHolding &&
				!FrameInput.MouseLeftButton
			) {
				RecordLock = false;
			}

			// Reset
			if (FrameInput.KeyboardDown(KeyboardKey.F1)) {
				bool changed = false;
				if (forGamepad) {
					for (int i = 0; i < GamepadKeys.Length; i++) {
						var defaultKey = FrameInput.GetDefaultGamepadMap((Gamekey)i);
						changed = changed || GamepadKeys[i] != defaultKey;
						GamepadKeys[i] = defaultKey;
					}
				} else {
					for (int i = 0; i < KeyboardKeys.Length; i++) {
						var defaultKey = FrameInput.GetDefaultKeyboardMap((Gamekey)i);
						changed = changed || KeyboardKeys[i] != defaultKey;
						KeyboardKeys[i] = defaultKey;
					}
				}
				RecordDirty = changed;
			}

			// Use ESC
			if (FrameInput.KeyboardUp(KeyboardKey.Escape) || FrameInput.GameKeyUp(Gamekey.Start)) {
				FrameInput.UseGameKey(Gamekey.Start);
				FrameInput.UseKeyboardKey(KeyboardKey.Escape);
			}

			// Func
			static void SaveKeySetting (bool forGamepad) {
				if (Instance == null) return;
				if (forGamepad) {
					for (int i = 0; i < Instance.GamepadKeys.Length; i++) {
						FrameInput.SetGamepadMap((Gamekey)i, Instance.GamepadKeys[i]);
					}
				} else {
					for (int i = 0; i < Instance.KeyboardKeys.Length; i++) {
						FrameInput.SetKeyboardMap((Gamekey)i, Instance.KeyboardKeys[i]);
					}
				}
			}
		}


		#endregion




	}
}
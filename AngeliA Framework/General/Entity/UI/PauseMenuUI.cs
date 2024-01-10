using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	[EntityAttribute.Capacity(1, 1)]
	[RequireLanguageFromField]
	public class PauseMenuUI : MenuUI {




		#region --- SUB ---


		private enum MenuMode { Pause, Setting, EditorSetting, KeySetter, Restart, Quit, Setter_Keyboard, Setter_Gamepad }


		#endregion




		#region --- VAR ---


		// Const
		private static readonly LanguageCode MENU_QUIT_MESSAGE = "Menu.Pause.QuitMessage";
		private static readonly LanguageCode MENU_RESTART_MESSAGE = "Menu.Pause.RestartMessage";
		private static readonly LanguageCode MENU_KEYSETTER_GAMEPAD_MESSAGE = "Menu.KeySetter.GamepadMessage";
		private static readonly LanguageCode MENU_KEYSETTER_KEYBOARD_MESSAGE = "Menu.KeySetter.KeyboardMessage";
		private static readonly LanguageCode MENU_KEYSETTER_CONFIRM_MESSAGE = "Menu.KeySetter.ConfirmMessage";
		private static readonly LanguageCode MENU_KEY_SETTER = "Menu.Pause.KeySetter";
		private static readonly LanguageCode MENU_SETTER_KEYBOARD = "Menu.KeySetter.Keyboard";
		private static readonly LanguageCode MENU_SETTER_GAMEPAD = "Menu.KeySetter.Gamepad";
		private static readonly LanguageCode MENU_SETTER_RECORD = "Menu.KeySetter.Record";
		private static readonly LanguageCode MENU_MUSIC_VOLUME = "Menu.Setting.MusicVolume";
		private static readonly LanguageCode MENU_SOUND_VOLUME = "Menu.Setting.SoundVolume";
		private static readonly LanguageCode MENU_FRAMERATE = "Menu.Setting.Framerate";
		private static readonly LanguageCode MENU_LANGUAGE = "Menu.Setting.Language";
		private static readonly LanguageCode MENU_SAVE_SLOT = "Menu.Setting.SaveSlot";
		private static readonly LanguageCode MENU_SHOW_FPS = "Menu.Setting.ShowFPS";
		private static readonly LanguageCode MENU_KEYSETTER_SAVE_BACK = "Menu.KeySetter.SaveAndBack";
		private static readonly LanguageCode MENU_FULLSCREEN_0 = "Menu.Setting.Fullscreen.0";
		private static readonly LanguageCode MENU_FULLSCREEN_1 = "Menu.Setting.Fullscreen.1";
		private static readonly LanguageCode MENU_FULLSCREEN_2 = "Menu.Setting.Fullscreen.2";
		private static readonly LanguageCode MENU_FULLSCREEN_LABEL = "Menu.Setting.Fullscreen.Label";
		private static readonly LanguageCode MENU_VSYNC = "Menu.Setting.VSync";
		private static readonly LanguageCode MENU_CONTROL_HINT = "Menu.Setting.UseControlHint";
		private static readonly LanguageCode MENU_GAMEPAD_HINT = "Menu.Setting.UseGamepadHint";
		private static readonly LanguageCode MENU_ALLOW_GAMEPAD = "Menu.Setting.AllowGamepad";
		private static readonly LanguageCode MENU_MEDT_SETTING = "Menu.Pause.MEDTSetting";
		private static readonly LanguageCode MENU_MEDT_AUTO_ZOOM = "Menu.MEDTSetting.AutoZoom";
		private static readonly LanguageCode MENU_MEDT_PLAYER_DROP = "Menu.MEDTSetting.PlayerDrop";
		private static readonly LanguageCode MENU_MEDT_STATE = "Menu.MEDTSetting.ShowState";
		private static readonly LanguageCode UI_CONTINUE = "UI.Continue";
		private static readonly LanguageCode UI_SETTING = "UI.Setting";
		private static readonly LanguageCode UI_QUIT = "UI.Quit";
		private static readonly LanguageCode UI_RESTART = "UI.Restart";
		private static readonly LanguageCode UI_QUIT_GAME = "UI.QuitGame";
		private static readonly LanguageCode UI_STOP_EDIT = "UI.StopEdit";
		private static readonly LanguageCode UI_BACK = "UI.Back";
		private static readonly LanguageCode UI_ON = "UI.ON";
		private static readonly LanguageCode UI_OFF = "UI.OFF";
		private static readonly LanguageCode UI_YES = "UI.Yes";
		private static readonly LanguageCode UI_SAVE = "UI.Save";
		private static readonly LanguageCode UI_DONT_SAVE = "UI.DontSave";
		private static readonly LanguageCode UI_NO = "UI.No";
		private static readonly LanguageCode UI_CANCEL = "UI.Cancel";
		private readonly string[] SLOT_NAMES = { };
		private static readonly LanguageCode[] GAMEKEY_UI_CODES = new LanguageCode[8] {
			$"UI.GameKey.{Gamekey.Left}",
			$"UI.GameKey.{Gamekey.Right}",
			$"UI.GameKey.{Gamekey.Down}",
			$"UI.GameKey.{Gamekey.Up}",
			$"UI.GameKey.{Gamekey.Action}",
			$"UI.GameKey.{Gamekey.Jump}",
			$"UI.GameKey.{Gamekey.Start}",
			$"UI.GameKey.{Gamekey.Select}",
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
		private int RequireNewSaveSlot = -1;
		private bool RecordLock = true;
		private bool RecordDirty = false;
		private bool KeySetterConfirming = false;


		#endregion




		#region --- MSG ---


		public PauseMenuUI () {
			Instance = this;
			SLOT_NAMES = new string[AngePath.SAVE_SLOT_COUNT];
			for (int i = 0; i < AngePath.SAVE_SLOT_COUNT; i++) {
				SLOT_NAMES[i] = ((char)(i + 'A')).ToString();
			}
		}


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
			RequireNewSaveSlot = -1;
			MaxItemCount = 11;
		}


		public override void OnInactivated () {
			base.OnInactivated();
			// Unpause
			if (Game.IsPausing) Game.IsPlaying = true;
			// Set Save Slot
			if (
				RequireNewSaveSlot >= 0 &&
				RequireNewSaveSlot < AngePath.SAVE_SLOT_COUNT &&
				RequireNewSaveSlot != AngePath.CurrentSaveSlot
			) {
				AngePath.CurrentSaveSlot = RequireNewSaveSlot;
				Game.RestartGame();
			}
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
			if (DrawItem(Language.Get(UI_CONTINUE, "Continue")) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				Game.IsPlaying = true;
				Active = false;
				FrameInput.UseAllHoldingKeys();
			}

			// 1-Key Setter
			if (DrawItem(Language.Get(MENU_KEY_SETTER, "Key Assignment"))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(0);
			}

			// 2-Setting
			if (DrawItem(Language.Get(UI_SETTING, "Setting"))) {
				RequireMode = MenuMode.Setting;
				SetSelection(0);
			}

			if (MapEditor.IsActived) {
				// 3-Map Editor Setting
				if (DrawItem(Language.Get(MENU_MEDT_SETTING, "Editor Setting"))) {
					RequireMode = MenuMode.EditorSetting;
					SetSelection(0);
				}
			} else if (!GlobalEditorUI.HaveActiveInstance) {
				// 3-Restart Game
				if (DrawItem(Language.Get(UI_RESTART, "Restart"))) {
					RequireMode = MenuMode.Restart;
					SetSelection(0);
				}
			}

			// 3/4-Quit
			if (DrawItem(Language.Get(UI_QUIT, "Quit"), Const.RED_BETTER)) {
				RequireMode = MenuMode.Quit;
				SetSelection(0);
			}

		}


		private void MenuKeySetterHub () {

			if (DrawItem(Language.Get(MENU_SETTER_KEYBOARD, "Keyboard"))) {
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

			if (DrawItem(Language.Get(MENU_SETTER_GAMEPAD, "Gamepad"))) {
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

			if (DrawItem(Language.Get(UI_BACK, "Back")) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(1);
			}
		}


		private void MenuSetting () {

			// Music Volume
			if (DrawArrowItem(
				Language.Get(MENU_MUSIC_VOLUME, "Music Volume"),
				CellContent.Get(MusicVolumeCache.GetChars(Game.MusicVolume / 100)),
				Game.MusicVolume > 0, Game.MusicVolume < 1000, out int delta
			)) {
				Game.SetMusicVolume(Game.MusicVolume + delta * 100);
			}

			// Sound Volume
			if (DrawArrowItem(
				Language.Get(MENU_SOUND_VOLUME, "Sound Volume"),
				CellContent.Get(SoundVolumeCache.GetChars(Game.SoundVolume / 100)),
				Game.SoundVolume > 0, Game.SoundVolume < 1000, out delta
			)) {
				Game.SetSoundVolume(Game.SoundVolume + delta * 100);
			}

			// Framerate
			int currentFramerate = Game.GraphicFramerate;
			if (DrawArrowItem(
				Language.Get(MENU_FRAMERATE, "Framerate"),
				CellContent.Get(FramerateCache.GetChars(currentFramerate)),
				currentFramerate > 30, currentFramerate < 120, out delta
			)) {
				Game.GraphicFramerate += delta * 30;
			}

			// Show FPS 
			if (DrawItem(
				Language.Get(MENU_SHOW_FPS),
				CellContent.Get(Game.ShowFPS ? Language.Get(UI_ON, "ON") : Language.Get(UI_OFF, "OFF"))
			)) {
				Game.ShowFPS = !Game.ShowFPS;
			}

			// VSync
			if (DrawItem(
				Language.Get(MENU_VSYNC),
				CellContent.Get(Game.VSync ? Language.Get(UI_ON, "ON") : Language.Get(UI_OFF, "OFF"))
			)) {
				Game.VSync = !Game.VSync;
			}

			// Fullscreen
			if (DrawArrowItem(
				Language.Get(MENU_FULLSCREEN_LABEL, "Fullscreen"),
				CellContent.Get(
					Game.FullscreenMode switch {
						FullscreenMode.Window => Language.Get(MENU_FULLSCREEN_0, "Windowed"),
						FullscreenMode.Fullscreen => Language.Get(MENU_FULLSCREEN_1, "Fullscreen"),
						FullscreenMode.FullscreenLow => Language.Get(MENU_FULLSCREEN_2, "Fullscreen (Low)"),
						_ => Language.Get(MENU_FULLSCREEN_0, "Windowed"),
					}
				),
				Game.FullscreenMode != FullscreenMode.Window,
				Game.FullscreenMode != FullscreenMode.FullscreenLow,
				out delta
			)) {
				Game.FullscreenMode = (FullscreenMode)((int)Game.FullscreenMode + delta).Clamp(0, 2);
			}

			// Language
			int currentLanguageIndex = 0;
			for (int i = 0; i < Language.LanguageCount; i++) {
				var lan = Language.GetLanguageAt(i);
				if (lan == Language.CurrentLanguage) {
					currentLanguageIndex = i;
					break;
				}
			}
			if (DrawArrowItem(
				Language.Get(MENU_LANGUAGE, "Language"),
				CellContent.Get(Language.CurrentLanguageDisplayName),
				currentLanguageIndex > 0, currentLanguageIndex < Language.LanguageCount - 1, out delta)
			) {
				int newIndex = currentLanguageIndex + delta;
				newIndex = newIndex.Clamp(0, Language.LanguageCount - 1);
				if (newIndex != currentLanguageIndex) {
					Language.SetLanguage(Language.GetLanguageAt(newIndex));
				}
			}

			// Save Slot
			int settedSaveSlot = RequireNewSaveSlot < 0 ? AngePath.CurrentSaveSlot : RequireNewSaveSlot;
			if (DrawArrowItem(
				Language.Get(MENU_SAVE_SLOT, "Save Slot"),
				CellContent.Get(SLOT_NAMES[settedSaveSlot]),
				settedSaveSlot > 0, settedSaveSlot < AngePath.SAVE_SLOT_COUNT - 1, out delta)
			) {
				int newIndex = settedSaveSlot + delta;
				newIndex = newIndex.Clamp(0, AngePath.SAVE_SLOT_COUNT - 1);
				if (newIndex != settedSaveSlot) {
					RequireNewSaveSlot = newIndex;
				}
			}

			// Allow Gamepad
			if (DrawItem(
			Language.Get(MENU_ALLOW_GAMEPAD, "Allow Gamepad"),
				CellContent.Get(FrameInput.AllowGamepad ? Language.Get(UI_YES, "YES") : Language.Get(UI_NO, "NO"))
			)) {
				FrameInput.AllowGamepad = !FrameInput.AllowGamepad;
			}

			// Control Hint
			if (DrawItem(
				Language.Get(MENU_CONTROL_HINT, "Show Control Hint"),
				CellContent.Get(ControlHintUI.UseControlHint ? Language.Get(UI_ON, "ON") : Language.Get(UI_OFF, "OFF"))
			)) {
				ControlHintUI.UseControlHint = !ControlHintUI.UseControlHint;
			}

			// Gamepad Hint
			if (DrawItem(
				Language.Get(MENU_GAMEPAD_HINT, "Show Gamepad Hint"),
				CellContent.Get(ControlHintUI.UseGamePadHint ? Language.Get(UI_ON, "ON") : Language.Get(UI_OFF, "OFF"))
			)) {
				ControlHintUI.UseGamePadHint = !ControlHintUI.UseGamePadHint;
			}

			// Back
			if (DrawItem(Language.Get(UI_BACK, "Back")) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(2);
			}
		}


		private void MenuMapEditorSetting () {

			var mapEditor = MapEditor.Instance;
			if (mapEditor != null) {

				// Auto Zoom
				if (DrawItem(
					Language.Get(MENU_MEDT_AUTO_ZOOM, "Auto Zoom"),
					CellContent.Get(mapEditor.AutoZoom ? Language.Get(UI_ON, "ON") : Language.Get(UI_OFF, "OFF"))
				)) {
					mapEditor.AutoZoom = !mapEditor.AutoZoom;
				}

				// Drop Player
				if (DrawItem(
					Language.Get(MENU_MEDT_PLAYER_DROP, "Quick Player Drop"),
					CellContent.Get(mapEditor.QuickPlayerDrop ? Language.Get(UI_ON, "ON") : Language.Get(UI_OFF, "OFF"))
				)) {
					mapEditor.QuickPlayerDrop = !mapEditor.QuickPlayerDrop;
				}

				// Show State
				if (DrawItem(
					Language.Get(MENU_MEDT_STATE, "Show State Info"),
					CellContent.Get(mapEditor.ShowState ? Language.Get(UI_ON, "ON") : Language.Get(UI_OFF, "OFF"))
				)) {
					mapEditor.ShowState = !mapEditor.ShowState;
				}
			}

			// Back
			if (DrawItem(Language.Get(UI_BACK, "Back")) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(3);
			}
		}


		private void MenuRestart () {

			Message = Language.Get(MENU_RESTART_MESSAGE, "Restart Game?");

			// Continue
			if (DrawItem(Language.Get(UI_BACK, "Back")) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(3);
			}

			// Restart
			if (DrawItem(Language.Get(UI_RESTART, "Restart"))) {
				Game.IsPlaying = true;
				Active = false;
				FrameInput.UseAllHoldingKeys();
				Game.RestartGame();
			}

		}


		private void MenuQuit () {

			bool editing = GlobalEditorUI.Instance != null && GlobalEditorUI.Instance.Active;

			Message = Language.Get(MENU_QUIT_MESSAGE, "Quit Game?");

			// Continue
			if (DrawItem(Language.Get(UI_CONTINUE, "Continue")) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(1024);
			}

			// Stop Edit
			if (editing && DrawItem(Language.Get(UI_STOP_EDIT, "Stop Editing"))) {
				Game.IsPlaying = true;
				Active = false;
				FrameInput.UseAllHoldingKeys();
				GlobalEditorUI.CloseEditorSmoothly();
			}

			// Quit Game
			if (DrawItem(Language.Get(UI_QUIT_GAME, "Quit Game"), Const.RED_BETTER)) {
				Game.QuitApplication();
			}

		}


		private void MenuKeySetter (bool forGamepad) {

			// Confirming
			if (KeySetterConfirming) {
				Message = Language.Get(MENU_KEYSETTER_CONFIRM_MESSAGE, "Save the changes?");
				if (DrawItem(Language.Get(UI_SAVE, "Save"))) {
					RequireMode = MenuMode.KeySetter;
					SetSelection(forGamepad ? 1 : 0);
					SaveKeySetting(forGamepad);
				}
				if (DrawItem(Language.Get(UI_DONT_SAVE, "Don't Save"))) {
					RequireMode = MenuMode.KeySetter;
					SetSelection(forGamepad ? 1 : 0);
				}
				if (DrawItem(Language.Get(UI_CANCEL, "Cancel"))) {
					KeySetterConfirming = false;
				}
				return;
			}

			// Key Setter
			Message = Language.Get(
				forGamepad ? MENU_KEYSETTER_GAMEPAD_MESSAGE : MENU_KEYSETTER_KEYBOARD_MESSAGE,
				"Press F1 key to reset"
			);

			// All Game Keys
			for (int i = 0; i < GAMEKEY_UI_CODES.Length; i++) {
				int code = GAMEKEY_UI_CODES[i];
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
					KeySetterLabel.Text = Language.Get(MENU_SETTER_RECORD, "Press key u want");
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
			if (RecordDirty && DrawItem(Language.Get(MENU_KEYSETTER_SAVE_BACK, "Save and Back"), Const.GREEN)) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(forGamepad ? 1 : 0);
				SaveKeySetting(forGamepad);
			}

			// Back
			if (
				DrawItem(Language.Get(UI_BACK, "Back")) ||
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
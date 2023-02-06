using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;


namespace Yaya {
	[EntityAttribute.DontDestroyOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public class ePauseMenu : MenuUI {




		#region --- SUB ---


		private enum MenuMode { Pause, Setting, KeySetter, Quit, Setter_Keyboard, Setter_Gamepad }


		#endregion




		#region --- VAR ---


		// Api
		public bool QuitMode => Mode == MenuMode.Quit;

		// Data
		private readonly Key[] KeyboardKeys = new Key[8];
		private readonly GamepadButton[] GamepadKeys = new GamepadButton[8];
		private MenuMode Mode = MenuMode.Pause;
		private MenuMode RequireMode = MenuMode.Pause;
		private int RecordingKey = -1;
		private int PauselessFrame = 0;
		private bool RecordLock = true;
		private readonly IntToString MusicVolumeCache = new();
		private readonly IntToString SoundVolumeCache = new();
		private readonly IntToString FramerateCache = new();


		#endregion




		#region --- MSG ---


		public override void OnActived () {
			base.OnActived();
			ScreenTint = new(0, 0, 0, 128);
			BackgroundTint = new(0, 0, 0, 255);
		}


		public override void FrameUpdate () {

			if (Mode != RequireMode) {
				Mode = RequireMode;
				RefreshAnimation();
			}
			Interactable = (Mode != MenuMode.Setter_Gamepad && Mode != MenuMode.Setter_Keyboard) || RecordingKey < 0;
			ContentPadding = new(32, 32, 46, string.IsNullOrEmpty(Message) ? 46 : 23);

			base.FrameUpdate();

			PauselessFrame++;
		}


		public override void OnInactived () {
			base.OnInactived();
			if (Game.Current.State == GameState.Pause) {
				Game.Current.State = GameState.Play;
			}
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
			if (DrawItem(Language.Get(WORD.UI_CONTINUE)) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				Game.Current.State = GameState.Play;
				Active = false;
				FrameInput.UseAllHoldingKeys();
			}

			// 1-Key Setter
			if (DrawItem(Language.Get(WORD.MENU_KEY_SETTER))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(0);
			}

			// 2-Setting
			if (DrawItem(Language.Get(WORD.UI_SETTING))) {
				RequireMode = MenuMode.Setting;
				SetSelection(0);
			}

			// 3-Quit
			if (DrawItem(Language.Get(WORD.UI_QUIT), Const.RED_BETTER)) {
				RequireMode = MenuMode.Quit;
				SetSelection(0);
			}
		}


		private void MenuKeySetterHub () {

			if (DrawItem(Language.Get(WORD.MENU_SETTER_KEYBOARD))) {
				RequireMode = MenuMode.Setter_Keyboard;
				SetSelection(0);
				RecordingKey = -1;
				RecordLock = true;
				for (int i = 0; i < KeyboardKeys.Length; i++) {
					KeyboardKeys[i] = FrameInput.GetKeyboardMap((Gamekey)i);
				}
			}

			if (DrawItem(Language.Get(WORD.MENU_SETTER_GAMEPAD))) {
				RequireMode = MenuMode.Setter_Gamepad;
				SetSelection(0);
				RecordingKey = -1;
				RecordLock = true;
				for (int i = 0; i < GamepadKeys.Length; i++) {
					GamepadKeys[i] = FrameInput.GetGamepadMap((Gamekey)i);
				}
			}

			if (DrawItem(Language.Get(WORD.UI_BACK)) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(1);
			}
		}


		private void MenuSetting () {

			// Music Volume
			if (DrawArrowItem(
				Language.Get(WORD.MENU_MUSIC_VOLUME),
				new CellLabel(MusicVolumeCache.GetString(AudioPlayer.MusicVolume / 100)),
				AudioPlayer.MusicVolume > 0, AudioPlayer.MusicVolume < 1000, out int delta
			)) {
				AudioPlayer.SetMusicVolume(AudioPlayer.MusicVolume + delta * 100);
			}

			// Sound Volume
			if (DrawArrowItem(
				Language.Get(WORD.MENU_SOUND_VOLUME),
				new CellLabel(SoundVolumeCache.GetString(AudioPlayer.SoundVolume / 100)),
				AudioPlayer.SoundVolume > 0, AudioPlayer.SoundVolume < 1000, out delta
			)) {
				AudioPlayer.SetSoundVolume(AudioPlayer.SoundVolume + delta * 100);
			}

			// Framerate
			int currentFramerate = Game.Current.GraphicFramerate;
			if (DrawArrowItem(
				Language.Get(WORD.MENU_FRAMERATE),
				new CellLabel(FramerateCache.GetString(currentFramerate)),
				currentFramerate > 30, currentFramerate < 120, out delta
			)) {
				Game.Current.GraphicFramerate += delta * 30;
			}

			// VSync
			if (DrawItem(
				Language.Get(WORD.MENU_VSYNC),
				new CellLabel(Language.Get(Game.Current.VSync ? WORD.UI_ON : WORD.UI_OFF))
			)) {
				Game.Current.VSync = !Game.Current.VSync;
			}

			// Screen Effect
			if (DrawItem(
				Language.Get(WORD.MENU_SCREEN_EFFECT),
				new CellLabel(Language.Get(Game.Current.UseScreenEffects ? WORD.UI_ON : WORD.UI_OFF))
			)) {
				Game.Current.UseScreenEffects = !Game.Current.UseScreenEffects;
			}

			// Fullscreen
			if (DrawArrowItem(
				Language.Get(WORD.MENU_FULLSCREEN_LABEL),
				new CellLabel(
					Language.Get(Game.Current.FullscreenMode switch {
						FullscreenMode.Window => WORD.MENU_FULLSCREEN_0,
						FullscreenMode.Fullscreen => WORD.MENU_FULLSCREEN_1,
						FullscreenMode.FullscreenLow => WORD.MENU_FULLSCREEN_2,
						_ => WORD.MENU_FULLSCREEN_0,
					})
				),
				Game.Current.FullscreenMode != FullscreenMode.Window,
				Game.Current.FullscreenMode != FullscreenMode.FullscreenLow,
				out delta
			)) {
				Game.Current.FullscreenMode = (FullscreenMode)((int)Game.Current.FullscreenMode + delta).Clamp(0, 2);
			}

			// Language
			int currentLanguageIndex = 0;
			for (int i = 0; i < Language.LanguageCount; i++) {
				var lan = Language.GetLanguageAt(i);
				if (lan == Game.Current.CurrentLanguage) {
					currentLanguageIndex = i;
					break;
				}
			}
			if (DrawArrowItem(
				Language.Get(WORD.MENU_LANGUAGE),
				new(Util.GetLanguageDisplayName(Game.Current.CurrentLanguage)),
				currentLanguageIndex > 0, currentLanguageIndex < Language.LanguageCount - 1, out delta)
			) {
				int newIndex = currentLanguageIndex + delta;
				newIndex = newIndex.Clamp(0, Language.LanguageCount - 1);
				if (newIndex != currentLanguageIndex) {
					Game.Current.SetLanguage(Language.GetLanguageAt(newIndex));
				}
			}

			// Control Hint
			if (DrawItem(
				Language.Get(WORD.MENU_CONTROL_HINT),
				new CellLabel(Language.Get(eControlHintUI.UseControlHint ? WORD.UI_ON : WORD.UI_OFF))
			)) {
				eControlHintUI.UseControlHint = !eControlHintUI.UseControlHint;
			}

			// Gamepad Hint
			if (DrawItem(
				Language.Get(WORD.MENU_GAMEPAD_HINT),
				new CellLabel(Language.Get(eControlHintUI.UseGamePadHint ? WORD.UI_ON : WORD.UI_OFF))
			)) {
				eControlHintUI.UseGamePadHint = !eControlHintUI.UseGamePadHint;
			}

			// Back
			if (DrawItem(Language.Get(WORD.UI_BACK)) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(2);
			}
		}


		private void MenuQuit () {

			Message = Language.Get(WORD.MENU_QUIT_MESSAGE);

			if (DrawItem(Language.Get(WORD.UI_BACK)) || FrameInput.GameKeyDown(Gamekey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(3);
			}

			if (DrawItem(Language.Get(WORD.UI_QUIT), Const.RED_BETTER)) {
				Application.Quit();
			}

		}


		private void MenuKeySetter (bool forGamepad) {

			Message = Language.Get(forGamepad ? WORD.MENU_KEYSETTER_GAMEPAD_MESSAGE : WORD.MENU_KEYSETTER_KEYBOARD_MESSAGE);

			// All Game Keys
			for (int i = 0; i < WORD.GAMEKEY_UI_CODES.Length; i++) {
				int code = WORD.GAMEKEY_UI_CODES[i];
				CellLabel valueLabel;
				if (RecordingKey != i) {
					// Normal
					valueLabel = new(forGamepad ? string.Empty : Util.GetKeyDisplayName(KeyboardKeys[i])) {
						Image = forGamepad && YayaConst.GAMEPAD_CODE.TryGetValue(GamepadKeys[i], out var _value0) ? _value0 : 0,
					};
				} else {
					// Recording
					valueLabel = new(Language.Get(WORD.MENU_SETTER_RECORD)) {
						Tint = PauselessFrame % 30 > 15 ? Const.BLACK : Const.WHITE,
						BackgroundTint = PauselessFrame % 30 > 15 ? Const.GREEN : Const.CLEAR,
					};
				}
				if (DrawItem(Language.Get(code), valueLabel)) {
					RecordLock = true;
					RecordingKey = i;
				}
			}

			// Save Back
			if (DrawItem(Language.Get(WORD.MENU_KEYSETTER_SAVE_BACK))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(forGamepad ? 1 : 0);
				if (forGamepad) {
					for (int i = 0; i < GamepadKeys.Length; i++) {
						FrameInput.SetGamepadMap((Gamekey)i, GamepadKeys[i]);
					}
				} else {
					for (int i = 0; i < KeyboardKeys.Length; i++) {
						FrameInput.SetKeyboardMap((Gamekey)i, KeyboardKeys[i]);
					}
				}
			}

			// Back
			if (DrawItem(Language.Get(WORD.UI_BACK))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(forGamepad ? 1 : 0);
			}

			// Record
			if (RecordingKey >= 0 && !RecordLock) {
				if (forGamepad) {
					// Gamepad
					if (FrameInput.TryGetHoldingGamepadButton(out var button)) {
						if (GamepadKeys[RecordingKey] != button) {
							for (int i = 0; i < GamepadKeys.Length; i++) {
								if (GamepadKeys[i] == button && GamepadKeys[RecordingKey] != button) {
									GamepadKeys[i] = GamepadKeys[RecordingKey];
								}
							}
							GamepadKeys[RecordingKey] = button;
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
						if (KeyboardKeys[RecordingKey] != button) {
							for (int i = 0; i < KeyboardKeys.Length; i++) {
								if (KeyboardKeys[i] == button && KeyboardKeys[RecordingKey] != button) {
									KeyboardKeys[i] = KeyboardKeys[RecordingKey];
								}
							}
							KeyboardKeys[RecordingKey] = button;
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
			if (FrameInput.KeyboardDown(Key.F1)) {
				if (forGamepad) {
					for (int i = 0; i < GamepadKeys.Length; i++) {
						GamepadKeys[i] = FrameInput.GetDefaultGamepadMap((Gamekey)i);
					}
				} else {
					for (int i = 0; i < KeyboardKeys.Length; i++) {
						KeyboardKeys[i] = FrameInput.GetDefaultKeyboardMap((Gamekey)i);
					}
				}
			}

		}


		#endregion




		#region --- API ---


		public void SetAsPauseMode () => Mode = RequireMode = MenuMode.Pause;
		public void SetAsQuitMode () => Mode = RequireMode = MenuMode.Quit;


		#endregion




	}
}
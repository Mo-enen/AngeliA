using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Yaya {
	[EntityAttribute.DontDespawnWhenOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public class ePauseMenu : MenuUI {


		// SUB
		private enum MenuMode { Pause, Setting, KeySetter, Quit, Setter_Keyboard, Setter_Gamepad }

		// VAR
		private static readonly int MENU_SELECTION_CODE = "Menu Selection Mark".AngeHash();
		private static readonly int MENU_MORE_CODE = "Menu More Mark".AngeHash();
		private static readonly int MENU_ARROW_MARK = "Menu Arrow Mark".AngeHash();

		public bool QuitMode => Mode == MenuMode.Quit;
		protected override int BackgroundCode => YayaConst.UI_PIXEL;
		protected override int SelectionMarkCode => MENU_SELECTION_CODE;
		protected override int MoreItemMarkCode => MENU_MORE_CODE;
		protected override int ArrowMarkCode => MENU_ARROW_MARK;
		protected override Int4 ContentPadding => new(32, 32, 46, 46);
		protected override Color32 ScreenTint => new(0, 0, 0, 128);
		protected override Color32 BackgroundTint => new(0, 0, 0, 255);
		protected override int TargetItemCount => Mode switch {
			MenuMode.Pause => 4,
			MenuMode.Setting => 3,
			MenuMode.KeySetter => 3,
			MenuMode.Quit => 2,
			MenuMode.Setter_Keyboard => 10,
			MenuMode.Setter_Gamepad => 10,
			_ => throw new System.NotImplementedException(),
		};
		protected override string Message => Mode switch {
			MenuMode.Setter_Gamepad => Language.Get(WORD.MENU_KEYSETTER_GAMEPAD_MESSAGE),
			MenuMode.Setter_Keyboard => Language.Get(WORD.MENU_KEYSETTER_KEYBOARD_MESSAGE),
			MenuMode.Quit => Language.Get(WORD.MENU_QUIT_MESSAGE),
			_ => string.Empty,
		};
		protected override bool Interactable => (Mode != MenuMode.Setter_Gamepad && Mode != MenuMode.Setter_Keyboard) || RecordingKey < 0;

		private readonly Key[] KeyboardKeys = new Key[8];
		private readonly GamepadButton[] GamepadKeys = new GamepadButton[8];
		private MenuMode Mode = MenuMode.Pause;
		private MenuMode RequireMode = MenuMode.Pause;
		private int RecordingKey = -1;
		private int PauselessFrame = 0;
		private bool RecordLock = true;


		// MSG
		public override void FrameUpdate () {
			var cameraRect = CellRenderer.CameraRect;
			X = cameraRect.x + cameraRect.width / 2 - 250 * UNIT;
			Width = 400 * UNIT;
			Mode = RequireMode;
			base.FrameUpdate();
			Y = cameraRect.y + cameraRect.height / 2 - Height / 2;
			PauselessFrame++;
		}


		public override void OnInactived () {
			base.OnInactived();
			Game.Current.IsPausing = false;
		}


		protected override void DrawMenu () {
			switch (Mode) {
				case MenuMode.Pause:
					MenuPause();
					break;
				case MenuMode.Setting:
					MenuSetting();
					break;
				case MenuMode.KeySetter:
					MenuKeySetter();
					break;
				case MenuMode.Quit:
					MenuQuit();
					break;
				case MenuMode.Setter_Keyboard:
					MenuSetter_Keyboard();
					break;
				case MenuMode.Setter_Gamepad:
					MenuSetter_Gamepad();
					break;
			}
		}


		// Menus
		private void MenuPause () {

			// 0-Continue
			if (DrawItem(Language.Get(WORD.UI_CONTINUE)) || FrameInput.GetKeyDown(GameKey.Jump)) {
				Game.Current.IsPausing = false;
				Active = false;
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


		private void MenuKeySetter () {

			if (DrawItem(Language.Get(WORD.MENU_SETTER_KEYBOARD))) {
				RequireMode = MenuMode.Setter_Keyboard;
				SetSelection(0);
				RecordingKey = -1;
				RecordLock = true;
				for (int i = 0; i < KeyboardKeys.Length; i++) {
					KeyboardKeys[i] = FrameInput.GetKeyboardMap((GameKey)i);
				}
			}

			if (DrawItem(Language.Get(WORD.MENU_SETTER_GAMEPAD))) {
				RequireMode = MenuMode.Setter_Gamepad;
				SetSelection(0);
				RecordingKey = -1;
				RecordLock = true;
				for (int i = 0; i < GamepadKeys.Length; i++) {
					GamepadKeys[i] = FrameInput.GetGamepadMap((GameKey)i);
				}
			}

			if (DrawItem(Language.Get(WORD.UI_BACK)) || FrameInput.GetKeyDown(GameKey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(1);
			}
		}


		private void MenuSetting () {

			// Framerate
			if (DrawArrowItem(
				Language.Get(WORD.MENU_FRAMERATE),
				new(Game.Current.GraphicFramerate.ToString()),
				out int delta
			)) {
				Game.Current.GraphicFramerate += delta * 30;
			}

			// Language
			if (DrawArrowItem(Language.Get(WORD.MENU_LANGUAGE), new(Util.GetLanguageDisplayName(Game.Current.CurrentLanguage)), out delta)) {
				// Get Current Index
				int index = 0;
				for (int i = 0; i < Language.LanguageCount; i++) {
					var lan = Language.GetLanguageAt(i);
					if (lan == Game.Current.CurrentLanguage) {
						index = i;
						break;
					}
				}
				int newIndex = index + delta;
				newIndex = newIndex.Clamp(0, Language.LanguageCount - 1);
				if (newIndex != index) {
					Game.Current.SetLanguage(Language.GetLanguageAt(newIndex));
				}
			}

			// Back
			if (DrawItem(Language.Get(WORD.UI_BACK)) || FrameInput.GetKeyDown(GameKey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(2);
			}
		}


		private void MenuQuit () {

			if (DrawItem(Language.Get(WORD.UI_BACK)) || FrameInput.GetKeyDown(GameKey.Jump)) {
				RequireMode = MenuMode.Pause;
				SetSelection(3);
			}

			if (DrawItem(Language.Get(WORD.UI_QUIT), Const.RED_BETTER)) {
				Application.Quit();
			}

		}


		private void MenuSetter_Keyboard () {




			// Back
			if (DrawItem(Language.Get(WORD.MENU_KEYSETTER_SAVE_BACK))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(0);
				for (int i = 0; i < KeyboardKeys.Length; i++) {
					FrameInput.SetKeyboardMap((GameKey)i, KeyboardKeys[i]);
				}
			}

			// Back
			if (DrawItem(Language.Get(WORD.UI_BACK))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(0);
			}

		}


		private void MenuSetter_Gamepad () {

			// All Game Keys
			for (int i = 0; i < WORD.GAMEKEY_UI_CODES.Length; i++) {
				int code = WORD.GAMEKEY_UI_CODES[i];
				var gamepadKey = GamepadKeys[i];
				int gCode = YayaConst.GAMEPAD_CODE.TryGetValue(gamepadKey, out var _value0) ? _value0 : 0;
				if (DrawItem(
					Language.Get(code),
					RecordingKey == i ?
						new CellLabel(Language.Get(WORD.MENU_SETTER_RECORD)) {
							Tint = PauselessFrame % 30 > 15 ? Const.BLACK : Const.WHITE,
							BackgroundTint = PauselessFrame % 30 > 15 ? Const.GREEN : Const.CLEAR
						} :
						new CellLabel() { Image = gCode }
				)) {
					RecordLock = true;
					RecordingKey = i;
				}
			}

			// Save Back
			if (DrawItem(Language.Get(WORD.MENU_KEYSETTER_SAVE_BACK))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(1);
				for (int i = 0; i < GamepadKeys.Length; i++) {
					FrameInput.SetGamepadMap((GameKey)i, GamepadKeys[i]);
				}
			}

			// Back
			if (DrawItem(Language.Get(WORD.UI_BACK))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(1);
			}

			// Record
			if (RecordingKey >= 0 && !RecordLock) {
				if (FrameInput.AnyGamepadButtonDown(out var button)) {
					if (GamepadKeys[RecordingKey] != button) {
						for (int i = 0; i < GamepadKeys.Length; i++) {
							if (GamepadKeys[i] == button && GamepadKeys[RecordingKey] != button) {
								GamepadKeys[i] = GamepadKeys[RecordingKey];
							}
						}
						GamepadKeys[RecordingKey] = button;
					}
					RecordingKey = -1;
					FrameInput.UseGameKey(GameKey.Start);
					FrameInput.UseCustomKey(Key.Escape);
				} else if (FrameInput.AnyKeyboardButtonDown(out _)) {
					RecordingKey = -1;
					FrameInput.UseGameKey(GameKey.Start);
					FrameInput.UseCustomKey(Key.Escape);
				}
			}

			// Unlock Record
			if (
				RecordLock &&
				!FrameInput.AnyGamepadButtonDown(out _) &&
				!FrameInput.AnyKeyboardButtonDown(out _)
			) {
				RecordLock = false;
			}

			// Reset
			if (FrameInput.CustomKeyDown(Key.F1)) {
				for (int i = 0; i < GamepadKeys.Length; i++) {
					GamepadKeys[i] = FrameInput.GetDefaultGamepadMap((GameKey)i); ;
				}
			}

		}


		// API
		public void SetAsPauseMode () => Mode = RequireMode = MenuMode.Pause;
		public void SetAsQuitMode () => Mode = RequireMode = MenuMode.Quit;


	}
}
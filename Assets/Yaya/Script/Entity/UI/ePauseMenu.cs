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

		private static readonly int QUIT_MESSAGE = "UI.QuitMessage".AngeHash();
		private static readonly int MENU_KEY_SETTER = "Menu.Pause.KeySetter".AngeHash();
		private static readonly int MENU_SETTER_KEYBOARD = "Menu.KeySetter.Keyboard".AngeHash();
		private static readonly int MENU_SETTER_GAMEPAD = "Menu.KeySetter.Gamepad".AngeHash();
		private static readonly int MENU_FRAMERATE = "Menu.Setting.Framerate".AngeHash();
		private static readonly int MENU_LANGUAGE = "Menu.Setting.Language".AngeHash();
		private static readonly int[] GAMEKEY_CODES = new int[] {
			$"GameKey.{GameKey.Left}".AngeHash(),
			$"GameKey.{GameKey.Right}".AngeHash(),
			$"GameKey.{GameKey.Down}".AngeHash(),
			$"GameKey.{GameKey.Up}".AngeHash(),
			$"GameKey.{GameKey.Action}".AngeHash(),
			$"GameKey.{GameKey.Jump}".AngeHash(),
			$"GameKey.{GameKey.Start}".AngeHash(),
			$"GameKey.{GameKey.Select}".AngeHash(),
		};

		public bool QuitMode => Mode == MenuMode.Quit;
		protected override Int4 ContentPadding => new(32, 32, 46, 46);
		protected override Color32 ScreenTint => new(0, 0, 0, 128);
		protected override int SelectionMarkCode => MENU_SELECTION_CODE;
		protected override int MoreItemMarkCode => MENU_MORE_CODE;
		protected override int ArrowMarkCode => MENU_ARROW_MARK;
		protected override int TargetItemCount => Mode switch {
			MenuMode.Pause => 4,
			MenuMode.Setting => 3,
			MenuMode.KeySetter => 3,
			MenuMode.Quit => 2,
			MenuMode.Setter_Keyboard => 9,
			MenuMode.Setter_Gamepad => 9,
			_ => throw new System.NotImplementedException(),
		};
		protected override string Message => Mode switch {
			MenuMode.Quit => Language.Get(QUIT_MESSAGE),
			_ => string.Empty,
		};

		private MenuMode Mode = MenuMode.Pause;
		private MenuMode RequireMode = MenuMode.Pause;
		private readonly Key[] KeyboardKeys = new Key[8];
		private readonly GamepadButton[] GamepadKeys = new GamepadButton[8];


		// MSG
		public override void FrameUpdate () {
			var cameraRect = CellRenderer.CameraRect;
			X = cameraRect.x + cameraRect.width / 2 - 250 * UNIT;
			Width = 400 * UNIT;
			Mode = RequireMode;
			base.FrameUpdate();
			Y = cameraRect.y + cameraRect.height / 2 - Height / 2;
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
			if (DrawItem(Language.Get(YayaConst.UI_CONTINUE))) {
				Game.Current.IsPausing = false;
				Active = false;
			}

			// 1-Key Setter
			if (DrawItem(Language.Get(MENU_KEY_SETTER))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(0);
			}

			// 2-Setting
			if (DrawItem(Language.Get(YayaConst.UI_SETTING))) {
				RequireMode = MenuMode.Setting;
				SetSelection(0);
			}

			// 3-Quit
			if (DrawItem(Language.Get(YayaConst.UI_QUIT), Const.RED_BETTER)) {
				RequireMode = MenuMode.Quit;
				SetSelection(0);
			}
		}


		private void MenuKeySetter () {

			if (DrawItem(Language.Get(MENU_SETTER_KEYBOARD))) {
				RequireMode = MenuMode.Setter_Keyboard;
				SetSelection(0);
				for (int i = 0; i < KeyboardKeys.Length; i++) {
					KeyboardKeys[i] = FrameInput.GetKeyboardMap((GameKey)i);
				}
			}

			if (DrawItem(Language.Get(MENU_SETTER_GAMEPAD))) {
				RequireMode = MenuMode.Setter_Gamepad;
				SetSelection(0);
				for (int i = 0; i < GamepadKeys.Length; i++) {
					GamepadKeys[i] = FrameInput.GetGamepadMap((GameKey)i);
				}
			}

			if (DrawItem(Language.Get(YayaConst.UI_BACK))) {
				RequireMode = MenuMode.Pause;
				SetSelection(1);
			}
		}


		private void MenuSetting () {

			// Framerate
			if (DrawArrowItem(Language.Get(MENU_FRAMERATE), Game.Current.GraphicFramerate.ToString(), out int delta)) {
				Game.Current.GraphicFramerate += delta * 30;
			}

			// Language
			if (DrawArrowItem(Language.Get(MENU_LANGUAGE), Util.GetLanguageDisplayName(Game.Current.CurrentLanguage), out delta)) {
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
			if (DrawItem(Language.Get(YayaConst.UI_BACK))) {
				RequireMode = MenuMode.Pause;
				SetSelection(2);
			}
		}


		private void MenuQuit () {

			if (DrawItem(Language.Get(YayaConst.UI_BACK))) {
				RequireMode = MenuMode.Pause;
				SetSelection(3);
			}

			if (DrawItem(Language.Get(YayaConst.UI_QUIT), Const.RED_BETTER)) {
				Application.Quit();
			}

		}


		private void MenuSetter_Keyboard () {




			if (DrawItem(Language.Get(YayaConst.UI_BACK))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(0);
				for (int i = 0; i < KeyboardKeys.Length; i++) {
					FrameInput.SetKeyboardMap((GameKey)i, KeyboardKeys[i]);
				}
			}
		}


		private void MenuSetter_Gamepad () {

			// All Game Keys
			for (int i = 0; i < GAMEKEY_CODES.Length; i++) {
				int code = GAMEKEY_CODES[i];
				var gamepadKey = GamepadKeys[i];
				int gCode = YayaConst.GAMEPAD_CODE.TryGetValue(gamepadKey, out var _value0) ? _value0 : 0;
				if (DrawArrowItem(Language.Get(code), gCode, out int delta)) {



				}
			}


			// Back
			if (DrawItem(Language.Get(YayaConst.UI_BACK))) {
				RequireMode = MenuMode.KeySetter;
				SetSelection(1);
				for (int i = 0; i < GamepadKeys.Length; i++) {
					FrameInput.SetGamepadMap((GameKey)i, GamepadKeys[i]);
				}
			}
		}


		// API
		public void SetAsPauseMode () => Mode = RequireMode = MenuMode.Pause;
		public void SetAsQuitMode () => Mode = RequireMode = MenuMode.Quit;


	}
}
using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace Yaya {
	public static class WORD {


		// UI General
		public static readonly int UI_OK = "UI.OK".AngeHash();
		public static readonly int UI_QUIT = "UI.Quit".AngeHash();
		public static readonly int UI_CANCEL = "UI.Cancel".AngeHash();
		public static readonly int UI_SETTING = "UI.Setting".AngeHash();
		public static readonly int UI_CONTINUE = "UI.Continue".AngeHash();
		public static readonly int UI_BACK = "UI.Back".AngeHash();
		public static readonly int UI_SAVE = "UI.Save".AngeHash();
		public static readonly int UI_ON = "UI.ON".AngeHash();
		public static readonly int UI_OFF = "UI.OFF".AngeHash();
		public static readonly int[] GAMEKEY_UI_CODES = new int[] {
			$"UI.GameKey.{GameKey.Left}".AngeHash(),
			$"UI.GameKey.{GameKey.Right}".AngeHash(),
			$"UI.GameKey.{GameKey.Down}".AngeHash(),
			$"UI.GameKey.{GameKey.Up}".AngeHash(),
			$"UI.GameKey.{GameKey.Action}".AngeHash(),
			$"UI.GameKey.{GameKey.Jump}".AngeHash(),
			$"UI.GameKey.{GameKey.Start}".AngeHash(),
			$"UI.GameKey.{GameKey.Select}".AngeHash(),
		};

		// Menu
		public static readonly int MENU_QUIT_MESSAGE = "Menu.Pause.QuitMessage".AngeHash();
		public static readonly int MENU_KEYSETTER_GAMEPAD_MESSAGE = "Menu.KeySetter.GamepadMessage".AngeHash();
		public static readonly int MENU_KEYSETTER_KEYBOARD_MESSAGE = "Menu.KeySetter.KeyboardMessage".AngeHash();
		public static readonly int MENU_KEY_SETTER = "Menu.Pause.KeySetter".AngeHash();
		public static readonly int MENU_SETTER_KEYBOARD = "Menu.KeySetter.Keyboard".AngeHash();
		public static readonly int MENU_SETTER_GAMEPAD = "Menu.KeySetter.Gamepad".AngeHash();
		public static readonly int MENU_SETTER_RECORD = "Menu.KeySetter.Record".AngeHash();
		public static readonly int MENU_MUSIC_VOLUME = "Menu.Setting.MusicVolume".AngeHash();
		public static readonly int MENU_SOUND_VOLUME = "Menu.Setting.SoundVolume".AngeHash();
		public static readonly int MENU_FRAMERATE = "Menu.Setting.Framerate".AngeHash();
		public static readonly int MENU_LANGUAGE = "Menu.Setting.Language".AngeHash();
		public static readonly int MENU_KEYSETTER_SAVE_BACK = "Menu.KeySetter.SaveAndBack".AngeHash();
		public static readonly int MENU_FULLSCREEN_0 = "Menu.Setting.Fullscreen.0".AngeHash();
		public static readonly int MENU_FULLSCREEN_1 = "Menu.Setting.Fullscreen.1".AngeHash();
		public static readonly int MENU_FULLSCREEN_2 = "Menu.Setting.Fullscreen.2".AngeHash();
		public static readonly int MENU_FULLSCREEN_LABEL = "Menu.Setting.Fullscreen.Label".AngeHash();
		public static readonly int MENU_VSYNC = "Menu.Setting.VSync".AngeHash();
		public static readonly int MENU_SCREEN_EFFECT = "Menu.Setting.ScreenEffect".AngeHash();

		// Hint
		public static readonly int HINT_MOVE_CODE = "CtrlHint.Move".AngeHash();
		public static readonly int HINT_VALUE_CODE = "CtrlHint.Adjust".AngeHash();
		public static readonly int HINT_JUMP_CODE = "CtrlHint.Jump".AngeHash();
		public static readonly int HINT_ATTACK_CODE = "CtrlHint.Attack".AngeHash();
		public static readonly int HINT_USE_CODE = "CtrlHint.Use".AngeHash();
		public static readonly int HINT_WAKE_CODE = "CtrlHint.WakeUp".AngeHash();
		public static readonly int HINT_SKIP_CODE = "CtrlHint.Skip".AngeHash();



	}
}
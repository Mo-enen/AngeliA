using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDespawnWhenOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public class ePauseMenu : MenuUI {


		// SUB
		private enum MenuMode { Pause, Setting, Keyboard, Quit }

		// VAR
		private static readonly int MENU_SELECTION_CODE = "Menu Selection Mark".AngeHash();
		private static readonly int MENU_MORE_CODE = "Menu More Mark".AngeHash();
		private static readonly int MENU_ARROW_MARK = "Menu Arrow Mark".AngeHash();
		private static readonly int MENU_KEYBOARD = "Menu.Pause.Keyboard".AngeHash();
		private static readonly int QUIT_MESSAGE = "UI.QuitMessage".AngeHash();

		public bool QuitMode => Mode == MenuMode.Quit;
		protected override Int4 ContentPadding => new(32, 32, 46, 46);
		protected override Color32 ScreenTint => new(0, 0, 0, 128);
		protected override int SelectionMarkCode => MENU_SELECTION_CODE;
		protected override int MoreItemMarkCode => MENU_MORE_CODE;
		protected override int ArrowMarkCode => MENU_ARROW_MARK;
		protected override int TargetItemCount => Mode switch {
			MenuMode.Pause => 4,
			MenuMode.Setting => 7,
			MenuMode.Keyboard => 7,
			MenuMode.Quit => 2,
			_ => throw new System.NotImplementedException(),
		};
		protected override string Message => Mode switch {
			MenuMode.Quit => Language.Get(QUIT_MESSAGE),
			_ => string.Empty,
		};

		private MenuMode Mode = MenuMode.Pause;
		private MenuMode RequireMode = MenuMode.Pause;


		// MSG
		public override void FrameUpdate () {
			var cameraRect = CellRenderer.CameraRect;
			X = cameraRect.x + cameraRect.width / 2 - 250 * UNIT;
			Y = cameraRect.y + cameraRect.height / 2 - 150 * UNIT;
			Width = 400 * UNIT;
			//FrameInput.IgnoreMouseForThisFrame();
			Mode = RequireMode;
			base.FrameUpdate();
		}


		protected override void DrawMenu () {
			switch (Mode) {
				case MenuMode.Pause:
					MenuPause();
					break;
				case MenuMode.Setting:
					MenuSetting();
					break;
				case MenuMode.Keyboard:
					MenuKeyboard();
					break;
				case MenuMode.Quit:
					MenuQuit();
					break;
			}
		}


		private void MenuPause () {

			// Continue
			if (DrawItem(Language.Get(YayaConst.UI_CONTINUE))) {
				Game.Current.IsPausing = false;
				Active = false;
			}

			// Keyboard
			if (DrawItem(Language.Get(MENU_KEYBOARD))) {
				RequireMode = MenuMode.Keyboard;
				SetSelection(0);
			}

			// Setting
			if (DrawItem(Language.Get(YayaConst.UI_SETTING))) {
				RequireMode = MenuMode.Setting;
				SetSelection(0);
			}

			// Quit
			if (DrawItem(Language.Get(YayaConst.UI_QUIT), Const.RED_BETTER)) {
				RequireMode = MenuMode.Quit;
				SetSelection(0);
			}
		}


		private void MenuKeyboard () {



			if (DrawItem(Language.Get(YayaConst.UI_BACK))) {
				RequireMode = MenuMode.Pause;
				SetSelection(1);
			}
		}


		private void MenuSetting () {



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


		// API
		public void SetAsPauseMode () => Mode = MenuMode.Pause;
		public void SetAsQuitMode () => Mode = MenuMode.Quit;


	}
}
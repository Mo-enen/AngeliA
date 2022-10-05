using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.DontDespawnWhenOutOfRange]
	[EntityAttribute.DontDestroyOnSquadTransition]
	public class ePauseMenu : MenuUI {


		// SUB
		private enum MenuMode { Pause, Setting, Keyboard, }

		// VAR
		private static readonly int MENU_SELECTION_CODE = "Menu Selection Mark".AngeHash();
		private static readonly int MENU_MORE_CODE = "Menu More Mark".AngeHash();
		private static readonly int MENU_KEYBOARD = "Menu.Pause.Keyboard".AngeHash();

		protected override Int4 ContentPadding => new(32, 32, 46, 46);
		protected override Color32 ScreenTint => new(0, 0, 0, 128);
		protected override int SelectionMarkCode => MENU_SELECTION_CODE;
		protected override int MoreItemMarkCode => MENU_MORE_CODE;
		protected override int TargetItemCount => Mode == MenuMode.Pause ? 4 : 7;

		private MenuMode Mode = MenuMode.Pause;


		// MSG
		public override void OnActived () {
			base.OnActived();
			Mode = MenuMode.Pause;
		}


		public override void FrameUpdate () {
			var cameraRect = CellRenderer.CameraRect;
			X = cameraRect.x + cameraRect.width / 2 - 250 * UNIT;
			Y = cameraRect.y + cameraRect.height / 2 - 150 * UNIT;
			Width = 400 * UNIT;
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
				Mode = MenuMode.Keyboard;
				SetSelection(0);
			}

			// Setting
			if (DrawItem(Language.Get(YayaConst.UI_SETTING))) {
				Mode = MenuMode.Setting;
				SetSelection(0);
			}

			// Quit
			if (DrawItem(Language.Get(YayaConst.UI_QUIT))) {
				Application.Quit();
			}
		}


		private void MenuSetting () {



			if (DrawItem(Language.Get(YayaConst.UI_BACK))) {
				Mode = MenuMode.Pause;
				SetSelection(2);
			}
		}


		private void MenuKeyboard () {



			if (DrawItem(Language.Get(YayaConst.UI_BACK))) {
				Mode = MenuMode.Pause;
				SetSelection(1);
			}
		}




	}
}
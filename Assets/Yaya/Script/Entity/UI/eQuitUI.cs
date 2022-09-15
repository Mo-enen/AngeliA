using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class eQuitDialog : eDialogUI {


		// Const
		private static readonly int QUIT_MESSAGE = "UI.QuitMessage".AngeHash();
		private static readonly int BACKGROUND = "Dialog Background".AngeHash();
		private static readonly int BUTTON = "Dialog Button".AngeHash();
		private static readonly int BUTTON_HIGHLIGHT = "Dialog Button Highlight".AngeHash();

		// Api
		protected override int ButtonCount => 2;
		protected override string Message => Language.Get(QUIT_MESSAGE);
		protected override int ArtworkCode_Background => BACKGROUND;
		protected override int ArtworkCode_Button => BUTTON;
		protected override int ArtworkCode_Button_Highlight => BUTTON_HIGHLIGHT;
		protected override Color32 BackMaskTint => new(0, 0, 0, 128);
		protected override int LayerID => YayaConst.SHADER_UI;


		// Override
		protected override string GetButtonLabel (int index) => Language.Get(index == 0 ? YayaConst.UI_QUIT : YayaConst.UI_CANCEL);


		// Msg
		protected override void UpdateForUI () {
			if (!Game.IsPausing) {
				Active = false;
				return;
			}
			base.UpdateForUI();
		}


		protected override void OnButtonClick (int index) {
			if (index == 0) Application.Quit();
			Game.IsPausing = false;
			base.OnButtonClick(index);
		}
		protected override Color32 GetButtonTint (int index) => index == 0 ? new(255, 64, 0, 255) : Const.WHITE;
		protected override Color32 GetButtonLabelTint (int index) => index == 0 ? Const.WHITE : new(16, 16, 16, 255);


	}
}
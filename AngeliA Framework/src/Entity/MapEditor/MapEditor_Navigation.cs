using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class MapEditor {


	// MSG
	private void Active_Navigation () {

	}


	private void Update_Navigation () {

		// Switch Mode Hotkey
		if (Input.KeyboardDown(KeyboardKey.Tab)) {
			Input.UseKeyboardKey(KeyboardKey.Tab);
			SetNavigationMode(false);
		}
		if (Input.KeyboardDown(KeyboardKey.Escape)) {
			Input.UseKeyboardKey(KeyboardKey.Escape);
			Input.UseGameKey(Gamekey.Start);
			SetNavigationMode(false);
		}
		ControlHintUI.AddHint(KeyboardKey.Tab, BuiltInText.UI_BACK);

		// Back Btn
		if (GUI.Button(
			Renderer.CameraRect.CornerInside(Alignment.TopLeft, Unify(46)).Shrink(Unify(6)), 
			BuiltInSprite.ICON_BACK
		)) {
			Input.UseAllMouseKey();
			SetNavigationMode(false);
		}

		// Update
		Update_NavigationView();
		Update_NavigationRendering();

	}


	private void Update_NavigationView () {



	}


	private void Update_NavigationRendering () {







	}


	// LGC
	private void SetNavigationMode (bool navigating) => RequireIsNavigating = navigating;


}
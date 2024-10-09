using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class MapEditor {


	// VAR
	private IRect NavWorldDoodledUnitRange = default;

	// MSG
	[OnWindowSizeChanged]
	internal static void OnWindowSizeChanged () {
		if (Instance == null) return;
		Instance.NavWorldDoodledUnitRange = default;
	}


	private void Active_Navigation () {
		NavWorldDoodledUnitRange = default;
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


		// TODO:  Doodle for DeltaX, Doodle for DeltaY, Over-Paint the Overlaped part
		






	}


	// LGC
	private void SetNavigationMode (bool navigating) => RequireIsNavigating = navigating;


}
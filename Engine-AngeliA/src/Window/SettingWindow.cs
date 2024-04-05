using System.Collections;
using System.Collections.Generic;
using AngeliA;


namespace AngeliaEngine;

[RequireLanguageFromField]
public class SettingWindow : WindowUI {

	// VAR


	// MSG
	public override void UpdateWindowUI () {

		var rect = WindowRect.Shrink(Unify(18)).EdgeInside(Direction4.Up, Unify(32));



	}

}
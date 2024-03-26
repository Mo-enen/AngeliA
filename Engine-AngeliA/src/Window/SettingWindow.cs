using System.Collections;
using System.Collections.Generic;
using AngeliA;


namespace AngeliaEngine;

[RequireLanguageFromField]
public class SettingWindow : WindowUI {

	// VAR
	//private static readonly LanguageCode LABEL_TO_MASCOT_LOST_FOCUS = ("Setting.MascotOnLostFocus", "Switch to Mascot on Lost Focus");
	//private static readonly SavingBool _SwitchToMascotOnLostFocus = new("Setting.ToMascotOnLostFocus", false);
	//
	//public static bool SwitchToMascotOnLostFocus => _SwitchToMascotOnLostFocus.Value;

	// MSG
	public override void UpdateWindowUI () {

		//var rect = WindowRect.Shrink(Unify(18)).EdgeInside(Direction4.Up, Unify(32));
		//
		//// Mascot On Lost Focus
		//_SwitchToMascotOnLostFocus.Value = GUI.ToggleLeft(rect, _SwitchToMascotOnLostFocus.Value, LABEL_TO_MASCOT_LOST_FOCUS);
		//rect.y -= rect.height;


	}

}
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace MarioTemplate;

[EntityAttribute.Capacity(1, 1)]
public class StarterHintUI : Entity {

	// VAR
	private const string Disclaimer = @"Disclaimer

This template project is a non-commercial example included with AngeliA Engine. It is not affiliated with, endorsed by, or associated with Nintendo or any of its subsidiaries. All trademarks, characters, and assets related to the Super Mario franchise remain the property of Nintendo.

This project is provided for educational purposes only, demonstrating the capabilities of the engine. Users are responsible for ensuring their own projects comply with copyright laws. If you are a representative of Nintendo and have concerns, please contact me, and I will promptly address any issues.";

	private static readonly GUIStyle DISC_STYLE = new() {
		Alignment = Alignment.BottomLeft,
		Clip = false,
		Wrap = WrapMode.WordWrap,
		CharSize = 22,
	};

	// MSG
	public override void LateUpdate () {
		base.LateUpdate();
		int height = GUI.Unify(32);

		ControlHintUI.DrawGlobalHint(X, Y, Gamekey.Left, Gamekey.Right, BuiltInText.HINT_MOVE, true);
		ControlHintUI.DrawGlobalHint(X, Y + height, Gamekey.Jump, BuiltInText.HINT_JUMP, true);

		GUI.ShadowLabel(
			new IRect(X - Const.CEL * 6, Y + height * 2, Const.CEL * 16, 1),
			Disclaimer,
			shadowDistance: 2,
			style: DISC_STYLE
		);

	}

}

using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace MarioTemplate;

[EntityAttribute.Capacity(1, 1)]
public class StarterHintUI : Entity {

	public override void LateUpdate () {
		base.LateUpdate();
		int height = GUI.Unify(32);
		ControlHintUI.DrawGlobalHint(X, Y, Gamekey.Left, Gamekey.Right, BuiltInText.HINT_MOVE, true);
		ControlHintUI.DrawGlobalHint(X, Y + height, Gamekey.Jump, BuiltInText.HINT_JUMP, true);
	}

}

using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Framework;

public static class GUISkin {

	public static readonly GUIStyle None = new() {
		BodySprite = 0,
		BodyHighlightSprite = 0,
		BodyPressSprite = 0,
		BodyDisableSprite = 0,
	};
	public static readonly GUIStyle Button = new() {
		BodySprite = BuiltInSprite.UI_BUTTON,
		BodyHighlightSprite = BuiltInSprite.UI_BUTTON,
		BodyPressSprite = BuiltInSprite.UI_BUTTON_DOWN,
		BodyDisableSprite = BuiltInSprite.UI_BUTTON,
	};

}

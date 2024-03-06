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
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentDisableShift = new(0, Const.ART_SCALE * 2),
		ContentHighlightShift = new(0, Const.ART_SCALE * 2),
	};

	public static readonly GUIStyle DarkButton = new() {
		BodySprite = BuiltInSprite.UI_DARK_BUTTON,
		BodyHighlightSprite = BuiltInSprite.UI_DARK_BUTTON,
		BodyPressSprite = BuiltInSprite.UI_DARK_BUTTON_DOWN,
		BodyDisableSprite = BuiltInSprite.UI_DARK_BUTTON,
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentDisableShift = new(0, Const.ART_SCALE * 2),
		ContentHighlightShift = new(0, Const.ART_SCALE * 2),
	};

	public static readonly GUIStyle ItemFrame = new() {
		BodySprite = BuiltInSprite.UI_ITEM_FRAME,
		BodyHighlightSprite = BuiltInSprite.UI_ITEM_FRAME,
		BodyPressSprite = BuiltInSprite.UI_ITEM_FRAME,
		BodyDisableSprite = BuiltInSprite.UI_ITEM_FRAME,
		ContentDisableColor = Color32.GREY_196,
	};

	public static readonly GUIStyle Label = new() {
		BodyColor = Color32.CLEAR,
		BodyHighlightColor = Color32.WHITE,
		BodyPressColor = Color32.GREY_230,
		BodyDisableColor = Color32.GREY_196,
		ContentColor = Color32.CLEAR,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_196,
	};

}

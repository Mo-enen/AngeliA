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

	public static readonly GUIStyle GreenPixel = new() {
		BodySprite = Const.PIXEL,
		BodyHighlightSprite = Const.PIXEL,
		BodyPressSprite = Const.PIXEL,
		BodyDisableSprite = Const.PIXEL,
		BodyColor = Color32.GREEN,
		BodyHighlightColor = Color32.GREEN,
		BodyPressColor = Color32.GREEN,
		BodyDisableColor = Color32.GREEN,
	};

	public static readonly GUIStyle ItemFrame = new() {
		BodySprite = BuiltInSprite.UI_ITEM_FRAME,
		BodyHighlightSprite = BuiltInSprite.UI_ITEM_FRAME,
		BodyPressSprite = BuiltInSprite.UI_ITEM_FRAME,
		BodyDisableSprite = BuiltInSprite.UI_ITEM_FRAME,
		ContentDisableColor = Color32.GREY_196,
	};


	// Button
	public static readonly GUIStyle Button = new() {
		BodySprite = BuiltInSprite.UI_BUTTON,
		BodyHighlightSprite = BuiltInSprite.UI_BUTTON,
		BodyPressSprite = BuiltInSprite.UI_BUTTON_DOWN,
		BodyDisableSprite = BuiltInSprite.UI_BUTTON,
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentDisableShift = new(0, Const.ART_SCALE * 2),
		ContentHighlightShift = new(0, Const.ART_SCALE * 2),
		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),

		ContentColor = Color32.WHITE,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_196,
		CharSize = -1,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle DarkButton = new() {
		BodySprite = BuiltInSprite.UI_DARK_BUTTON,
		BodyHighlightSprite = BuiltInSprite.UI_DARK_BUTTON,
		BodyPressSprite = BuiltInSprite.UI_DARK_BUTTON_DOWN,
		BodyDisableSprite = BuiltInSprite.UI_DARK_BUTTON,
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentDisableShift = new(0, Const.ART_SCALE * 2),
		ContentHighlightShift = new(0, Const.ART_SCALE * 2),
		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),

		ContentColor = Color32.WHITE,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_196,
		CharSize = -1,
		Alignment = Alignment.MidMid,
	};


	// Toggle
	public static readonly GUIStyle Toggle = new() {
		BodySprite = BuiltInSprite.UI_TOGGLE,
		BodyHighlightSprite = BuiltInSprite.UI_TOGGLE,
		BodyPressSprite = BuiltInSprite.UI_TOGGLE_DOWN,
		BodyDisableSprite = BuiltInSprite.UI_TOGGLE,
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentDisableShift = new(0, Const.ART_SCALE * 2),
		ContentHighlightShift = new(0, Const.ART_SCALE * 2),
		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),
	};

	public static readonly GUIStyle ToggleMark = new() {
		BodySprite = BuiltInSprite.UI_TOGGLE_MARK,
		BodyHighlightSprite = BuiltInSprite.UI_TOGGLE_MARK,
		BodyPressSprite = BuiltInSprite.UI_TOGGLE_MARK,
		BodyDisableSprite = BuiltInSprite.UI_TOGGLE_MARK,
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentDisableShift = new(0, Const.ART_SCALE * 2),
		ContentHighlightShift = new(0, Const.ART_SCALE * 2),
	};


	// Text
	public static readonly GUIStyle Label = new() {
		BodyColor = Color32.CLEAR,
		BodyHighlightColor = Color32.WHITE,
		BodyPressColor = Color32.GREY_230,
		BodyDisableColor = Color32.GREY_196,

		ContentColor = Color32.WHITE,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_196,
		CharSize = 28,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle TextArea = new() {
		BodyColor = Color32.CLEAR,
		BodyHighlightColor = Color32.WHITE,
		BodyPressColor = Color32.GREY_230,
		BodyDisableColor = Color32.GREY_196,

		ContentColor = Color32.WHITE,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_196,

		CharSize = 28,
		Alignment = Alignment.MidLeft,
		Wrap = WrapMode.WordWrap,
		Clip = true,
	};

	public static readonly GUIStyle CenterLabel = new() {
		BodyColor = Color32.CLEAR,
		BodyHighlightColor = Color32.WHITE,
		BodyPressColor = Color32.GREY_230,
		BodyDisableColor = Color32.GREY_196,

		ContentColor = Color32.WHITE,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_196,
		CharSize = 28,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle CenterMiniLabel = new() {
		BodyColor = Color32.CLEAR,
		BodyHighlightColor = Color32.WHITE,
		BodyPressColor = Color32.GREY_230,
		BodyDisableColor = Color32.GREY_196,

		ContentColor = Color32.WHITE,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_196,
		CharSize = 14,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle InputField = new() {
		BodySprite = BuiltInSprite.UI_INPUT_FIELD,
		BodyHighlightSprite = BuiltInSprite.UI_INPUT_FIELD,
		BodyPressSprite = BuiltInSprite.UI_INPUT_FIELD,
		BodyDisableSprite = BuiltInSprite.UI_INPUT_FIELD,
		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),
		Alignment = Alignment.MidLeft,

		ContentColor = Color32.WHITE,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_196,
	};

	// Scroll
	public static readonly GUIStyle Scrollbar = new() {
		BodySprite = BuiltInSprite.UI_SCROLL_BAR,
		BodyHighlightSprite = BuiltInSprite.UI_SCROLL_BAR,
		BodyPressSprite = BuiltInSprite.UI_SCROLL_BAR_DOWN,
		BodyDisableSprite = BuiltInSprite.UI_SCROLL_BAR,
	};

}

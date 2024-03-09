using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Framework;

public static class GUISkin {


	private static readonly Color32 LABEL_CONTENT_TINT = Color32.GREY_245;

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

	public static readonly GUIStyle IconButton = new() {
		BodySprite = 0,
		BodyHighlightSprite = Const.PIXEL,
		BodyPressSprite = Const.PIXEL,
		BodyDisableSprite = 0,

		BodyColor = Color32.CLEAR,
		BodyHighlightColor = Color32.GREY_20,
		BodyPressColor = Color32.GREY_12,
		BodyDisableColor = Color32.CLEAR,

		ContentColor = Color32.WHITE,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.CLEAR,
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

	public static readonly GUIStyle DarkMiniButton = new() {

		BodySprite = BuiltInSprite.UI_MINI_BUTTON_DARK,
		BodyHighlightSprite = BuiltInSprite.UI_MINI_BUTTON_DARK,
		BodyPressSprite = BuiltInSprite.UI_MINI_BUTTON_DARK_DOWN,
		BodyDisableSprite = BuiltInSprite.UI_MINI_BUTTON_DARK,

		ContentShift = new(0, Const.ART_SCALE),
		ContentDisableShift = new(0, Const.ART_SCALE),
		ContentHighlightShift = new(0, Const.ART_SCALE),
		ContentBorder = Int4.Direction(Const.ART_SCALE, Const.ART_SCALE, Const.ART_SCALE, Const.ART_SCALE),

		ContentColor = Color32.WHITE,
		ContentHighlightColor = Color32.WHITE,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_196,

		CharSize = -1,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle LabelButton = new() {
		BodySprite = 0,
		BodyHighlightSprite = Const.PIXEL,
		BodyPressSprite = Const.PIXEL,
		BodyDisableSprite = 0,

		BodyColor = Color32.CLEAR,
		BodyHighlightColor = Color32.GREY_20,
		BodyPressColor = Color32.CLEAR,
		BodyDisableColor = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentHighlightColor = Color32.GREY_230,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_230,

		CharSize = 28,
		Alignment = Alignment.MidLeft,
	};
	public static readonly GUIStyle MediumLabelButton = new() {
		BodySprite = 0,
		BodyHighlightSprite = Const.PIXEL,
		BodyPressSprite = Const.PIXEL,
		BodyDisableSprite = 0,

		BodyColor = Color32.CLEAR,
		BodyHighlightColor = Color32.GREY_20,
		BodyPressColor = Color32.CLEAR,
		BodyDisableColor = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentHighlightColor = Color32.GREY_230,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_230,

		CharSize = 21,
		Alignment = Alignment.MidLeft,
	};
	public static readonly GUIStyle MiniLabelButton = new() {
		BodySprite = 0,
		BodyHighlightSprite = Const.PIXEL,
		BodyPressSprite = Const.PIXEL,
		BodyDisableSprite = 0,

		BodyColor = Color32.CLEAR,
		BodyHighlightColor = Color32.GREY_20,
		BodyPressColor = Color32.CLEAR,
		BodyDisableColor = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentHighlightColor = Color32.GREY_230,
		ContentPressColor = Color32.GREY_230,
		ContentDisableColor = Color32.GREY_230,

		CharSize = 14,
		Alignment = Alignment.MidLeft,
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
	public static readonly GUIStyle LargeLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
		ContentDisableColor = Color32.GREY_196,

		CharSize = 28,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle Label = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
		ContentDisableColor = Color32.GREY_196,

		CharSize = 21,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle SmallLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
		ContentDisableColor = Color32.GREY_196,

		CharSize = 14,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle CenterLargeLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
		ContentDisableColor = Color32.GREY_196,
		CharSize = 28,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle CenterLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
		ContentDisableColor = Color32.GREY_196,
		CharSize = 21,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle CenterSmallLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
		ContentDisableColor = Color32.GREY_196,
		CharSize = 14,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle LargeTextArea = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
		ContentDisableColor = Color32.GREY_196,

		CharSize = 28,
		Alignment = Alignment.TopLeft,
		Wrap = WrapMode.WordWrap,
		Clip = true,
	};

	public static readonly GUIStyle TextArea = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
		ContentDisableColor = Color32.GREY_196,

		CharSize = 21,
		Alignment = Alignment.TopLeft,
		Wrap = WrapMode.WordWrap,
		Clip = true,
	};

	public static readonly GUIStyle SmallTextArea = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
		ContentDisableColor = Color32.GREY_196,

		CharSize = 14,
		Alignment = Alignment.TopLeft,
		Wrap = WrapMode.WordWrap,
		Clip = true,
	};

	public static readonly GUIStyle InputField = new() {

		BodySprite = BuiltInSprite.UI_INPUT_FIELD,
		BodyHighlightSprite = BuiltInSprite.UI_INPUT_FIELD,
		BodyPressSprite = BuiltInSprite.UI_INPUT_FIELD,
		BodyDisableSprite = BuiltInSprite.UI_INPUT_FIELD,

		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),
		Alignment = Alignment.MidLeft,

		ContentColor = LABEL_CONTENT_TINT,
		ContentHighlightColor = LABEL_CONTENT_TINT,
		ContentPressColor = LABEL_CONTENT_TINT,
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

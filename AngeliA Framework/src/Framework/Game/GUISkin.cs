using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Framework;

public static class GUISkin {


	private static readonly Color32 LABEL_CONTENT_TINT = Color32.GREY_245;
	private static readonly Color32 LABEL_GREY_CONTENT_TINT = Color32.GREY_128;
	private static readonly Color32 LABEL_DOWN_TINT = Color32.GREY_216;

	public static readonly GUIStyle None = new() {
		BodySprite = 0,
		BodySpriteHover = 0,
		BodySpriteDown = 0,
		BodySpriteDisable = 0,
	};

	public static readonly GUIStyle HighlightPixel = new() {
		BodySprite = Const.PIXEL,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = Const.PIXEL,
		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.WHITE_12,
		BodyColorDown = Color32.WHITE_20,
		BodyColorDisable = Color32.CLEAR,
	};

	public static readonly GUIStyle GreenPixel = new() {
		BodySprite = Const.PIXEL,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = Const.PIXEL,
		BodyColor = Color32.GREEN,
		BodyColorHover = Color32.GREEN,
		BodyColorDown = Color32.GREEN,
		BodyColorDisable = Color32.GREEN,
	};

	public static readonly GUIStyle ItemFrame = new() {
		BodySprite = BuiltInSprite.UI_ITEM_FRAME,
		BodySpriteHover = BuiltInSprite.UI_ITEM_FRAME,
		BodySpriteDown = BuiltInSprite.UI_ITEM_FRAME,
		BodySpriteDisable = BuiltInSprite.UI_ITEM_FRAME,

		ContentColorDisable = Color32.WHITE_128,
		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),
	};


	// Button
	public static readonly GUIStyle Button = new() {
		BodySprite = BuiltInSprite.UI_BUTTON,
		BodySpriteHover = BuiltInSprite.UI_BUTTON_HOVER,
		BodySpriteDown = BuiltInSprite.UI_BUTTON_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_BUTTON,
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentShiftDisable = new(0, Const.ART_SCALE * 2),
		ContentShiftHover = new(0, Const.ART_SCALE * 2),
		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),

		ContentColor = Color32.WHITE,
		ContentColorHover = Color32.WHITE,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_196,
		CharSize = -1,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle IconButton = new() {
		BodySprite = 0,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = 0,

		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.GREY_20,
		BodyColorDown = Color32.GREY_12,
		BodyColorDisable = Color32.CLEAR,

		ContentColor = Color32.WHITE,
		ContentColorHover = Color32.WHITE,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.CLEAR,
	};

	public static readonly GUIStyle DarkButton = new() {
		BodySprite = BuiltInSprite.UI_DARK_BUTTON,
		BodySpriteHover = BuiltInSprite.UI_DARK_BUTTON_HOVER,
		BodySpriteDown = BuiltInSprite.UI_DARK_BUTTON_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_DARK_BUTTON,
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentShiftDisable = new(0, Const.ART_SCALE * 2),
		ContentShiftHover = new(0, Const.ART_SCALE * 2),
		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),

		ContentColor = Color32.WHITE,
		ContentColorHover = Color32.WHITE,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_196,
		CharSize = -1,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle SmallDarkButton = new() {

		BodySprite = BuiltInSprite.UI_MINI_BUTTON_DARK,
		BodySpriteHover = BuiltInSprite.UI_MINI_BUTTON_DARK_HOVER,
		BodySpriteDown = BuiltInSprite.UI_MINI_BUTTON_DARK_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_MINI_BUTTON_DARK,

		ContentShift = new(0, Const.ART_SCALE),
		ContentShiftDisable = new(0, Const.ART_SCALE),
		ContentShiftHover = new(0, Const.ART_SCALE),
		ContentBorder = Int4.Direction(Const.ART_SCALE, Const.ART_SCALE, Const.ART_SCALE, Const.ART_SCALE),

		ContentColor = Color32.WHITE,
		ContentColorHover = Color32.WHITE,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_196,

		CharSize = -1,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle LargeLabelButton = new() {
		BodySprite = 0,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = 0,

		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.GREY_20,
		BodyColorDown = Color32.CLEAR,
		BodyColorDisable = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentColorHover = Color32.GREY_230,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_230,

		CharSize = 28,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle LabelButton = new() {
		BodySprite = 0,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = 0,

		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.GREY_20,
		BodyColorDown = Color32.CLEAR,
		BodyColorDisable = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentColorHover = Color32.GREY_230,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_230,

		CharSize = 21,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle SmallLabelButton = new() {
		BodySprite = 0,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = 0,

		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.GREY_20,
		BodyColorDown = Color32.CLEAR,
		BodyColorDisable = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentColorHover = Color32.GREY_230,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_230,

		CharSize = 14,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle LargeCenterLabelButton = new() {
		BodySprite = 0,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = 0,

		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.GREY_20,
		BodyColorDown = Color32.CLEAR,
		BodyColorDisable = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentColorHover = Color32.GREY_230,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_230,

		CharSize = 28,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle CenterLabelButton = new() {
		BodySprite = 0,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = 0,

		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.GREY_20,
		BodyColorDown = Color32.CLEAR,
		BodyColorDisable = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentColorHover = Color32.GREY_230,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_230,

		CharSize = 21,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle SmallCenterLabelButton = new() {
		BodySprite = 0,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = 0,

		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.GREY_20,
		BodyColorDown = Color32.CLEAR,
		BodyColorDisable = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentColorHover = Color32.GREY_230,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_230,

		CharSize = 14,
		Alignment = Alignment.MidMid,
	};

	// Toggle
	public static readonly GUIStyle Toggle = new() {
		BodySprite = BuiltInSprite.UI_TOGGLE,
		BodySpriteHover = BuiltInSprite.UI_TOGGLE_HOVER,
		BodySpriteDown = BuiltInSprite.UI_TOGGLE_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_TOGGLE,
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentShiftDisable = new(0, Const.ART_SCALE * 2),
		ContentShiftHover = new(0, Const.ART_SCALE * 2),
		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),
	};

	public static readonly GUIStyle ToggleMark = new() {
		BodySprite = BuiltInSprite.UI_TOGGLE_MARK,
		BodySpriteHover = BuiltInSprite.UI_TOGGLE_MARK,
		BodySpriteDown = BuiltInSprite.UI_TOGGLE_MARK,
		BodySpriteDisable = BuiltInSprite.UI_TOGGLE_MARK,
		ContentShift = new(0, Const.ART_SCALE * 2),
		ContentShiftDisable = new(0, Const.ART_SCALE * 2),
		ContentShiftHover = new(0, Const.ART_SCALE * 2),
	};


	// Text
	public static readonly GUIStyle LargeLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 28,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle Label = new() {

		BodySprite = 0,
		BodySpriteHover = 0,
		BodySpriteDown = 0,
		BodySpriteDisable = 0,

		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.CLEAR,
		BodyColorDown = Color32.CLEAR,
		BodyColorDisable = Color32.CLEAR,

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 21,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle SmallLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 14,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle SmallGreyLabel = new() {

		ContentColor = LABEL_GREY_CONTENT_TINT,
		ContentColorHover = LABEL_GREY_CONTENT_TINT,
		ContentColorDown = LABEL_GREY_CONTENT_TINT,
		ContentColorDisable = Color32.GREY_32,

		CharSize = 14,
		Alignment = Alignment.MidLeft,
	};

	public static readonly GUIStyle CenterLargeLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,
		CharSize = 28,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle CenterLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 21,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle CenterSmallLabel = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,
		CharSize = 14,
		Alignment = Alignment.MidMid,
	};

	public static readonly GUIStyle LargeTextArea = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 28,
		Alignment = Alignment.TopLeft,
		Wrap = WrapMode.WordWrap,
		Clip = true,
	};

	public static readonly GUIStyle TextArea = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 21,
		Alignment = Alignment.TopLeft,
		Wrap = WrapMode.WordWrap,
		Clip = true,
	};

	public static readonly GUIStyle SmallTextArea = new() {

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 14,
		Alignment = Alignment.TopLeft,
		Wrap = WrapMode.WordWrap,
		Clip = true,
	};

	// Input Field
	public static readonly GUIStyle LargeInputField = new() {

		BodySprite = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteHover = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDown = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDisable = BuiltInSprite.UI_INPUT_FIELD,

		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),
		Alignment = Alignment.MidLeft,

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 28,
		Wrap = WrapMode.NoWrap,
	};
	public static readonly GUIStyle InputField = new() {

		BodySprite = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteHover = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDown = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDisable = BuiltInSprite.UI_INPUT_FIELD,

		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),
		Alignment = Alignment.MidLeft,

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 21,
		Wrap = WrapMode.NoWrap,
	};
	public static readonly GUIStyle SmallInputField = new() {

		BodySprite = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteHover = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDown = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDisable = BuiltInSprite.UI_INPUT_FIELD,

		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),
		Alignment = Alignment.MidLeft,

		ContentColor = LABEL_CONTENT_TINT,
		ContentColorHover = LABEL_CONTENT_TINT,
		ContentColorDown = LABEL_DOWN_TINT,
		ContentColorDisable = Color32.GREY_196,

		CharSize = 14,
		Wrap = WrapMode.NoWrap,
	};

	// Scroll
	public static readonly GUIStyle Scrollbar = new() {
		BodySprite = BuiltInSprite.UI_SCROLL_BAR,
		BodySpriteHover = BuiltInSprite.UI_SCROLL_BAR,
		BodySpriteDown = BuiltInSprite.UI_SCROLL_BAR_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_SCROLL_BAR,
	};

}

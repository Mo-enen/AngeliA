using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class GUISkin {


	public static readonly GUISkin Default = new() { Name = "Built-in" };
	public string Name = "";

	// Misc
	public readonly GUIStyle HighlightPixel = new() {
		BodySprite = Const.PIXEL,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = Const.PIXEL,
		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.WHITE_12,
		BodyColorDown = Color32.WHITE_20,
		BodyColorDisable = Color32.CLEAR,
	};

	public readonly GUIStyle WeakHighlightPixel = new() {
		BodySprite = Const.PIXEL,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = Const.PIXEL,
		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.WHITE_6,
		BodyColorDown = new Color32(255, 255, 255, 8),
		BodyColorDisable = Color32.CLEAR,
	};

	public readonly GUIStyle Frame = new() {
		BodySprite = BuiltInSprite.FRAME_16,
		BodySpriteHover = BuiltInSprite.FRAME_16,
		BodySpriteDown = BuiltInSprite.FRAME_16,
		BodySpriteDisable = BuiltInSprite.FRAME_16,
		BodyColor = Color32.GREY_20,
		BodyColorHover = Color32.GREY_20,
		BodyColorDown = Color32.GREY_20,
		BodyColorDisable = Color32.GREY_20,
		BodyBorder = Int4.Direction(2, 2, 2, 2),
	};

	public readonly GUIStyle WeakPixel = new() {
		BodySprite = Const.PIXEL,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = Const.PIXEL,
		BodyColor = new Color32(26, 26, 26, 255),
		BodyColorHover = new Color32(26, 26, 26, 255),
		BodyColorDown = Color32.GREY_20,
		BodyColorDisable = Color32.CLEAR,
	};

	public readonly GUIStyle GreenPixel = new() {
		BodySprite = Const.PIXEL,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = Const.PIXEL,
		BodyColor = Color32.GREEN,
		BodyColorHover = Color32.GREEN,
		BodyColorDown = Color32.GREEN,
		BodyColorDisable = Color32.GREEN,
	};

	public readonly GUIStyle DialogBG = new() {
		BodySprite = BuiltInSprite.UI_DIALOG_BG,
		BodySpriteHover = BuiltInSprite.UI_DIALOG_BG,
		BodySpriteDown = BuiltInSprite.UI_DIALOG_BG,
		BodySpriteDisable = BuiltInSprite.UI_DIALOG_BG,
	};

	public readonly GUIStyle ItemFrame = new() {
		BodySprite = BuiltInSprite.UI_ITEM_FRAME,
		BodySpriteHover = BuiltInSprite.UI_ITEM_FRAME,
		BodySpriteDown = BuiltInSprite.UI_ITEM_FRAME,
		BodySpriteDisable = BuiltInSprite.UI_ITEM_FRAME,

		ContentColorDisable = Color32.WHITE_128,
		ContentBorder = Int4.Direction(2, 2, 2, 2),
	};

	public readonly GUIStyle Scrollbar = new() {
		BodySprite = BuiltInSprite.UI_SCROLL_BAR,
		BodySpriteHover = BuiltInSprite.UI_SCROLL_BAR,
		BodySpriteDown = BuiltInSprite.UI_SCROLL_BAR,
		BodySpriteDisable = BuiltInSprite.UI_SCROLL_BAR,
		BodyColor = Color32.GREY_42,
		BodyColorHover = Color32.GREY_46,
		BodyColorDown = Color32.GREY_38,
		BodyColorDisable = Color32.CLEAR,
	};


	// Slider
	public readonly GUIStyle SliderHandle = new() {
		BodySprite = BuiltInSprite.UI_SLIDER_HANDLE,
		BodySpriteHover = BuiltInSprite.UI_SLIDER_HANDLE,
		BodySpriteDown = BuiltInSprite.UI_SLIDER_HANDLE,
		BodySpriteDisable = BuiltInSprite.UI_SLIDER_HANDLE,
		BodyColor = Color32.GREY_42,
		BodyColorHover = Color32.GREY_46,
		BodyColorDown = Color32.GREY_38,
		BodyColorDisable = Color32.CLEAR,
	};
	public readonly GUIStyle SliderBody = new() {
		BodySprite = BuiltInSprite.UI_SLIDER_BODY,
		BodySpriteHover = BuiltInSprite.UI_SLIDER_BODY,
		BodySpriteDown = BuiltInSprite.UI_SLIDER_BODY,
		BodySpriteDisable = BuiltInSprite.UI_SLIDER_BODY,
		BodyColor = Color32.GREY_96,
		BodyColorHover = Color32.GREY_96,
		BodyColorDown = Color32.GREY_96,
		BodyColorDisable = Color32.CLEAR,
	};
	public readonly GUIStyle SliderFill = new() {
		BodySprite = BuiltInSprite.UI_SLIDER_FILL,
		BodySpriteHover = BuiltInSprite.UI_SLIDER_FILL,
		BodySpriteDown = BuiltInSprite.UI_SLIDER_FILL,
		BodySpriteDisable = BuiltInSprite.UI_SLIDER_FILL,
		BodyColor = Color32.WHITE,
		BodyColorHover = Color32.WHITE,
		BodyColorDown = Color32.WHITE,
		BodyColorDisable = Color32.GREY_128,
	};

	// Toggle
	public readonly GUIStyle Toggle = new() {
		BodySprite = BuiltInSprite.UI_TOGGLE,
		BodySpriteHover = BuiltInSprite.UI_TOGGLE_HOVER,
		BodySpriteDown = BuiltInSprite.UI_TOGGLE_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_TOGGLE,
		ContentShift = new(0, 2),
		ContentShiftHover = new(0, 2),
		ContentShiftDisable = new(0, 2),
		ContentBorder = Int4.Direction(2, 2, 2, 2),
	};

	public readonly GUIStyle ToggleMark = new() {
		BodySprite = BuiltInSprite.UI_TOGGLE_MARK,
		BodySpriteHover = BuiltInSprite.UI_TOGGLE_MARK,
		BodySpriteDown = BuiltInSprite.UI_TOGGLE_MARK,
		BodySpriteDisable = BuiltInSprite.UI_TOGGLE_MARK,
	};


	// Input Field
	public readonly GUIStyle LargeInputField = new() {

		BodySprite = BuiltInSprite.UI_INPUT_FIELD_LARGE,
		BodySpriteHover = BuiltInSprite.UI_INPUT_FIELD_LARGE,
		BodySpriteDown = BuiltInSprite.UI_INPUT_FIELD_LARGE,
		BodySpriteDisable = BuiltInSprite.UI_INPUT_FIELD_LARGE,

		ContentBorder = Int4.one * 12,
		Alignment = Alignment.MidLeft,

		ContentColor = Color32.GREY_245,
		ContentColorHover = Color32.GREY_245,
		ContentColorDown = Color32.GREY_216,
		ContentColorDisable = Color32.GREY_245.WithNewA(128),

		CharSize = 28,
		Wrap = WrapMode.NoWrap,

	};
	public readonly GUIStyle InputField = new() {

		BodySprite = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteHover = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDown = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDisable = BuiltInSprite.UI_INPUT_FIELD,

		ContentBorder = Int4.one * 12,
		Alignment = Alignment.MidLeft,

		ContentColor = Color32.GREY_245,
		ContentColorHover = Color32.GREY_245,
		ContentColorDown = Color32.GREY_216,
		ContentColorDisable = Color32.GREY_245.WithNewA(128),

		CharSize = 21,
		Wrap = WrapMode.NoWrap,

	};
	public readonly GUIStyle SmallInputField = new() {

		BodySprite = BuiltInSprite.UI_INPUT_FIELD_SMALL,
		BodySpriteHover = BuiltInSprite.UI_INPUT_FIELD_SMALL,
		BodySpriteDown = BuiltInSprite.UI_INPUT_FIELD_SMALL,
		BodySpriteDisable = BuiltInSprite.UI_INPUT_FIELD_SMALL,

		ContentBorder = Int4.one * 12,
		Alignment = Alignment.MidLeft,

		ContentColor = Color32.GREY_245,
		ContentColorHover = Color32.GREY_245,
		ContentColorDown = Color32.GREY_216,
		ContentColorDisable = Color32.GREY_245.WithNewA(128),

		CharSize = 14,
		Wrap = WrapMode.NoWrap,


	};

}

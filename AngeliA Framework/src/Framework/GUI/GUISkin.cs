using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework;

public static partial class GUISkin {


	// Misc
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

	public static readonly GUIStyle Scrollbar = new() {
		BodySprite = BuiltInSprite.UI_SCROLL_BAR,
		BodySpriteHover = BuiltInSprite.UI_SCROLL_BAR,
		BodySpriteDown = BuiltInSprite.UI_SCROLL_BAR_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_SCROLL_BAR,
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


	// Input Field
	public static readonly GUIStyle LargeInputField = new() {

		BodySprite = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteHover = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDown = BuiltInSprite.UI_INPUT_FIELD,
		BodySpriteDisable = BuiltInSprite.UI_INPUT_FIELD,

		ContentBorder = Int4.Direction(Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2, Const.ART_SCALE * 2),
		Alignment = Alignment.MidLeft,

		ContentColor = Color32.GREY_245,
		ContentColorHover = Color32.GREY_245,
		ContentColorDown = Color32.GREY_216,
		ContentColorDisable = Color32.GREY_245.WithNewA(128),

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

		ContentColor = Color32.GREY_245,
		ContentColorHover = Color32.GREY_245,
		ContentColorDown = Color32.GREY_216,
		ContentColorDisable = Color32.GREY_245.WithNewA(128),

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

		ContentColor = Color32.GREY_245,
		ContentColorHover = Color32.GREY_245,
		ContentColorDown = Color32.GREY_216,
		ContentColorDisable = Color32.GREY_245.WithNewA(128),

		CharSize = 14,
		Wrap = WrapMode.NoWrap,
	};

}

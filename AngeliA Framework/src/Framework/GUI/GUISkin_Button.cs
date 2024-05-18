using System;
using System.Collections.Generic;

namespace AngeliA;

public partial class GUISkin {


	public readonly GUIStyle Button = new() {
		BodySprite = BuiltInSprite.UI_BUTTON,
		BodySpriteHover = BuiltInSprite.UI_BUTTON_HOVER,
		BodySpriteDown = BuiltInSprite.UI_BUTTON_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_BUTTON,
		ContentShift = new(0, 2),
		ContentShiftDisable = new(0, 2),
		ContentShiftHover = new(0, 2),
		ContentBorder = Int4.Direction(2, 2, 2, 2),

		ContentColor = Color32.GREY_32,
		ContentColorHover = Color32.GREY_32,
		ContentColorDown = Color32.GREY_32,
		ContentColorDisable = Color32.GREY_64,
		CharSize = -1,
		Alignment = Alignment.MidMid,
	};

	public readonly GUIStyle IconButton = new() {
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
		ContentColorDisable = Color32.WHITE_128,

		ContentBorder = Int4.Direction(3, 3, 3, 3),
	};
	public readonly GUIStyle SmallIconButton = new() {
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

		ContentBorder = Int4.Direction(1, 1, 1, 1),
	};

	public readonly GUIStyle DarkButton = new() {
		BodySprite = BuiltInSprite.UI_DARK_BUTTON,
		BodySpriteHover = BuiltInSprite.UI_DARK_BUTTON_HOVER,
		BodySpriteDown = BuiltInSprite.UI_DARK_BUTTON_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_DARK_BUTTON,
		ContentShift = new(0, 2),
		ContentShiftDisable = new(0, 2),
		ContentShiftHover = new(0, 2),
		ContentBorder = Int4.Direction(2, 2, 2, 2),

		ContentColor = Color32.WHITE,
		ContentColorHover = Color32.WHITE,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_196,
		CharSize = -1,
		Alignment = Alignment.MidMid,
	};

	public readonly GUIStyle SmallDarkButton = new() {

		BodySprite = BuiltInSprite.UI_MINI_BUTTON_DARK,
		BodySpriteHover = BuiltInSprite.UI_MINI_BUTTON_DARK_HOVER,
		BodySpriteDown = BuiltInSprite.UI_MINI_BUTTON_DARK_DOWN,
		BodySpriteDisable = BuiltInSprite.UI_MINI_BUTTON_DARK,

		ContentShift = new(0, 1),
		ContentShiftDisable = new(0, 1),
		ContentShiftHover = new(0, 1),
		ContentBorder = Int4.Direction(1, 1, 1, 1),

		ContentColor = Color32.WHITE,
		ContentColorHover = Color32.WHITE,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_196,

		CharSize = -1,
		Alignment = Alignment.MidMid,
	};

	public readonly GUIStyle LargeLabelButton = new() {
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

	public readonly GUIStyle LabelButton = new() {
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

	public readonly GUIStyle SmallLabelButton = new() {
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

	public readonly GUIStyle LargeCenterLabelButton = new() {
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

	public readonly GUIStyle CenterLabelButton = new() {
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

	public readonly GUIStyle SmallCenterLabelButton = new() {
		BodySprite = 0,
		BodySpriteHover = Const.PIXEL,
		BodySpriteDown = Const.PIXEL,
		BodySpriteDisable = 0,

		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.GREY_20,
		BodyColorDown = Color32.CLEAR,
		BodyColorDisable = Color32.CLEAR,

		ContentColor = Color32.GREY_230,
		ContentColorHover = Color32.WHITE,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_230,

		CharSize = 14,
		Alignment = Alignment.MidMid,
	};

}
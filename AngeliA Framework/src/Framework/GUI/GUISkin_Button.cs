using System;
using System.Collections.Generic;

namespace AngeliA;

public partial class GUISkin {


	public readonly GUIStyle Button = new() {
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
		ContentColorDisable = Color32.CLEAR,

		ContentBorder = Int4.Direction(Const.ART_SCALE * 3, Const.ART_SCALE * 3, Const.ART_SCALE * 3, Const.ART_SCALE * 3),
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

		ContentBorder = Int4.Direction(Const.ART_SCALE, Const.ART_SCALE, Const.ART_SCALE, Const.ART_SCALE),
	};

	public readonly GUIStyle DarkButton = new() {
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

	public readonly GUIStyle SmallDarkButton = new() {

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
		ContentColorHover = Color32.GREY_230,
		ContentColorDown = Color32.GREY_230,
		ContentColorDisable = Color32.GREY_230,

		CharSize = 14,
		Alignment = Alignment.MidMid,
	};

}
using System.Collections;
using System.Collections.Generic;

namespace AngeliA.Framework;

public sealed class GUIStyle {

	// Body
	public int BodySprite = Const.PIXEL;
	public int BodySpriteHover = Const.PIXEL;
	public int BodySpriteDown = Const.PIXEL;
	public int BodySpriteDisable = Const.PIXEL;
	public Color32 BodyColor = Color32.WHITE;
	public Color32 BodyColorHover = Color32.WHITE;
	public Color32 BodyColorDown = Color32.WHITE;
	public Color32 BodyColorDisable = Color32.WHITE;
	public Int4? BodyBorder = null;

	// Content
	public Color32 ContentColor = Color32.WHITE;
	public Color32 ContentColorHover = Color32.WHITE;
	public Color32 ContentColorDown = Color32.WHITE;
	public Color32 ContentColorDisable = Color32.WHITE;
	public Int2 ContentShift = Int2.zero;
	public Int2 ContentShiftHover = Int2.zero;
	public Int2 ContentShiftDown = Int2.zero;
	public Int2 ContentShiftDisable = Int2.zero;
	public Int4? ContentBorder = null;

	// Text
	public int CharSize = -1;
	public int CharSpace = 0;
	public int LineSpace = 5;
	public Alignment Alignment = Alignment.MidMid;
	public WrapMode Wrap = WrapMode.NoWrap;
	public bool Clip = true;

	// API
	public GUIStyle (GUIStyle source = null) {
		if (source == null) return;
		BodySprite = source.BodySprite;
		BodySpriteHover = source.BodySpriteHover;
		BodySpriteDown = source.BodySpriteDown;
		BodySpriteDisable = source.BodySpriteDisable;
		BodyBorder = source.BodyBorder;
		BodyColor = source.BodyColor;
		BodyColorHover = source.BodyColorHover;
		BodyColorDown = source.BodyColorDown;
		BodyColorDisable = source.BodyColorDisable;
		ContentBorder = source.ContentBorder;
		ContentColor = source.ContentColor;
		ContentColorHover = source.ContentColorHover;
		ContentColorDown = source.ContentColorDown;
		ContentColorDisable = source.ContentColorDisable;
		ContentShift = source.ContentShift;
		ContentShiftHover = source.ContentShiftHover;
		ContentShiftDown = source.ContentShiftDown;
		ContentShiftDisable = source.ContentShiftDisable;
		CharSize = source.CharSize;
		CharSpace = source.CharSpace;
		LineSpace = source.LineSpace;
		Alignment = source.Alignment;
		Wrap = source.Wrap;
		Clip = source.Clip;
	}
	public Int2 GetContentShift (GUIState state) => state switch {
		GUIState.Normal => ContentShift,
		GUIState.Hover => ContentShiftHover,
		GUIState.Press => ContentShiftDown,
		GUIState.Disable => ContentShiftDisable,
		_ => ContentShift,
	};
	public Color32 GetContentColor (GUIState state) => state switch {
		GUIState.Normal => ContentColor,
		GUIState.Hover => ContentColorHover,
		GUIState.Press => ContentColorDown,
		GUIState.Disable => ContentColorDisable,
		_ => ContentColor,
	};
	public Color32 GetBodyColor (GUIState state) => state switch {
		GUIState.Normal => BodyColor,
		GUIState.Hover => BodyColorHover,
		GUIState.Press => BodyColorDown,
		GUIState.Disable => BodyColorDisable,
		_ => BodyColor,
	};
	public int GetBodySprite (GUIState state) => state switch {
		GUIState.Normal => BodySprite,
		GUIState.Hover => BodySpriteHover,
		GUIState.Press => BodySpriteDown,
		GUIState.Disable => BodySpriteDisable,
		_ => BodySprite,
	};

}

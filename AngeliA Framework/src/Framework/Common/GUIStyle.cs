using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public sealed class GUIStyle {

	// Body
	public int BodySprite = Const.PIXEL;
	public int BodyHighlightSprite = Const.PIXEL;
	public int BodyPressSprite = Const.PIXEL;
	public int BodyDisableSprite = Const.PIXEL;
	public Int4? BodyBorder = null;
	public Color32 BodyColor = Color32.WHITE;
	public Color32 BodyHighlightColor = Color32.WHITE;
	public Color32 BodyPressColor = Color32.WHITE;
	public Color32 BodyDisableColor = Color32.WHITE;

	// Content
	public Int4? ContentBorder = null;
	public Color32 ContentColor = Color32.WHITE;

	// Text
	public int CharSize = -1;
	public Alignment Alignment = Alignment.MidMid;
	public WrapMode Wrap = WrapMode.NoWrap;
	public bool Clip = true;

}

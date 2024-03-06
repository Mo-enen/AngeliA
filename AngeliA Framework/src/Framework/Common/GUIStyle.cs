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
	public Color32 ContentHighlightColor = Color32.WHITE;
	public Color32 ContentPressColor = Color32.WHITE;
	public Color32 ContentDisableColor = Color32.WHITE;
	public Int2 ContentShift = Int2.zero;
	public Int2 ContentHighlightShift = Int2.zero;
	public Int2 ContentPressShift = Int2.zero;
	public Int2 ContentDisableShift = Int2.zero;

	// Text
	public int CharSize = -1;
	public Alignment Alignment = Alignment.MidMid;
	public WrapMode Wrap = WrapMode.NoWrap;
	public bool Clip = true;

	// API
	public Int2 GetContentShift (GUIState state) => state switch {
		GUIState.Normal => ContentShift,
		GUIState.Hover => ContentHighlightShift,
		GUIState.Press => ContentPressShift,
		GUIState.Disable => ContentDisableShift,
		_ => ContentShift,
	};
	public Color32 GetContentColor (GUIState state) => state switch {
		GUIState.Normal => ContentColor,
		GUIState.Hover => ContentHighlightColor,
		GUIState.Press => ContentPressColor,
		GUIState.Disable => ContentDisableColor,
		_ => ContentColor,
	};
	public Color32 GetBodyColor (GUIState state) => state switch {
		GUIState.Normal => BodyColor,
		GUIState.Hover => BodyHighlightColor,
		GUIState.Press => BodyPressColor,
		GUIState.Disable => BodyDisableColor,
		_ => BodyColor,
	};
	public int GetBodySprite (GUIState state) => state switch {
		GUIState.Normal => BodySprite,
		GUIState.Hover => BodyHighlightSprite,
		GUIState.Press => BodyPressSprite,
		GUIState.Disable => BodyDisableSprite,
		_ => BodySprite,
	};

}

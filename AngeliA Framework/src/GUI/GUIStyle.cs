using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public sealed class GUIStyle {

	public static readonly GUIStyle None = new() {
		BodySprite = 0,
		BodySpriteHover = 0,
		BodySpriteDown = 0,
		BodySpriteDisable = 0,
		BodyColor = Color32.CLEAR,
		BodyColorHover = Color32.CLEAR,
		BodyColorDown = Color32.CLEAR,
		BodyColorDisable = Color32.CLEAR,
		ContentColor = Color32.CLEAR,
		ContentColorHover = Color32.CLEAR,
		ContentColorDown = Color32.CLEAR,
		ContentColorDisable = Color32.CLEAR,
	};

	// Body
	public int BodySprite = Const.PIXEL;
	public int BodySpriteHover = Const.PIXEL;
	public int BodySpriteDown = Const.PIXEL;
	public int BodySpriteDisable = Const.PIXEL;
	public Color32 BodyColor = Color32.WHITE;
	public Color32 BodyColorHover = Color32.WHITE;
	public Color32 BodyColorDown = Color32.WHITE;
	public Color32 BodyColorDisable = Color32.WHITE;

	// Content
	public Color32 ContentColor = Color32.WHITE;
	public Color32 ContentColorHover = Color32.WHITE;
	public Color32 ContentColorDown = Color32.WHITE;
	public Color32 ContentColorDisable = Color32.WHITE;

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
		BodyColor = source.BodyColor;
		BodyColorHover = source.BodyColorHover;
		BodyColorDown = source.BodyColorDown;
		BodyColorDisable = source.BodyColorDisable;
		ContentColor = source.ContentColor;
		ContentColorHover = source.ContentColorHover;
		ContentColorDown = source.ContentColorDown;
		ContentColorDisable = source.ContentColorDisable;
		CharSize = source.CharSize;
		CharSpace = source.CharSpace;
		LineSpace = source.LineSpace;
		Alignment = source.Alignment;
		Wrap = source.Wrap;
		Clip = source.Clip;
	}

	public Int4 GetBodyBorder (GUIState state) {
		int id = GetBodySprite(state);
		if (!Renderer.TryGetSprite(id, out var sprite, true)) return default;
		var border = sprite.GlobalBorder;
		border.left /= 10;
		border.right /= 10;
		border.down /= 10;
		border.up /= 10;
		return border;
	}

	public Int4 GetContentBorder (GUIState state) {
		int id = GetBodySprite(state);
		if (!Renderer.TryGetSprite(id, out var sprite, true)) return default;
		var border = sprite.GlobalBorder;
		border.left /= 10;
		border.right /= 10;
		border.down /= 10;
		border.up /= 10;
		return border;
	}

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

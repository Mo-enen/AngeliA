using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public class CharSprite {
	public char Char;
	public FRect Offset;
	public float Advance;
}


public class TextContent {

	public static readonly TextContent Empty = new();
	private static readonly TextContent Temp = new();

	public string Text;
	public char[] Chars;
	public bool FromString;
	public Color32 Tint;
	public Color32 BackgroundTint;
	public Alignment Alignment;
	public WrapMode Wrap;
	public int CharSize;
	public int CharSpace;
	public int LineSpace;
	public int ShadowOffset;
	public bool Clip;
	public int BackgroundPadding;
	public Color32 Shadow;

	public TextContent (string text = "") {
		Text = text;
		Chars = null;
		FromString = true;
		Tint = Color32.WHITE;
		BackgroundTint = Color32.CLEAR;
		Alignment = Alignment.MidMid;
		CharSize = 24;
		CharSpace = 0;
		LineSpace = 5;
		Wrap = WrapMode.NoWrap;
		Clip = false;
		BackgroundPadding = -1;
		Shadow = Color32.CLEAR;
	}

	public TextContent SetText (string newText) {
		Text = newText;
		Chars = null;
		FromString = true;
		return this;
	}

	public TextContent SetText (string newText, int charSize) {
		Text = newText;
		Chars = null;
		CharSize = charSize;
		FromString = true;
		return this;
	}

	public static TextContent Get (string text, int charSize = 24, Alignment alignment = Alignment.MidMid, WrapMode wrap = WrapMode.NoWrap, bool clip = false) {
		Temp.CharSize = charSize;
		Temp.Text = text;
		Temp.Chars = null;
		Temp.Alignment = alignment;
		Temp.Tint = Color32.WHITE;
		Temp.Wrap = wrap;
		Temp.Clip = clip;
		Temp.FromString = true;
		Temp.BackgroundTint = default;
		Temp.Shadow = Color32.CLEAR;
		return Temp;
	}

	public static TextContent Get (string text, Color32 tint, int charSize = 24, Alignment alignment = Alignment.MidMid, WrapMode wrap = WrapMode.NoWrap, bool clip = false) {
		Temp.CharSize = charSize;
		Temp.Text = text;
		Temp.Chars = null;
		Temp.Alignment = alignment;
		Temp.Tint = tint;
		Temp.Wrap = wrap;
		Temp.Clip = clip;
		Temp.FromString = true;
		Temp.BackgroundTint = default;
		Temp.Shadow = Color32.CLEAR;
		return Temp;
	}

	public static TextContent Get (char[] chars, int charSize = 24, Alignment alignment = Alignment.MidMid, WrapMode wrap = WrapMode.NoWrap) {
		Temp.CharSize = charSize;
		Temp.Chars = chars;
		Temp.Alignment = alignment;
		Temp.Tint = Color32.WHITE;
		Temp.Wrap = wrap;
		Temp.Clip = false;
		Temp.FromString = false;
		Temp.BackgroundTint = default;
		Temp.Shadow = Color32.CLEAR;
		return Temp;
	}

	public static TextContent Get (char[] chars, Color32 tint, int charSize = 24, Alignment alignment = Alignment.MidMid, WrapMode wrap = WrapMode.NoWrap) {
		Temp.CharSize = charSize;
		Temp.Chars = chars;
		Temp.Alignment = alignment;
		Temp.Tint = tint;
		Temp.Wrap = wrap;
		Temp.Clip = false;
		Temp.FromString = false;
		Temp.BackgroundTint = default;
		Temp.Shadow = Color32.CLEAR;
		return Temp;
	}

	public static TextContent Get (string text, GUIStyle style) {
		Temp.CharSize = style.CharSize;
		Temp.Text = text;
		Temp.Chars = null;
		Temp.Alignment = style.Alignment;
		Temp.Tint = style.ContentColor;
		Temp.Wrap = style.Wrap;
		Temp.Clip = style.Clip;
		Temp.FromString = true;
		Temp.BackgroundTint = default;
		Temp.Shadow = Color32.CLEAR;
		return Temp;
	}

}

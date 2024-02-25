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
	public int CharSize;
	public int CharSpace;
	public int LineSpace;
	public int ShadowOffset;
	public bool Wrap;
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
		Wrap = false;
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

	public static TextContent Get (string text, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
		Temp.CharSize = charSize;
		Temp.Text = text;
		Temp.Chars = null;
		Temp.Alignment = alignment;
		Temp.Tint = Color32.WHITE;
		Temp.Wrap = wrap;
		Temp.FromString = true;
		Temp.BackgroundTint = default;
		Temp.Shadow = Color32.CLEAR;
		return Temp;
	}

	public static TextContent Get (string text, Color32 tint, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
		Temp.CharSize = charSize;
		Temp.Text = text;
		Temp.Chars = null;
		Temp.Alignment = alignment;
		Temp.Tint = tint;
		Temp.Wrap = wrap;
		Temp.FromString = true;
		Temp.BackgroundTint = default;
		Temp.Shadow = Color32.CLEAR;
		return Temp;
	}

	public static TextContent Get (char[] chars, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
		Temp.CharSize = charSize;
		Temp.Chars = chars;
		Temp.Alignment = alignment;
		Temp.Tint = Color32.WHITE;
		Temp.Wrap = wrap;
		Temp.FromString = false;
		Temp.BackgroundTint = default;
		Temp.Shadow = Color32.CLEAR;
		return Temp;
	}

	public static TextContent Get (char[] chars, Color32 tint, int charSize = 24, Alignment alignment = Alignment.MidMid, bool wrap = false) {
		Temp.CharSize = charSize;
		Temp.Chars = chars;
		Temp.Alignment = alignment;
		Temp.Tint = tint;
		Temp.Wrap = wrap;
		Temp.FromString = false;
		Temp.BackgroundTint = default;
		Temp.Shadow = Color32.CLEAR;
		return Temp;
	}

}

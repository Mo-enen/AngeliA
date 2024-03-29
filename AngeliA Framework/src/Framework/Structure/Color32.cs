using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace AngeliA;
[Serializable]
public struct Color32 : IFormattable, IComparable {

	public static readonly Color32 WHITE = new(255, 255, 255, 255);
	public static readonly Color32 WHITE_196 = new(255, 255, 255, 196);
	public static readonly Color32 WHITE_128 = new(255, 255, 255, 128);
	public static readonly Color32 WHITE_96 = new(255, 255, 255, 96);
	public static readonly Color32 WHITE_64 = new(255, 255, 255, 64);
	public static readonly Color32 WHITE_12 = new(255, 255, 255, 12);
	public static readonly Color32 WHITE_20 = new(255, 255, 255, 20);
	public static readonly Color32 WHITE_0 = new(255, 255, 255, 0);
	public static readonly Color32 ORANGE = new(255, 128, 0, 255);
	public static readonly Color32 ORANGE_BETTER = new(255, 220, 100, 255);
	public static readonly Color32 YELLOW = new(255, 255, 0, 255);
	public static readonly Color32 BLACK = new(0, 0, 0, 255);
	public static readonly Color32 BLACK_220 = new(0, 0, 0, 220);
	public static readonly Color32 BLACK_196 = new(0, 0, 0, 196);
	public static readonly Color32 BLACK_128 = new(0, 0, 0, 128);
	public static readonly Color32 BLACK_96 = new(0, 0, 0, 96);
	public static readonly Color32 BLACK_64 = new(0, 0, 0, 64);
	public static readonly Color32 BLACK_32 = new(0, 0, 0, 32);
	public static readonly Color32 BLACK_12 = new(0, 0, 0, 12);
	public static readonly Color32 RED = new(255, 0, 0, 255);
	public static readonly Color32 RED_BETTER = new(255, 64, 64, 255);
	public static readonly Color32 GREEN = new(0, 255, 0, 255);
	public static readonly Color32 GREEN_DARK = new(42, 128, 36, 255);
	public static readonly Color32 GREEN_BETTER = new(32, 200, 96, 255);
	public static readonly Color32 CYAN = new(0, 255, 255, 255);
	public static readonly Color32 CYAN_BETTER = new(32, 232, 255, 255);
	public static readonly Color32 BLUE = new(0, 0, 255, 255);
	public static readonly Color32 BLUE_BETTER = new(12, 24, 255, 255);
	public static readonly Color32 PURPLE = new(128, 0, 255, 255);
	public static readonly Color32 PINK = new(255, 0, 255, 255);
	public static readonly Color32 PURPLE_BETTER = new(176, 94, 196, 255);
	public static readonly Color32 CLEAR = new(0, 0, 0, 0);
	public static readonly Color32 GREY_245 = new(245, 245, 245, 255);
	public static readonly Color32 GREY_230 = new(230, 230, 230, 255);
	public static readonly Color32 GREY_216 = new(216, 216, 216, 255);
	public static readonly Color32 GREY_196 = new(196, 196, 196, 255);
	public static readonly Color32 GREY_160 = new(160, 160, 160, 255);
	public static readonly Color32 GREY_128 = new(128, 128, 128, 255);
	public static readonly Color32 GREY_112 = new(112, 112, 112, 255);
	public static readonly Color32 GREY_96 = new(96, 96, 96, 255);
	public static readonly Color32 GREY_64 = new(64, 64, 64, 255);
	public static readonly Color32 GREY_56 = new(56, 56, 56, 255);
	public static readonly Color32 GREY_42 = new(42, 42, 42, 255);
	public static readonly Color32 GREY_32 = new(32, 32, 32, 255);
	public static readonly Color32 GREY_20 = new(20, 20, 20, 255);
	public static readonly Color32 GREY_12 = new(12, 12, 12, 255);
	public static readonly Color32 SKIN_YELLOW = new(245, 217, 196, 255);

	public byte r;
	public byte g;
	public byte b;
	public byte a;

	public Color32 (byte r, byte g, byte b, byte a = 255) {
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public static Color32 Lerp (Color32 a, Color32 b, float t) {
		t = t.Clamp01();
		return new Color32((byte)((float)(int)a.r + (float)(b.r - a.r) * t), (byte)((float)(int)a.g + (float)(b.g - a.g) * t), (byte)((float)(int)a.b + (float)(b.b - a.b) * t), (byte)((float)(int)a.a + (float)(b.a - a.a) * t));
	}

	public static Color32 LerpUnclamped (Color32 a, Color32 b, float t) {
		return new Color32((byte)((float)(int)a.r + (float)(b.r - a.r) * t), (byte)((float)(int)a.g + (float)(b.g - a.g) * t), (byte)((float)(int)a.b + (float)(b.b - a.b) * t), (byte)((float)(int)a.a + (float)(b.a - a.a) * t));
	}

	public override readonly string ToString () {
		return ToString(null, null);
	}

	public readonly string ToString (string format) {
		return ToString(format, null);
	}

	public readonly string ToString (string format, IFormatProvider formatProvider) {
		formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
		return string.Format("RGBA({0}, {1}, {2}, {3})", r.ToString(format, formatProvider), g.ToString(format, formatProvider), b.ToString(format, formatProvider), a.ToString(format, formatProvider));
	}

	public readonly int CompareTo (object obj) {
		var other = (Color32)obj;
		return
			r != other.r ? r.CompareTo(other.r) :
			g != other.g ? g.CompareTo(other.g) :
			b != other.b ? b.CompareTo(other.b) :
			a.CompareTo(other.a);
	}

	public override readonly bool Equals (object obj) {
		if (obj is not Color32) return false;
		var other = (Color32)obj;
		return r == other.r && g == other.g && b == other.b && a == other.a;
	}

	public override readonly int GetHashCode () => r ^ (g << 2) ^ (b >> 2) ^ (a >> 1);

	public static bool operator == (Color32 a, Color32 b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;

	public static bool operator != (Color32 a, Color32 b) => a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;

	public static Color32 operator * (Color32 a, Color32 b) => new(
		(byte)(a.r * b.r / 255),
		(byte)(a.g * b.g / 255),
		(byte)(a.b * b.b / 255),
		(byte)(a.a * b.a / 255)
	);

}

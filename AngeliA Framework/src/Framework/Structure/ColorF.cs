using System.Collections.Generic;
using System.Collections;
using System;
using System.Globalization;

namespace AngeliA;

public struct ColorF {

	public float r;
	public float g;
	public float b;
	public float a;

	public ColorF (float r, float g, float b, float a = 1f) {
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public static ColorF Lerp (ColorF a, ColorF b, float t) {
		t = t.Clamp01();
		return new ColorF(
			a.r + (b.r - a.r) * t,
			a.g + (b.g - a.g) * t,
			a.b + (b.b - a.b) * t,
			a.a + (b.a - a.a) * t
		);
	}

	public static ColorF LerpUnclamped (ColorF a, ColorF b, float t) {
		return new ColorF(
			a.r + (b.r - a.r) * t,
			a.g + (b.g - a.g) * t,
			a.b + (b.b - a.b) * t,
			a.a + (b.a - a.a) * t
		);
	}

	public override readonly string ToString () => ToString(null, null);

	public readonly string ToString (string format) => ToString(format, null);

	public readonly string ToString (string format, IFormatProvider formatProvider) {
		formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
		return string.Format("RGBA({0}, {1}, {2}, {3})", r.ToString(format, formatProvider), g.ToString(format, formatProvider), b.ToString(format, formatProvider), a.ToString(format, formatProvider));
	}

	public readonly int CompareTo (object obj) {
		var other = (ColorF)obj;
		return
			r != other.r ? r.CompareTo(other.r) :
			g != other.g ? g.CompareTo(other.g) :
			b != other.b ? b.CompareTo(other.b) :
			a.CompareTo(other.a);
	}

	public override readonly bool Equals (object obj) {
		if (obj is not ColorF) return false;
		var other = (ColorF)obj;
		return r.Almost(other.r) && g.Almost(other.g) && b.Almost(other.b) && a.Almost(other.a);
	}

	public override readonly int GetHashCode () => r.GetHashCode() ^ (g.GetHashCode() << 2) ^ (b.GetHashCode() >> 2) ^ (a.GetHashCode() >> 1);

	public static bool operator == (ColorF a, ColorF b) => a.r.Almost(b.r) && a.g.Almost(b.g) && a.b.Almost(b.b) && a.a.Almost(b.a);

	public static bool operator != (ColorF a, ColorF b) => !a.r.Almost(b.r) || !a.g.Almost(b.g) || !a.b.Almost(b.b) || !a.a.Almost(b.a);

	public static ColorF operator * (ColorF a, ColorF b) => new(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);



}

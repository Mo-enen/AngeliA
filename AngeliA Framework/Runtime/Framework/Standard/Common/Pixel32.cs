using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;


namespace AngeliaFramework {
	[Serializable]
	public struct Pixel32 : IFormattable {

		public byte r;
		public byte g;
		public byte b;
		public byte a;

		public byte this[int index] {
			readonly get {
				return index switch {
					0 => r,
					1 => g,
					2 => b,
					3 => a,
					_ => throw new IndexOutOfRangeException("Invalid Color32 index(" + index + ")!"),
				};
			}
			set {
				switch (index) {
					case 0:
						r = value;
						break;
					case 1:
						g = value;
						break;
					case 2:
						b = value;
						break;
					case 3:
						a = value;
						break;
					default:
						throw new IndexOutOfRangeException("Invalid Color32 index(" + index + ")!");
				}
			}
		}

		public Pixel32 (byte r, byte g, byte b, byte a) {
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public static Pixel32 Lerp (Pixel32 a, Pixel32 b, float t) {
			t = t.Clamp01();
			return new Pixel32((byte)((float)(int)a.r + (float)(b.r - a.r) * t), (byte)((float)(int)a.g + (float)(b.g - a.g) * t), (byte)((float)(int)a.b + (float)(b.b - a.b) * t), (byte)((float)(int)a.a + (float)(b.a - a.a) * t));
		}

		public static Pixel32 LerpUnclamped (Pixel32 a, Pixel32 b, float t) {
			return new Pixel32((byte)((float)(int)a.r + (float)(b.r - a.r) * t), (byte)((float)(int)a.g + (float)(b.g - a.g) * t), (byte)((float)(int)a.b + (float)(b.b - a.b) * t), (byte)((float)(int)a.a + (float)(b.a - a.a) * t));
		}

		public override string ToString () {
			return ToString(null, null);
		}

		public string ToString (string format) {
			return ToString(format, null);
		}

		public string ToString (string format, IFormatProvider formatProvider) {
			formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
			return string.Format("RGBA({0}, {1}, {2}, {3})", r.ToString(format, formatProvider), g.ToString(format, formatProvider), b.ToString(format, formatProvider), a.ToString(format, formatProvider));
		}

#if UNITY_2017_1_OR_NEWER
		public static implicit operator Pixel32 (UnityEngine.Color c) {
			return new Pixel32((byte)Math.Round(c.r.Clamp01() * 255f), (byte)Math.Round(c.g.Clamp01() * 255f), (byte)Math.Round(c.b.Clamp01() * 255f), (byte)Math.Round(c.a.Clamp01() * 255f));
		}
		public static implicit operator UnityEngine.Color (Pixel32 c) {
			return new UnityEngine.Color((float)(int)c.r / 255f, (float)(int)c.g / 255f, (float)(int)c.b / 255f, (float)(int)c.a / 255f);
		}
		public static implicit operator Pixel32 (UnityEngine.Color32 c) => new(c.r, c.g, c.b, c.a);
		public static implicit operator UnityEngine.Color32 (Pixel32 c) => new(c.r, c.g, c.b, c.a);
#endif
	}
}

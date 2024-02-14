using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;


namespace AngeliA {
	[Serializable]
	public struct Byte4 : IFormattable, IComparable {

		public byte r {
			readonly get => x;
			set => x = value;
		}
		public byte g {
			readonly get => y;
			set => y = value;
		}
		public byte b {
			readonly get => z;
			set => z = value;
		}
		public byte a {
			readonly get => w;
			set => w = value;
		}

		public byte x;
		public byte y;
		public byte z;
		public byte w;

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

		public Byte4 (byte r, byte g, byte b, byte a) {
			x = r;
			y = g;
			z = b;
			w = a;
		}

		public static Byte4 Lerp (Byte4 a, Byte4 b, float t) {
			t = t.Clamp01();
			return new Byte4((byte)((float)(int)a.r + (float)(b.r - a.r) * t), (byte)((float)(int)a.g + (float)(b.g - a.g) * t), (byte)((float)(int)a.b + (float)(b.b - a.b) * t), (byte)((float)(int)a.a + (float)(b.a - a.a) * t));
		}

		public static Byte4 LerpUnclamped (Byte4 a, Byte4 b, float t) {
			return new Byte4((byte)((float)(int)a.r + (float)(b.r - a.r) * t), (byte)((float)(int)a.g + (float)(b.g - a.g) * t), (byte)((float)(int)a.b + (float)(b.b - a.b) * t), (byte)((float)(int)a.a + (float)(b.a - a.a) * t));
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

		public int CompareTo (object obj) {
			var other = (Byte4)obj;
			return
				x != other.x ? x.CompareTo(other.x) :
				y != other.y ? y.CompareTo(other.y) :
				z != other.z ? z.CompareTo(other.z) :
				w.CompareTo(other.w);
		}

		public override readonly bool Equals (object obj) {
			if (obj is not Byte4) return false;
			var b = (Byte4)obj;
			return x == b.x && y == b.y && z == b.z && w == b.w;
		}

		public override readonly int GetHashCode () => x ^ (y << 2) ^ (z >> 2) ^ (w >> 1);

		public static bool operator == (Byte4 a, Byte4 b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
		public static bool operator != (Byte4 a, Byte4 b) => a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;

	}
}

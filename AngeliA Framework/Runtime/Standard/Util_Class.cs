using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {



	[System.Serializable]
	public struct Vector4Int {

		public static readonly Vector4Int Zero = new(0, 0, 0, 0);
		public int this[int index] {
			get => index switch {
				0 => x,
				1 => y,
				2 => z,
				3 => w,
				_ => throw new System.ArgumentOutOfRangeException(),
			};
			set {
				switch (index) {
					case 0: x = value; break;
					case 1: y = value; break;
					case 2: z = value; break;
					case 3: w = value; break;
					default: throw new System.ArgumentOutOfRangeException();
				}
			}
		}
		public readonly bool IsZero => x == 0 && y == 0 && z == 0 && w == 0;
		public int left { readonly get => x; set => x = value; }
		public int right { readonly get => y; set => y = value; }
		public int down { readonly get => z; set => z = value; }
		public int up { readonly get => w; set => w = value; }
		public readonly int horizontal => left + right;
		public readonly int vertical => down + up;

		public int x;
		public int y;
		public int z;
		public int w;

		public Vector4Int (int x, int y, int z, int w) {
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public static Vector4Int operator * (int b, Vector4Int a) => a * b;
		public static bool operator == (Vector4Int a, Vector4Int b) => a.Equals(b);
		public static bool operator != (Vector4Int a, Vector4Int b) => !a.Equals(b);
		public static Vector4Int operator * (Vector4Int a, int b) {
			a.x *= b;
			a.y *= b;
			a.z *= b;
			a.w *= b;
			return a;
		}


		public override readonly bool Equals (object other) {
			if (other is not Vector4Int) return false;
			return Equals((Vector4Int)other);
		}
		public readonly bool Equals (Vector4Int other) => x == other.x && y == other.y && z == other.z && w == other.w;
		public override readonly int GetHashCode () => x ^ (y << 2) ^ (z >> 2) ^ (w >> 1);


		public readonly bool Contains (int value) => x == value || y == value || z == value || w == value;
		public bool Swap (int value, int newValue) {
			if (x == value) {
				x = newValue;
				return true;
			}
			if (y == value) {
				y = newValue;
				return true;
			}
			if (z == value) {
				z = newValue;
				return true;
			}
			if (w == value) {
				w = newValue;
				return true;
			}
			return false;
		}
		public readonly int Count (int value) {
			int count = 0;
			if (x == value) count++;
			if (y == value) count++;
			if (z == value) count++;
			if (w == value) count++;
			return count;
		}

	}



	public class IntToString {

		private int IntValue = 0;
		private string StringValue = "0";
		private readonly string Prefix = "";
		private readonly string Suffix = "";
		private readonly bool RequireFix = false;

		public IntToString (string prefix = "", string suffix = "") {
			Prefix = prefix;
			Suffix = suffix;
			StringValue = $"{prefix}0{suffix}";
			RequireFix = !string.IsNullOrEmpty(prefix) || !string.IsNullOrEmpty(suffix);
		}

		public string GetString (int value) {
			if (value != IntValue) {
				IntValue = value;
				if (RequireFix) {
					StringValue = $"{Prefix}{value}{Suffix}";
				} else {
					StringValue = value.ToString();
				}
			}
			return StringValue;
		}

	}



	// Enum
	public enum Direction2 {

		Negative = -1,
		Left = -1,
		Down = -1,
		Horizontal = -1,

		Positive = 1,
		Up = 1,
		Right = 1,
		Vertical = 1,
	}



	public enum Direction3 {
		None = 0,

		Negative = -1,
		Left = -1,
		Down = -1,
		Horizontal = -1,

		Positive = 1,
		Up = 1,
		Right = 1,
		Vertical = 1,
	}



	public enum Direction4 {
		Up = 0,
		Top = 0,

		Down = 1,
		Bottom = 1,

		Left = 2,

		Right = 3,
	}



	public enum Alignment {
		TopLeft = 0,
		TopMid = 1,
		TopRight = 2,
		MidLeft = 3,
		MidMid = 4,
		MidRight = 5,
		BottomLeft = 6,
		BottomMid = 7,
		BottomRight = 8,
		Full = 9,
	}



}
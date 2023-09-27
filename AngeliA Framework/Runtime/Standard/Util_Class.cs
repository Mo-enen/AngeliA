using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	[System.Serializable]
	public struct Int2 {

		public int x {
			get => A;
			set => A = value;
		}
		public int y {
			get => B;
			set => B = value;
		}

		public int A;
		public int B;

		public Int2 (int a, int b) {
			A = a;
			B = b;
		}

		public override bool Equals (object other) {
			if (other is not Int2) {
				return false;
			}

			return Equals((Int2)other);
		}
		public bool Equals (Int2 other) {
			return A == other.A && B == other.B;
		}
		public override int GetHashCode () {
			return A ^ (B << 2);
		}

		public static implicit operator Vector2Int (Int2 i) => new(i.A, i.B);
		public static implicit operator Int2 (Vector2Int v) => new(v.x, v.y);

	}


	[System.Serializable]
	public struct Int3 {

		public int x {
			get => A;
			set => A = value;
		}
		public int y {
			get => B;
			set => B = value;
		}
		public int z {
			get => C;
			set => C = value;
		}

		public int A;
		public int B;
		public int C;

		public Int3 (int a, int b, int c) {
			A = a;
			B = b;
			C = c;
		}

		public Vector3Int ToVectorInt3 () => new(A, B, C);

		public override bool Equals (object other) {
			if (other is not Int3) {
				return false;
			}

			return Equals((Int3)other);
		}
		public bool Equals (Int3 other) {
			return A == other.A && B == other.B && C == other.C;
		}
		public override int GetHashCode () {
			return A.GetHashCode() ^ (B << 4) ^ (B >> 28) ^ (C >> 4) ^ (C << 28);
		}


	}




	[System.Serializable]
	public struct Byte4 {

		public static readonly Byte4 Zero = new(0, 0, 0, 0);
		public bool IsZero => A == 0 && B == 0 && C == 0 && D == 0;
		public byte Left { get => A; set => A = value; }
		public byte Right { get => B; set => B = value; }
		public byte Down { get => C; set => C = value; }
		public byte Up { get => D; set => D = value; }

		public byte A;
		public byte B;
		public byte C;
		public byte D;

		public Byte4 (byte a, byte b, byte c, byte d) {
			A = a;
			B = b;
			C = c;
			D = d;
		}

	}



	[System.Serializable]
	public struct Int4 {

		public static readonly Int4 Zero = new(0, 0, 0, 0);
		public int this[int index] => index switch {
			0 => A,
			1 => B,
			2 => C,
			3 => D,
			_ => throw new System.ArgumentOutOfRangeException(),
		};
		public bool IsZero => A == 0 && B == 0 && C == 0 && D == 0;
		public int Left { get => A; set => A = value; }
		public int Right { get => B; set => B = value; }
		public int Down { get => C; set => C = value; }
		public int Up { get => D; set => D = value; }
		public int Horizontal => Left + Right;
		public int Vertical => Down + Up;

		public int A;
		public int B;
		public int C;
		public int D;

		public Int4 (int a, int b, int c, int d) {
			A = a;
			B = b;
			C = c;
			D = d;
		}

		public static Int4 operator * (int b, Int4 a) => a * b;
		public static bool operator == (Int4 a, Int4 b) => a.Equals(b);
		public static bool operator != (Int4 a, Int4 b) => !a.Equals(b);
		public static Int4 operator * (Int4 a, int b) {
			a.A *= b;
			a.B *= b;
			a.C *= b;
			a.D *= b;
			return a;
		}


		public override bool Equals (object other) {
			if (other is not Int4) return false;
			return Equals((Int4)other);
		}
		public bool Equals (Int4 other) => A == other.A && B == other.B && C == other.C && D == other.D;
		public override int GetHashCode () => A ^ (B << 2) ^ (C >> 2) ^ (D >> 1);


		public bool Contains (int value) => A == value || B == value || C == value || D == value;
		public bool Swap (int value, int newValue) {
			if (A == value) {
				A = newValue;
				return true;
			}
			if (B == value) {
				B = newValue;
				return true;
			}
			if (C == value) {
				C = newValue;
				return true;
			}
			if (D == value) {
				D = newValue;
				return true;
			}
			return false;
		}
		public int Count (int value) {
			int count = 0;
			if (A == value) count++;
			if (B == value) count++;
			if (C == value) count++;
			if (D == value) count++;
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
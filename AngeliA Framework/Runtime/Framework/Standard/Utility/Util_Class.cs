using System.Collections;
using System.Collections.Generic;
using System.Globalization;


namespace AngeliaFramework {


	public class IntToChars {

		private int IntValue = 0;
		public readonly char[] CharsValue = null;
		public readonly string Prefix = "";
		public readonly string Suffix = "";

		public IntToChars (string prefix = "", string suffix = "") {
			Prefix = prefix;
			Suffix = suffix;
			CharsValue = new char[prefix.Length + suffix.Length + 11];
			for (int i = 0; i < prefix.Length; i++) {
				CharsValue[i] = prefix[i];
			}
			CharsValue[prefix.Length] = '0';
			for (int i = 0; i < suffix.Length; i++) {
				CharsValue[prefix.Length + 1 + i] = suffix[i];
			}
			int endIndex = prefix.Length + suffix.Length + 1;
			if (endIndex < CharsValue.Length) {
				CharsValue[endIndex] = '\0';
			}
		}

		public char[] GetChars (int value) {
			if (value != IntValue) {
				IntValue = value;
				int digitCount = value.DigitCount();
				int startIndex = Prefix.Length;
				if (value < 0) {
					value = -value;
					CharsValue[startIndex] = '-';
					startIndex++;
				}
				// Fill Value
				for (int i = startIndex + digitCount - 1; i >= startIndex; i--) {
					CharsValue[i] = (char)((value % 10) + '0');
					value /= 10;
				}
				// Fill Suffix
				startIndex += digitCount;
				if (Suffix.Length > 0) {
					for (int i = 0; i < Suffix.Length; i++) {
						CharsValue[startIndex + i] = Suffix[i];
					}
				}
				// Fill End
				startIndex += Suffix.Length;
				if (startIndex < CharsValue.Length) {
					CharsValue[startIndex] = '\0';
				}
			}
			return CharsValue;
		}

	}



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
		Down = 1,
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
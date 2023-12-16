using System.Collections;
using System.Collections.Generic;
using System.Globalization;


namespace AngeliaFramework {


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
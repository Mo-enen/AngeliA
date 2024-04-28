namespace AngeliA;
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
		if (value == IntValue) return CharsValue;
		IntValue = value;
		// Fill Int Value
		int startIndex = Prefix.Length;
		Int_to_Chars(value, CharsValue, ref startIndex);
		// Fill Suffix
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
		return CharsValue;
	}

	public static void Int_to_Chars (int intValue, char[] charsValue, ref int startIndex) {
		int digitCount = intValue.DigitCount();
		if (intValue < 0) {
			intValue = -intValue;
			charsValue[startIndex] = '-';
			startIndex++;
		}
		// Fill Value
		for (int i = startIndex + digitCount - 1; i >= startIndex; i--) {
			charsValue[i] = (char)((intValue % 10) + '0');
			intValue /= 10;
		}
		startIndex += digitCount;
		if (startIndex < charsValue.Length) charsValue[startIndex] = '\0';
	}

	public static int Chars_to_Int (char[] charsValue, int startIndex = 0) {
		int result = 0;
		for (int i = startIndex; i < charsValue.Length; i++) {
			char c = charsValue[i];
			if (!char.IsNumber(c)) break;
			result = result * 10 + (c - '0');
		}
		return result;
	}

}
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
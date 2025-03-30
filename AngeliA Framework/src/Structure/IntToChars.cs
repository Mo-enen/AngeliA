namespace AngeliA;

/// <summary>
/// Get char array that holds the given intager as text content without creating heap pressure
/// </summary>
public class IntToChars {

	private int IntValue = 0;
	private readonly char[] CharsValue = null;
	private readonly string Prefix = "";
	private readonly string Suffix = "";

	/// <summary>
	/// Get char array that holds the given intager as text content without creating heap pressure 
	/// </summary>
	/// <param name="prefix">Label that add before the text content</param>
	/// <param name="suffix">Label that add after the text content</param>
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

	/// <summary>
	/// Calculate the char array based on the given intager
	/// </summary>
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

	/// <summary>
	/// Calculate char array based on the given intager
	/// </summary>
	/// <param name="intValue"></param>
	/// <param name="charsValue"></param>
	/// <param name="startIndex">Index of the next edit value</param>
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

	/// <summary>
	/// Get intager from the given char array
	/// </summary>
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
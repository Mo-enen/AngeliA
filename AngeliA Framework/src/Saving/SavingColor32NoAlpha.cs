namespace AngeliA;

public class SavingColor32NoAlpha (string key, Color32 defaultValue, SavingLocation location) : Saving<Color32>(key, defaultValue, location) {
	protected override string ValueToString (Color32 value) {
		value.a = 255;
		return Util.ColorToInt(value).ToString();
	}
	protected override Color32 StringToValue (string str) {
		var result = int.TryParse(str, out int value) ? Util.IntToColor(value) : Color32.CLEAR;
		result.a = 255;
		return result;
	}
}



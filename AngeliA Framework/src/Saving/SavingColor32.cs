namespace AngeliA;

public class SavingColor32 (string key, Color32 defaultValue, SavingLocation location) : Saving<Color32>(key, defaultValue, location) {
	protected override string ValueToString (Color32 value) => Util.ColorToInt(value).ToString();
	protected override Color32 StringToValue (string str) => int.TryParse(str, out int value) ? Util.IntToColor(value) : Color32.CLEAR;
}



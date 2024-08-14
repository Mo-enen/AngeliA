namespace AngeliA;



public class SavingHotkey : Saving<Hotkey> {
	public SavingHotkey (string key, Hotkey defaultValue, SavingLocation location) : base(key, defaultValue, location) { }
	protected override string ValueToString (Hotkey value) => value.GetStringData();
	protected override Hotkey StringToValue (string str) => new(str);
}



public class SavingColor32NoAlpha : Saving<Color32> {
	public SavingColor32NoAlpha (string key, Color32 defaultValue, SavingLocation location) : base(key, defaultValue, location) { }
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



public class SavingColor32 : Saving<Color32> {
	public SavingColor32 (string key, Color32 defaultValue, SavingLocation location) : base(key, defaultValue, location) { }
	protected override string ValueToString (Color32 value) => Util.ColorToInt(value).ToString();
	protected override Color32 StringToValue (string str) => int.TryParse(str, out int value) ? Util.IntToColor(value) : Color32.CLEAR;
}


public class SavingInt : Saving<int> {
	public SavingInt (string key, int defaultValue, SavingLocation location) : base(key, defaultValue, location) { }
	protected override string ValueToString (int value) => value.ToString();
	protected override int StringToValue (string str) => int.TryParse(str, out int value) ? value : 0;
}



public class SavingBool : Saving<bool> {
	public SavingBool (string key, bool defaultValue, SavingLocation location) : base(key, defaultValue, location) { }
	protected override string ValueToString (bool value) => value ? "1" : "0";
	protected override bool StringToValue (string str) => str == "1";
}



public class SavingString : Saving<string> {
	public SavingString (string key, string defaultValue, SavingLocation location) : base(key, defaultValue, location) { }
	protected override string ValueToString (string value) => value;
	protected override string StringToValue (string str) => str;
}



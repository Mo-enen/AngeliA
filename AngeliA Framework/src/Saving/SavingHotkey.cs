namespace AngeliA;

public class SavingHotkey (string key, Hotkey defaultValue, SavingLocation location) : Saving<Hotkey>(key, defaultValue, location) {
	protected override string ValueToString (Hotkey value) => value.GetStringData();
	protected override Hotkey StringToValue (string str) => new(str);
}



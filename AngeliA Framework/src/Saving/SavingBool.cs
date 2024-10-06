namespace AngeliA;

public class SavingBool (string key, bool defaultValue, SavingLocation location) : Saving<bool>(key, defaultValue, location) {
	protected override string ValueToString (bool value) => value ? "1" : "0";
	protected override bool StringToValue (string str) => str == "1";
}



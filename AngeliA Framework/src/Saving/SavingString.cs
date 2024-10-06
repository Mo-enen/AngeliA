namespace AngeliA;



public class SavingString (string key, string defaultValue, SavingLocation location) : Saving<string>(key, defaultValue, location) {
	protected override string ValueToString (string value) => value;
	protected override string StringToValue (string str) => str;
}



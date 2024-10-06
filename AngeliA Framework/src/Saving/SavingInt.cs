namespace AngeliA;

public class SavingInt (string key, int defaultValue, SavingLocation location) : Saving<int>(key, defaultValue, location) {
	protected override string ValueToString (int value) => value.ToString();
	protected override int StringToValue (string str) => int.TryParse(str, out int value) ? value : 0;
}



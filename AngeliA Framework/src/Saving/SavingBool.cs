namespace AngeliA;

/// <summary>
/// Boolean data that auto save into player saving data
/// </summary>
/// <param name="key">Unique key to identify this data</param>
/// <param name="defaultValue"></param>
/// <param name="location">Set to "global" if this data shares between all saving slots</param>
public class SavingBool (string key, bool defaultValue, SavingLocation location) : Saving<bool>(key, defaultValue, location) {
	protected override string ValueToString (bool value) => value ? "1" : "0";
	protected override bool StringToValue (string str) => str == "1";
}



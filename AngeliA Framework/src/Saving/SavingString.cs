namespace AngeliA;


/// <summary>
/// String data that auto save into player saving data
/// </summary>
/// <param name="key">Unique key to identify this data</param>
/// <param name="defaultValue"></param>
/// <param name="location">Set to "global" if this data shares between all saving slots</param>
public class SavingString (string key, string defaultValue, SavingLocation location) : Saving<string>(key, defaultValue, location) {
	protected override string ValueToString (string value) => value;
	protected override string StringToValue (string str) => str;
}


